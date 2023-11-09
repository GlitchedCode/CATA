using System;

public struct Rule
{
    public int Bits { get; private set; } = 0;
    public int KnownBits { get; private set; } = 0;

    public Rule() { }

    public Rule(int bits)
    {
        Bits = bits;
        KnownBits = unchecked((int)0xFFFFFFFF);
    }

    public bool Get(int bit)
    {
        return ((Bits >> bit) & 1) != 0;
    }

    public void Set(int bit, bool value)
    {
        if (value)
            Bits |= (1 << bit);
        else
            Bits &= ~(1 << bit);

        KnownBits |= (1 << bit);
    }

    public bool IsKnown() => KnownBits == 0;

    public bool GetNeighborhoodNext(bool[] neighborhood)
    {
        int bit = 0;
        for (int i = 0; i < 5; ++i)
            if (neighborhood[i])
                bit += (1 << i);

        return Get(bit);
    }

    public static Rule Random(Random rng = null)
    {
        if (rng == null) rng = new Random();
        return new Rule(rng.Next());
    }
}
