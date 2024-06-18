using Simulation.Container;
using Simulation;

class CompressedBuffer
{
    readonly CompressionRule rule;
    readonly Array<State>.View[] startingBuffer;
    readonly State sparseDefault = new State(1, 0);
    readonly int generationLength;
    readonly int lookback;


    (int count, TableRule rule) Analyze(List<Array<State>.View> dynamics)
    {
        TableRule rule = new(2, 0);
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

        // convert to sim state
        var dynamics = new List<Array<State>.View>(buffer.Select(b =>
        {
            var ret = new Array<State>(1, sparseDefault);
            ret.Set(0, new State(1, b ? 1 : 0));
            return ret.GetView();
        }));

        startingBuffer = dynamics.Take(lookback + 1).ToArray();
        Dictionary<int, int> offsetTable = new();
        
        // Generate rules
        var offset = 0;
        var rules = new List<TableRule>();
        while (dynamics.Count > lookback + 1)
        {
            var result = Analyze(dynamics);
            rules.Add(result.rule);
            offsetTable[offset] = rules.Count() - 1;
            offset += result.count;
        }

        rules = dedupeRules(rules);

        TableRule master = new(2, 0);
        master.Neighborhood = new Radius1D(0, (uint)lookback);
        List<State[]> ignoredConfigs = new();
        foreach (var rule in rules)
        {
            foreach(var config in rule.EnumerateConfigurations())
            {
                if(ignoredConfigs.Any(c => Enumerable.SequenceEqual(c, config))) continue;

                var dist = rule.Distribution(config);
                var same = true;
                foreach(var other in rules)
                {
                    var otherdist = other.Distribution(config);
                    if(dist[0] != otherdist[0])
                    {
                        ignoredConfigs.Add(config);
                        same = false;
                        break;
                    }
                }

                if(same)
                {
                    master.Set(config, dist[0] == 1 ? 0 : 1);
                    rule.Unset(config);
                }
            }
        }

        rule = new Simulation.CompressionRule(master, rules.ToArray(), offsetTable);
        //rule.Optimize();

        List<TableRule> dedupeRules(List<TableRule> rules)
        {
            var nodupe = new List<TableRule>();
            for (int i = 0; i < rules.Count; i++)
            {
                var dupe = false;
                for (int j = i + 1; j < rules.Count; j++)
                {
                    var r1 = rules[i];
                    var r2 = rules[j];

                    if (r1.Equals(r2))
                    {
                        dupe = true;
                        break;
                    }
                }

                if (!dupe) nodupe.Add(rules[i]);
            }

            resetTable(rules, nodupe);
            return nodupe;
        }

        void resetTable(List<TableRule> orig, List<TableRule> other)
        {
            foreach (var k in offsetTable.Keys)
                for (int i = 0; i < other.Count; i++)
                    if (other[i].Equals(orig[offsetTable[k]]))
                    {
                        offsetTable[k] = i;
                        break;
                    }
        }
    }



    public bool[] Decompress()
    {
        var ret = new List<bool>(startingBuffer.Select(v => v.Get(0).Value == 1));

        var model = new Model1D(1, lookback + 1);
        model.Rule = rule;
        model.ResetHistory(startingBuffer);
        for (int i = 0; i < generationLength; i++)
        {
            model.Advance();
            rule.Advance();
            ret.Add(model.GetCurrentStateView().Get(0).Value == 1);
        }

        return ret.ToArray();
    }

    public void PrintInfo()
    {
        Console.WriteLine($"lookback: {lookback}");
        Console.WriteLine($"original size: {generationLength + lookback + 1}");
        Console.WriteLine($"rule count: {rule.RuleCount}");
        Console.WriteLine($"segment count: {rule.SegmentCount}");
        Console.WriteLine($"average segment length: {rule.AverageSegmentLength}");
        Console.WriteLine($"master configs count: {rule.MasterConfigsCount}");

        
        Console.WriteLine($"approx rules bit count: {rule.PathBitCount}");
        Console.WriteLine($"approx segment bit count: {rule.SegmentCount * 32}");

        // Console.WriteLine("rules:");
        // foreach(var rule in rule.Rules)
        // {
        //     Console.WriteLine(
        //         String.Join("", rule.GetBits())
        //     );
        // }
        
    }
}
