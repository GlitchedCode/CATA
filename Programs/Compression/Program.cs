// Compression — demonstrates and benchmarks the CA-based lossless compression algorithm.
//
// Usage:
//   Compression                  run random test + benchmark table
//   Compression --benchmark      print benchmark table only
//   Compression <file>           compress a file and verify round-trip

using Simulation;
using Analysis;

class CompressionProgram
{
    static bool[] GenerateRandomBits(int length, Random rng)
    {
        bool[] ret = new bool[length];
        for (int i = 0; i < length; i++) ret[i] = rng.Next(2) == 1;
        return ret;
    }

    static bool[] GenerateRepetitiveBits(int length, int period = 4)
    {
        // Repeating pattern like 1000 1000 ... — highly compressible
        bool[] pattern = new bool[period];
        pattern[0] = true;
        bool[] ret = new bool[length];
        for (int i = 0; i < length; i++) ret[i] = pattern[i % period];
        return ret;
    }

    static bool[] GenerateAlternatingBits(int length)
    {
        bool[] ret = new bool[length];
        for (int i = 0; i < length; i++) ret[i] = i % 2 == 0;
        return ret;
    }

    static bool[] BytesToBits(byte[] bytes)
    {
        bool[] ret = new bool[bytes.Length * 8];
        for (int i = 0; i < bytes.Length; i++)
            for (int j = 0; j < 8; j++)
            {
                int idx = i * 8 + (7 - j);
                ret[idx] = ((bytes[i] << j) & 128) == 128;
            }
        return ret;
    }

    static byte[] BitsToBytes(bool[] bits)
    {
        byte[] ret = new byte[bits.Length / 8];
        for (int i = 0; i < bits.Length; i++)
            if (bits[i]) ret[i / 8] |= (byte)(1 << (i % 8));
        return ret;
    }

    // Compute approximate compressed size from PrintInfo metrics.
    // Compressed representation = (rule path bits) + (segment count × 32 bits for offsets)
    //                           + (lookback + 1 initial bits)
    static long ApproxCompressedBits(CompressedBuffer buf, int lookback)
        => buf.PathBitCount + (long)buf.SegmentCount * 32 + (lookback + 1);

    // ── Benchmark ─────────────────────────────────────────────────────────────

    static void RunBenchmark()
    {
        Console.WriteLine();
        Console.WriteLine("=== Compression Benchmark ===");
        Console.WriteLine();
        Console.WriteLine($"{"Data type",-30} {"Orig (bits)",12} {"Rules",7} {"Segs",6} {"~Compr (bits)",14} {"Ratio",7}");
        Console.WriteLine(new string('─', 80));

        void Row(string label, bool[] data, int lookback)
        {
            var buf = new CompressedBuffer(data, lookback);
            long orig    = data.Length;
            long compr   = ApproxCompressedBits(buf, lookback);
            double ratio = (double)compr / orig;
            var inflated = buf.Decompress();
            string ok = Enumerable.SequenceEqual(data, inflated) ? "✓" : "✗";
            Console.WriteLine($"{label,-30} {orig,12} {buf.RuleCount,7} {buf.SegmentCount,6} {compr,14} {ratio,7:F2}×  {ok}");
        }

        var rng = new Random(42);

        Row("Random (200 bits,   lb=2)", GenerateRandomBits(200,  rng), 2);
        Row("Random (200 bits,   lb=4)", GenerateRandomBits(200,  rng), 4);
        Row("Alternating 01 (200 bits)", GenerateAlternatingBits(200),  2);
        Row("Periodic 1000 (200 bits)", GenerateRepetitiveBits(200, 4), 2);
        Row("Periodic 1000 (200 bits)", GenerateRepetitiveBits(200, 4), 4);
        Row("Random (1000 bits,  lb=4)", GenerateRandomBits(1000, rng), 4);
        Row("Random (1000 bits,  lb=8)", GenerateRandomBits(1000, rng), 8);

        Console.WriteLine(new string('─', 80));
        Console.WriteLine("Ratio < 1.0 = compression; > 1.0 = expansion.  ✓ = round-trip verified.");
        Console.WriteLine("~Compr = PathBitCount + SegmentCount×32 + (lookback+1) initial bits.");
    }

    // ── Main ──────────────────────────────────────────────────────────────────

    static void Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "--benchmark")
        {
            // Quick random round-trip demo
            var rng = new Random(99);
            bool[] original = GenerateRandomBits(100, rng);
            var buf = new CompressedBuffer(original, lookback: 2);
            var inflated = buf.Decompress();
            Console.WriteLine("=== Random round-trip test (100 bits, lookback 2) ===");
            buf.PrintInfo();
            Console.WriteLine($"Round-trip: {(Enumerable.SequenceEqual(original, inflated) ? "OK" : "FAIL")}");

            RunBenchmark();
        }
        else
        {
            // File mode
            int lookback = 4;
            for (int i = 0; i < args.Length; i++)
                if (args[i] == "--lookback" && i + 1 < args.Length)
                    lookback = int.Parse(args[++i]);

            string filename = args[0];
            var bytes = File.ReadAllBytes(filename);
            bool[] original = BytesToBits(bytes);

            Console.WriteLine($"File: {filename}  ({bytes.Length} bytes = {original.Length} bits)");

            var buf = new CompressedBuffer(original, lookback);
            buf.PrintInfo();

            long compr = ApproxCompressedBits(buf, lookback);
            Console.WriteLine($"Approx compressed: {compr} bits  (ratio: {(double)compr / original.Length:F2}×)");

            var inflated = buf.Decompress();
            Console.WriteLine($"Round-trip: {(Enumerable.SequenceEqual(original, inflated) ? "OK ✓" : "FAIL ✗")}");

            if (Enumerable.SequenceEqual(original, inflated))
            {
                File.WriteAllBytes("out.bin", BitsToBytes(inflated));
                Console.WriteLine("Decompressed written to out.bin");
            }
        }
    }
}
