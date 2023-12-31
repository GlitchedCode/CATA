namespace Simulation;

using System;

public struct CompactRule
{
    public uint Bits { get; private set; } = 0;
    public uint KnownBits { get; private set; } = 0;

    public Neighborhood Neighborhood { get; private set; } = new VonNeumann();

    public CompactRule() { }

    public CompactRule(uint bits)
    {
        Bits = bits;
        KnownBits = 0xFFFFFFFF;
    }

    public bool Get(int bit)
    {
        return ((Bits >> bit) & 1) != 0;
    }

    public void Set(int bit, bool value)
    {
        var mask = (1u << bit);
        if (value)
            Bits |= mask;
        else
            Bits &= ~mask;

        KnownBits |= mask;
    }

    public bool IsBitKnown(int bit) => ((KnownBits >> bit) & 1) != 0;

    public bool IsKnown() => KnownBits == 0xFFFFFFFF;

    public int GetNeighborhoodBit(bool[] neighborhood)
    {

        int bit = 0;
        for (int i = 0; i < 5; ++i)
            if (neighborhood[i])
                bit += (1 << i);

        return bit;
    }

    public bool GetNeighborhoodNext(bool[] neighborhood)
    {
        return Get(GetNeighborhoodBit(neighborhood));
    }

    public static CompactRule Random(Random rng = null)
    {
        if (rng == null) rng = new Random();
        return new CompactRule((uint)rng.Next());
    }
}
