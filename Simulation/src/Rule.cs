namespace Simulation;

using System;
using System.Collections.Generic;


public interface StateCounter<T>
{
    public void Increment(T state, uint amount);
    public void Set(T state);
    public void Reset();

    public T Get();
    public double[] Distribution();
    public double Variance();
}

public class BinaryStateCounter : StateCounter<bool>
{
    uint[] counters = new uint[2];
    Random rng = new();

    public BinaryStateCounter(bool defaultValue = false, Random rng = null)
    {
        if (rng != null) this.rng = rng;
        Set(defaultValue);
    }

    public void Increment(bool state, uint amount = 1)
    {
        counters[state ? 0 : 1] += amount;
    }

    public void Reset()
    {
        counters[0] = 0;
        counters[1] = 1;
    }

    public void Set(bool state)
    {
        counters[state ? 0 : 1] = 1;
        counters[state ? 1 : 0] = 0;
    }

    public bool Get()
    {
        return rng.NextDouble() < Distribution()[0];
    }

    public double[] Distribution()
    {
        var ratio = (double)counters[0] / (double)(counters[0] + counters[1]);
        return new double[] { ratio, 1 - ratio };
    }

    public uint[] Values()
    {
        return (uint[])counters.Clone();
    }

    public double Variance()
    {
        return 0;
    }
}


public struct Rule
{
    Dictionary<ConfigurationKey, BinaryStateCounter> stateTable = new();
    public bool DefaultValue = false;

    private Neighborhood __neighborhood = new VonNeumann();
    public Neighborhood Neighborhood
    {
        get => __neighborhood;
        set
        {
            if (value == null) throw new NullReferenceException();
            __neighborhood = value;
        }
    }

    public IEnumerable<ConfigurationKey> ConfigurationKeys
    {
        get => stateTable.Keys;
    }

    Random rng = new();

    public Rule() { }


    void EnsureKey(ConfigurationKey key)
    {
        if (!stateTable.ContainsKey(key))
            stateTable[key] = new(DefaultValue, rng);
    }

    public void Increment(ConfigurationKey configurationKey, bool value, uint amount = 1)
    {
        EnsureKey(configurationKey);
        stateTable[configurationKey].Increment(value, amount);
    }

    public void Set(ConfigurationKey configurationKey, bool value)
    {
        EnsureKey(configurationKey);
        stateTable[configurationKey].Set(value);
    }

    public void Unset(ConfigurationKey configurationKey)
    {
        stateTable.Remove(configurationKey);
    }

    public void Reset()
    {
        stateTable.Clear();
    }

    public bool Get(ConfigurationKey configurationKey)
    {
        if (stateTable.ContainsKey(configurationKey))
            return stateTable[configurationKey].Get();
        return DefaultValue;
    }

    public double[] Distribution(ConfigurationKey configurationKey)
    {
        if (stateTable.ContainsKey(configurationKey))
            return stateTable[configurationKey].Distribution();

        return new double[] {
            DefaultValue ? 1 : 0,
            !DefaultValue ? 1 : 0,
        };
    }

    public double Variance() => 0;

    public double Difference(Rule other)
    {
        var ret = 0d;

        foreach (var key in ConfigurationKeys)
        {
            var thisDist = Distribution(key);
            var otherDist = other.Distribution(key);

            ret += Math.Abs(thisDist[0] - otherDist[0]);
            ret += Math.Abs(thisDist[1] - otherDist[1]);
        }

        return ret;
    }

    public static Rule Random(Neighborhood neighborhood = null, Random rng = null)
    {
        if (rng == null) rng = new Random();
        if (neighborhood == null) neighborhood = new VonNeumann();

        var ret = new Rule();
        ret.Neighborhood = neighborhood;

        foreach (var config in Neighborhood.EnumerateBooleanConfigurations(neighborhood))
        {
            if (rng.Next(2) == 0)
                ret.Set(config, rng.Next(2) == 0);
            else
            {
                uint deadCount = (uint)rng.Next() % 5000;
                uint aliveCount = (uint)rng.Next() % 5000;
                ret.Increment(config, false, deadCount);
                ret.Increment(config, true, aliveCount);
            }
        }


        return ret;
    }
}
