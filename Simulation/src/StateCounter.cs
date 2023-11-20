namespace Simulation;

using System;
using System.Collections.Generic;

public class StateCounter
{
    uint[] counters;
    Random rng = new();

    public StateCounter(int statesCount, Random rng = null)
    {
        counters = new uint[statesCount];

        if (rng != null) this.rng = rng;
        Reset();
    }

    public void Increment(int state, uint amount = 1)
    {
        if (state >= counters.Length || state < 0)
            throw new Exception("Invalid state");
        counters[state] += amount;
    }

    public void Reset()
    {
        for (int i = 0; i < counters.Length; i++)
            counters[i] = 0;
    }

    public void Set(int state)
    {
        if (state >= counters.Length || state < 0)
            throw new Exception("Invalid state");
        Reset();
        counters[state] = 1;
    }

    public int Get()
    {
        var dist = Distribution();
        var rand = rng.NextDouble();

        var total = 0d;
        for (int i = 0; i < counters.Length; i++)
        {
            total += dist[i];
            if (rand <= total)
                return i;
        }

        return counters.Length - 1;
    }

    public double[] Distribution()
    {
        var ret = new double[counters.Length];

        double total = 0d;
        foreach (var c in counters)
            total += c;

        if (total == 0)
            throw new Exception("you set nothing here");

        for (int i = 0; i < counters.Length; i++)
            ret[i] = (double)counters[i] / total;

        return ret;
    }

    public double Variance()
    {
        var total = 0d;
        foreach (var c in counters)
            total += c;

        var t1 = 0d;
        for (int i = 1; i < counters.Length; i++)
            t1 += Math.Pow(i * counters[i], 2);

        t1 /= total;

        var t2 = 0d;
        for (int i = 1; i < counters.Length; i++)
            t2 += i * counters[i];

        t2 /= total;
        t2 = Math.Pow(t2, 2);

        return t1 - t2;
    }

}

public class BinaryStateCounter
{
    uint[] counters = new uint[2];
    Random rng = new();

    public BinaryStateCounter(Random rng = null)
    {
        if (rng != null) this.rng = rng;
        Reset();
    }

    public void Increment(bool state, uint amount = 1)
    {
        counters[state ? 0 : 1] += amount;
    }

    public void Reset()
    {
        counters[0] = 0;
        counters[1] = 0;
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
        double total = counters[0] + counters[1];

        if (total == 0)
            throw new Exception("you set nothing here");

        var ratio = (double)counters[0] / total;
        return new double[] { ratio, 1 - ratio };
    }

    public double Variance()
    {
        var t1 = counters[0] / (double)(counters[0] + counters[1]);
        var t2 = Math.Pow(t1, 2d);
        return t1 - t2;
    }
}
