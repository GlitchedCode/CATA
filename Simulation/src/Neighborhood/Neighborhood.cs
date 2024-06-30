namespace Simulation;

public abstract class Neighborhood
{
    public uint LookBack = 0;
   
    public abstract uint Count();

    public abstract State[] Get(Container.Array<State>[] states, int index);
    public State[] Get(Container.Array<State> state, int index)
    {
      return Get(new Container.Array<State>[] {state}, index);
    }

    public static ConfigurationKey Encode(State[] configuration)
    {
        return OldNeighborhood.Encode(configuration);
    }

    public static State[] Decode(ConfigurationKey config, int bitCount)
    {
        return OldNeighborhood.Decode(config, bitCount);
    }

    public IEnumerable<ConfigurationKey> EnumerateConfigurationKeys(int stateCount)
    {
        foreach (var c in EnumerateConfigurations(stateCount))
            yield return Encode(c);
        yield break;
    }

    public IEnumerable<State[]> EnumerateConfigurations(int stateCount)
    {
        var bitCount = (int)Math.Ceiling(Math.Log2(stateCount));
        var config = Enumerable.Range(1, (int)Count()).Select(_ => new State(bitCount)).ToArray();

        IEnumerable<State[]> enumerate(int idx)
        {
            if (idx < 0)
            {
                yield return config;
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
