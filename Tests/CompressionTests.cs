namespace Test;

using Analysis;

public class CompressionTests
{
    // ── Round-trip fidelity ───────────────────────────────────────────────────

    [Fact]
    public void Compress_Decompress_SmallKnownSequence()
    {
        bool[] original = { true, false, true, true, false, true, false, false, true, true };
        var buf = new CompressedBuffer(original, lookback: 2);
        var inflated = buf.Decompress();
        Assert.Equal(original, inflated);
    }

    [Theory]
    [InlineData(50, 1)]
    [InlineData(50, 2)]
    [InlineData(100, 3)]
    [InlineData(200, 4)]
    public void Compress_Decompress_RandomSequence(int length, int lookback)
    {
        var rng = new Random(42);
        bool[] original = Enumerable.Range(0, length)
            .Select(_ => rng.Next(2) == 1)
            .ToArray();

        var buf = new CompressedBuffer(original, lookback);
        var inflated = buf.Decompress();

        Assert.Equal(original, inflated);
    }

    [Fact]
    public void Compress_AllZeros_RoundTrips()
    {
        bool[] original = new bool[64];   // all false
        var buf = new CompressedBuffer(original, lookback: 2);
        var inflated = buf.Decompress();
        Assert.Equal(original, inflated);
    }

    [Fact]
    public void Compress_AllOnes_RoundTrips()
    {
        bool[] original = Enumerable.Repeat(true, 64).ToArray();
        var buf = new CompressedBuffer(original, lookback: 2);
        var inflated = buf.Decompress();
        Assert.Equal(original, inflated);
    }

    [Fact]
    public void Compress_AlternatingPattern_RoundTrips()
    {
        bool[] original = Enumerable.Range(0, 80).Select(i => i % 2 == 0).ToArray();
        var buf = new CompressedBuffer(original, lookback: 2);
        var inflated = buf.Decompress();
        Assert.Equal(original, inflated);
    }

    // ── Guard: invalid params ─────────────────────────────────────────────────

    [Fact]
    public void Compress_TooShortForLookback_Throws()
    {
        bool[] original = { true, false };   // length 2, lookback 4 → generationLength < 0
        Assert.Throws<Exception>(() => new CompressedBuffer(original, lookback: 4));
    }
}
