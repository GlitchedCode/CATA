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


public abstract class Neighborhood<T>
{
    public abstract uint Count();
    public abstract T[] Get(Grid2DView<T> state, int row, int column);

    public abstract ConfigurationKey ConvertKey(ConfigurationKey input);

    public ConfigurationKey Encode(T[] configuration)
    {
        if (configuration is bool[])
            return EncodeBool(configuration as bool[]);

        return null;
    }

    public static ConfigurationKey EncodeBool(bool[] configuration)
    {
        byte[] buf = new byte[(configuration.Length / 8) + 1];
        for (int i = 0; i < buf.Length; i++)
            buf[i] = 0;

        for (int i = 0; i < configuration.Length; i++)
        {
            if (configuration[i])
            {
                int bufIdx = i / 8;
                int bitIdx = i % 8;

                buf[bufIdx] |= (byte)(1 << bitIdx);
            }
        }

        return new ConfigurationKey(buf);
    }

    public IEnumerable<ConfigurationKey> EnumerateConfigurations()
    {
        if (this is Neighborhood<bool>)
        {
            foreach (var k in EnumerateBooleanConfigurations(this as Neighborhood<bool>))
                yield return k;
        }


        yield break;
    }

    public static IEnumerable<ConfigurationKey> EnumerateBooleanConfigurations(Neighborhood<bool> neighborhood)
    {
        var config = new bool[neighborhood.Count()];

        IEnumerable<ConfigurationKey> enumerate(int idx)
        {
            if (idx < 0)
            {
                yield return neighborhood.Encode(config);
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


public class VonNeumann<T> : Neighborhood<T>
{
    public uint Radius = 1;
    public T DefaultValue;

    public VonNeumann(T defaultValue, uint radius = 1)
    {
        this.DefaultValue = defaultValue;
        this.Radius = radius;
    }

    public override uint Count()
    {
        uint ret = 1 + (Radius * 4);
        for (uint i = 1; i < Radius; i++) ret += i * 4;
        return ret;
    }

    public override T[] Get(Grid2DView<T> state, int row, int column)
    {
        var count = Count();
        T[] neighborhood = new T[count];

        neighborhood[0] = state.Get(row, column);
        int idx = 1;

        for (int i = 1; i <= Radius; i++)
            for (int j = 0; j < i; j++)
            {
                var r = -i + j;
                var c = j;
                neighborhood[idx] = state.Get(row + r, column + c);
                idx++;

                neighborhood[idx] = state.Get(row + c, column - r);
                idx++;

                neighborhood[idx] = state.Get(row - r, column - c);
                idx++;

                neighborhood[idx] = state.Get(row - c, column + r);
                idx++;
            }


        return neighborhood;
    }

    public T[] Convert(T[] input, T defaultValue)
    {
        var count = Count();
        T[] ret = Enumerable.Repeat(defaultValue, (int)count).ToArray();
        Array.Copy(input, 0, ret, 0, input.Length);
        return ret;
    }

    public override ConfigurationKey ConvertKey(ConfigurationKey input)
    {
        var config = Enumerable.Repeat(DefaultValue, (int)Count()).ToArray();
        var ret = Encode(config);

        Array.Copy(input.Bytes, 0, ret.Bytes, 0, Math.Min(input.Bytes.Length, ret.Bytes.Length));

        return ret;
    }
}
