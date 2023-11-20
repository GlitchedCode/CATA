namespace Simulation;

using System;

public struct State : ICloneable
{
    byte __value = 0;
    public int Value
    {
        get => __value;
        set
        {
            var shift = 8 - BitsCount;
            var val = value << shift;
            __value = (byte)(val >> shift);
        }
    }

    byte __bitsCount = 1;
    public int BitsCount
    {
        get => __bitsCount;
        set
        {
            if (value < 1 || value > 8)
                throw new Exception("bits should be between 1 and 8 inclusive");

            __bitsCount = (byte)value;
        }
    }

    public State(int bitsCount, int value = 0)
    {
        BitsCount = bitsCount;
        Value = value;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
