using System.Dynamic;

namespace Simulation;

public class CompressionRule : MetaRule
{
    readonly SingleRule master;
    readonly SingleRule[] rules;
    readonly Dictionary<int, int> offsetTable = new();

    public int RuleCount { get => rules.Length; }
    public int MasterConfigsCount { get => master.EnumerateConfigurations().Count(); }
    public int SegmentCount { get => offsetTable.Count; }
    public int PathBitCount
    {
        get
        {
            var ret = 0;
            foreach (var rule in rules)
                foreach (var path in rule.EnumerateConfigurations())
                    ret += path.Length;

            foreach (var path in master.EnumerateConfigurations())
                ret += path.Length;
            return ret;
        }
    }

    public double AverageSegmentLength
    {
        get
        {
            var acc = 0d;
            var count = 0;

            var last = offsetTable.Keys.First();
            foreach (var offset in offsetTable.Keys.Skip(1))
            {
                acc += offset - last;
                last = offset;
                count++;
            }

            return acc / count;
        }
    }

    public IEnumerable<SingleRule> Rules { get => rules; }

    SingleRule CurrentRule { get => rules[ruleIndex]; }
    int ruleIndex = -1;
    int currentIndex = -1;

    public CompressionRule(SingleRule master, SingleRule[] rules, Dictionary<int, int> offsetTable)
    {
        this.master = master;
        this.rules = rules;
        this.offsetTable = offsetTable;
        Advance();
    }

    public override void Advance()
    {
        currentIndex++;
        if (offsetTable.ContainsKey(currentIndex))
            ruleIndex = offsetTable[currentIndex];
    }

    public override int GetDefaultState() => CurrentRule.GetDefaultState();
    public override void SetDefaultState(int v) => CurrentRule.SetDefaultState(v);
    public override int GetStatesCount() => CurrentRule.GetStatesCount();
    public override void SetStatesCount(int v) => CurrentRule.SetStatesCount(v);
    public override int GetBitsCount() => CurrentRule.GetBitsCount();
    public override void SetBitsCount(int v) => CurrentRule.SetBitsCount(v);
    public override Neighborhood GetNeighborhood() => CurrentRule.GetNeighborhood();
    public override void SetNeighborhood(Neighborhood v) => CurrentRule.SetNeighborhood(v);
    public override IEnumerable<State[]> EnumerateConfigurations()
        => CurrentRule.EnumerateConfigurations().Union(master.EnumerateConfigurations());

    Rule getRule(State[] config)
        => CurrentRule.Contains(config) ? CurrentRule : master;

    public override State Get(State[] config)
        => CurrentRule.Get(config);

    public override double[] Distribution(State[] config) => getRule(config).Distribution(config);

    public override double AverageDifference(SingleRule other)
        => CurrentRule.AverageDifference(other) + master.AverageDifference(other);

    public override double AverageVariance()
        => CurrentRule.AverageVariance() + master.AverageVariance();


    public void Optimize()
    {
        master.Optimize();
        foreach (var rule in rules) rule.Optimize();
    }


}
