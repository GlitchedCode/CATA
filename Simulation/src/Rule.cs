namespace Simulation;

using System;
using System.Collections.Generic;

public class Rule : ICloneable
{
    Dictionary<ConfigurationKey, StateCounter> stateTable = new();
    public int DefaultState;
    public int StatesCount { get; protected set; }
    public int BitsCount { get; protected set; }

    private Neighborhood __neighborhood;
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

    public Rule(int statesCount, int defaultState = 0)
    {
        DefaultState = defaultState;
        StatesCount = statesCount;
        BitsCount = (int)Math.Ceiling(Math.Log2(StatesCount));
        __neighborhood = new VonNeumann();
    }

    void EnsureKey(ConfigurationKey key)
    {
        if (!stateTable.ContainsKey(key))
            stateTable[key] = new(StatesCount, rng);
    }

    public void Increment(ConfigurationKey configurationKey, int stateValue, uint amount = 1)
    {
        EnsureKey(configurationKey);
        stateTable[configurationKey].Increment(stateValue, amount);
    }

    public void Set(ConfigurationKey configurationKey, int stateValue)
    {
        EnsureKey(configurationKey);
        stateTable[configurationKey].Set(stateValue);
    }

    public void Unset(ConfigurationKey configurationKey)
    {
        stateTable.Remove(configurationKey);
    }

    public void Reset()
    {
        stateTable.Clear();
    }

    public State Get(ConfigurationKey configurationKey)
    {
        int state;
        if (stateTable.ContainsKey(configurationKey))
            state = stateTable[configurationKey].Get();
        else
            state = DefaultState;

        return new State(BitsCount, state);
    }

    public static Rule operator +(Rule a, Rule b)
    {
        var ret = (Rule)a.Clone();

        foreach (var k in b.stateTable.Keys)
        {
            var counter = b.stateTable[k];
            for (int i = 0; i < ret.StatesCount; i++)
                ret.Increment(k, i, counter[i]);
        }

        return ret;
    }

    public double[] Distribution(ConfigurationKey configurationKey)
    {
        if (stateTable.ContainsKey(configurationKey))
            return stateTable[configurationKey].Distribution();

        return new double[] { 1 };
    }

    public double Variance(ConfigurationKey configurationKey)
    {
        if (stateTable.ContainsKey(configurationKey))
            return stateTable[configurationKey].Variance();

        return 0d;
    }

    public double AverageDifference(Rule other)
    {
        var ret = 0d;
        var count = 0d;

        foreach (var key in ConfigurationKeys)
        {
            var thisDist = Distribution(key);

            var otherKey = other.Neighborhood.ConvertKey(
                key, new State(BitsCount, DefaultState));
            var otherDist = other.Distribution(otherKey);

            ret += Math.Abs(thisDist[0] - otherDist[0]);
            ret += Math.Abs(thisDist[1] - otherDist[1]);

            count += 1d;
        }

        return ret / count;
    }

    public double AverageVariance()
    {
        var ret = 0d;
        var count = 0d;

        foreach (var key in ConfigurationKeys)
        {
            ret += Variance(key);
            count += 1d;
        }

        return ret / count;
    }

    public object Clone()
    {
        var ret = new Rule(StatesCount, DefaultState);
        ret.Neighborhood = Neighborhood;
        foreach (var k in stateTable.Keys)
        {
            var counter = stateTable[k];
            for (int i = 0; i < StatesCount; i++)
                ret.Increment(k, i, counter[i]);
        }
        return ret;
    }

}


public class RandomRule
{
    public static Rule Make(Neighborhood neighborhood, int stateCount, Random rng = null)
    {
        if (rng == null) rng = new Random();

        var ret = new Rule(stateCount);
        ret.Neighborhood = neighborhood;

        foreach (var config in neighborhood.EnumerateConfigurations(stateCount))
        {
            if (rng.Next(2) == 0)
                ret.Set(config, rng.Next(stateCount));
            else
                for (int i = 0; i < stateCount; i++)
                    ret.Increment(config, i, (uint)rng.Next() % 5000);
        }


        return ret;
    }
}
