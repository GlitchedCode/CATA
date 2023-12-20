namespace Simulation;

using System;
using System.Collections.Generic;

public abstract class Neighborhood
{
    public abstract uint Count2D();
    public abstract uint Count1D();

    public abstract State[] Get2D(Container.Grid2D<State>.View state, int row, int column);
    public abstract State[] Get1D(Container.Array<State>.View state, int index);

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

    public static State[] Decode(ConfigurationKey config, int bitCount)
    {
        var buf = config.Bytes;
        var statesPerByte = 8 / bitCount;

        var ret = new State[buf.Length * statesPerByte];
        var stateIdx = 0;

        for (int i = 0; i < buf.Length; i++)
        {
            var bitIdx = 0;
            var leftShift = 0;
            while ((leftShift = 8 - bitIdx - bitCount) >= 0)
            {
                var val = (buf[i] << leftShift);
                val = val >> (8 - bitCount);
                ret[stateIdx] = new State(bitCount, val);
                stateIdx++;
                bitIdx += bitCount;
            }
        }

        return ret;
    }


    public IEnumerable<ConfigurationKey> Enumerate2DConfigurations(int stateCount)
    {

        var bitCount = (int)Math.Ceiling(Math.Log2(stateCount));
        var config = Enumerable.Range(1, (int)Count2D()).Select(_ => new State(bitCount)).ToArray();

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
