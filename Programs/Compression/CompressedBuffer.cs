using System.Collections.Generic;

using Simulation;
using Simulation.Container;

class CompressedBuffer
{
    readonly Dictionary<int, SingleRule> rules = new();
    readonly Array<State>.View[] startingBuffer;
    readonly State sparseDefault = new State(1, 0);
    readonly int generationLength;
    readonly int lookback;


    (int count, SingleRule rule) Analyze(List<Array<State>.View> dynamics)
    {
        SingleRule rule = new(2, 0);
        rule.Neighborhood = new Radius1D(0, (uint)lookback);

        int count = 0;
        while (dynamics.Count > lookback + 1)
        {
            var neighborhood = rule.Neighborhood
                .Get(dynamics.Take(lookback + 1).ToArray(), 0);
            var expected = dynamics[lookback + 1].Get(0).Value;

            if (rule.Contains(neighborhood))
            {
                var dist = rule.Distribution(neighborhood);
                if (dist[expected] == 0)
                    break;
            }

            rule.Set(neighborhood, expected);
            dynamics.RemoveAt(0);
            count++;
        }

        return (count, rule);
    }

    public CompressedBuffer(bool[] buffer, int lookback = 4)
    {
        this.lookback = lookback;
        generationLength = buffer.Length - lookback - 1;
        if (generationLength < 0)
            throw new Exception("invalid params");

        var dynamics = new List<Array<State>.View>(buffer.Select(b =>
        {
            var ret = new Array<State>(1, sparseDefault);
            ret.Set(0, new State(1, b ? 1 : 0));
            return ret.GetView();
        }));

        startingBuffer = dynamics.Take(lookback + 1).ToArray();

        var offset = 0;
        while (dynamics.Count > lookback + 1)
        {
            var result = Analyze(dynamics);
            rules[offset] = result.rule;
            offset += result.count;
        }
    }

    public bool[] Decompress()
    {
        var ret = new List<bool>(startingBuffer.Select(v => v.Get(0).Value == 1));

        var model = new Model1D(1, lookback + 1);
        model.ResetHistory(startingBuffer);
        for (int i = 0; i < generationLength; i++)
        {
            if (rules.ContainsKey(i))
                model.Rule = rules[i];

            model.Advance();
            ret.Add(model.GetCurrentStateView().Get(0).Value == 1);
        }

        return ret.ToArray();
    }
}