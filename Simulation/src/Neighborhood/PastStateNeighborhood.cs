namespace Simulation;

public abstract class PastStateNeighborhood
{
    public abstract uint Count1D();

    public abstract State[] Get1D(Container.Array<State>.View[] states, int index);

    public static ConfigurationKey Encode(State[] configuration)
    {
        return Neighborhood.Encode(configuration);
    }

    public static State[] Decode(ConfigurationKey config, int bitCount)
    {
        return Neighborhood.Decode(config, bitCount);
    }

    public IEnumerable<ConfigurationKey> EnumerateConfigurations(int stateCount)
    {
        var bitCount = (int)Math.Ceiling(Math.Log2(stateCount));
        var config = Enumerable.Range(1, (int)Count1D()).Select(_ => new State(bitCount)).ToArray();

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
