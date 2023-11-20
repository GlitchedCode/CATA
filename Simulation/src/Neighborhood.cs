namespace Simulation;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public class ConfigurationKey
{
    public byte[] Bytes;

    public ConfigurationKey(byte[] bytes = null)
    {
        if (bytes == null) bytes = new byte[] { };
        this.Bytes = bytes;
    }

    public override int GetHashCode()
    {
        using (var md5 = MD5.Create())
        {
            var ret = BitConverter.ToInt32(md5.ComputeHash(Bytes), 0);
            return ret;
        }

        //return BitConverter.ToInt32(Bytes, 0);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is ConfigurationKey)) return false;
        var other = (ConfigurationKey)obj;

        if (Bytes.Length != other.Bytes.Length) return false;

        for (int i = 0; i < Bytes.Length; i++)
            if (Bytes[i] != other.Bytes[i])
                return false;

        return true;
    }
}


public abstract class Neighborhood
{
    public abstract uint Count();
    public abstract State[] Get(Grid2DView<State> state, int row, int column);

    public abstract ConfigurationKey ConvertKey(ConfigurationKey input, State defaultState);

    public static ConfigurationKey Encode(State[] configuration)
    {
        var bitCount = configuration[0].BitsCount;
        var statesPerByte = 8 / bitCount;

        byte[] buf = new byte[(configuration.Length / statesPerByte) + 1];
        for (int i = 0; i < buf.Length; i++)
            buf[i] = 0;

        for (int i = 0; i < configuration.Length; i++)
        {
            var bufIdx = i / statesPerByte;
            var shift = (i % statesPerByte) * bitCount;

            var val = configuration[i].Value;
            buf[bufIdx] |= (byte)(val << shift);
        }

        return new ConfigurationKey(buf);
    }

    public IEnumerable<ConfigurationKey> EnumerateConfigurations(int stateCount)
    {

        var bitCount = (int)Math.Ceiling(Math.Log2(stateCount));
        var config = Enumerable.Range(1, (int)Count()).Select(_ => new State(bitCount)).ToArray();

        IEnumerable<ConfigurationKey> enumerate(int idx)
        {
            if (idx < 0)
            {
                yield return Neighborhood.Encode(config);
                yield break;
            }

            for (int s = 0; s < stateCount; s++)
            {
                config[idx].Value = s;
                foreach (var c in enumerate(idx - 1))
                    yield return c;
            }

            yield break;
        }

        foreach (var c in enumerate(config.Length - 1))
            yield return c;
        yield break;
    }
}


public class VonNeumann : Neighborhood
{
    public uint Radius = 1;

    public VonNeumann(uint radius = 1)
    {
        this.Radius = radius;
    }

    public override uint Count()
    {
        uint ret = 1 + (Radius * 4);
        for (uint i = 1; i < Radius; i++) ret += i * 4;
        return ret;
    }

    public override State[] Get(Grid2DView<State> gridState, int row, int column)
    {
        var count = Count();
        State[] configuration = new State[count];

        configuration[0] = gridState.Get(row, column);
        int idx = 1;

        for (int i = 1; i <= Radius; i++)
            for (int j = 0; j < i; j++)
            {
                var r = -i + j;
                var c = j;
                configuration[idx] = gridState.Get(row + r, column + c);
                idx++;

                configuration[idx] = gridState.Get(row + c, column - r);
                idx++;

                configuration[idx] = gridState.Get(row - r, column - c);
                idx++;

                configuration[idx] = gridState.Get(row - c, column + r);
                idx++;
            }


        return configuration;
    }

    public State[] Convert(State[] input, State defaultValue)
    {
        var count = Count();
        var ret = Enumerable.Range(1, (int)Count()).Select(_ => (State)defaultValue.Clone()).ToArray();
        Array.Copy(input, 0, ret, 0, input.Length);
        return ret;
    }

    public override ConfigurationKey ConvertKey(ConfigurationKey input, State defaultValue)
    {
        var config = Enumerable.Range(1, (int)Count()).Select(_ => (State)defaultValue.Clone()).ToArray();
        var ret = Encode(config);

        Array.Copy(input.Bytes, 0, ret.Bytes, 0, Math.Min(input.Bytes.Length, ret.Bytes.Length));

        return ret;
    }
}
