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
    public abstract T[] Get<T>(Grid2DView<T> state, int row, int column);

    public static ConfigurationKey Encode(bool[] neighborhood)
    {
        byte[] buf = new byte[(neighborhood.Length / 8) + 1];
        for (int i = 0; i < buf.Length; i++)
            buf[i] = 0;

        for (int i = 0; i < neighborhood.Length; i++)
        {
            if (neighborhood[i])
            {
                int bufIdx = i / 8;
                int bitIdx = i % 8;

                buf[bufIdx] |= (byte)(1 << bitIdx);
            }
        }

        return new ConfigurationKey(buf);
    }

    public static IEnumerable<ConfigurationKey> EnumerateBooleanConfigurations(Neighborhood neighborhood)
    {
        var config = new bool[neighborhood.Count()];

        IEnumerable<ConfigurationKey> enumerate(int idx)
        {
            if (idx < 0)
            {
                yield return Encode(config);
                yield break;
            }

            config[idx] = false;
            foreach (var c in enumerate(idx - 1))
                yield return c;

            config[idx] = true;
            foreach (var c in enumerate(idx - 1))
                yield return c;

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

    public VonNeumann(uint radius = 1) => this.Radius = radius;

    public override uint Count()
    {
        uint ret = 1 + (Radius * 4);
        for (uint i = 1; i < Radius; i++) ret += i;
        return ret;
    }

    public override T[] Get<T>(Grid2DView<T> state, int row, int column)
    {
        var count = Count();
        T[] neighborhood = new T[count];

        neighborhood[0] = state.Get(row, column);
        int idx = 1;
        // cross
        for (int i = 1; i <= Radius; i++)
        {
            neighborhood[idx] = state.Get(row + i, column);
            idx++;
            neighborhood[idx] = state.Get(row - i, column);
            idx++;
            neighborhood[idx] = state.Get(row, column + i);
            idx++;
            neighborhood[idx] = state.Get(row, column - i);
            idx++;
        }

        // quadrants
        for (int c = 1; c < Radius; c++)
            for (int r = c - (int)Radius; r < 0; r++)
            {
                neighborhood[idx] = state.Get(row + r, column + c);
                idx++;
                neighborhood[idx] = state.Get(row + r, column - c);
                idx++;
                neighborhood[idx] = state.Get(row - r, column + c);
                idx++;
                neighborhood[idx] = state.Get(row - r, column - c);
                idx++;
            }

        return neighborhood;
    }
}

