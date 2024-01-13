using System.Reflection.Metadata;

class Program
{
    static bool[] GenerateRandomBoolString(int length)
    {
        Random rng = new();
        bool[] ret = new bool[length];
        for (int i = 0; i < length; i++) ret[i] = rng.Next() % 2 == 0;
        return ret;
    }

    static bool[] BytesToBoolString(byte[] bytes)
    {
        bool[] ret = new bool[bytes.Length * 8];
        for (int i = 0; i < bytes.Length; i++)
            for (int j = 0; j < 8; j++)
            {
                var word = bytes[i];
                var idx = (i * 8) + (7 - j);
                var shifted = word << j;
                ret[idx] = (shifted & 128) == 128;
            }
        return ret;
    }

    static byte[] BoolStringToBytes(bool[] bits)
    {
        byte[] ret = Enumerable.Repeat<byte>(0, bits.Length / 8).ToArray();
        for (int i = 0; i < bits.Length; i++)
        {
            int idx = i / 8;
            int bit = (i % 8);
            if (bits[i])
                ret[idx] |= (byte)(1 << bit);
        }
        return ret;
    }

    static void PrintBoolString(bool[] input)
    {
        Console.WriteLine(
            String.Join("", input.Select(b => b ? 1 : 0))
        );
    }

    static void Main(string[] args)
    {
        if (args.Length < 1)
            TestRand();
        else
            TestFile(args[0]);
    }

    static void TestFile(string filename)
    {
        int lookback = 4;

        var bytes = System.IO.File.ReadAllBytes(filename);
        bool[] original = BytesToBoolString(bytes);

        var compressed = new CompressedBuffer(original, lookback);
        compressed.PrintInfo();

        var inflated = compressed.Decompress();
        if (Enumerable.SequenceEqual(original, inflated))
            Console.WriteLine("they the same");

        var file = File.Open("out", FileMode.Create, FileAccess.Write);
        file.Write(BoolStringToBytes(inflated));
    }

    static void TestRand()
    {
        int length = 100;
        int lookback = 2;
        bool[] original = GenerateRandomBoolString(length);

        Console.WriteLine("original");
        PrintBoolString(original);

        var compressed = new CompressedBuffer(original, lookback);

        compressed.PrintInfo();

        var inflated = compressed.Decompress();
        Console.WriteLine("inflated");
        PrintBoolString(inflated);

        if (Enumerable.SequenceEqual(original, inflated))
            Console.WriteLine("they the same");

    }
}