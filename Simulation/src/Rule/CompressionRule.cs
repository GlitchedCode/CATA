using System.Dynamic;

namespace Simulation;

public class CompressionRule : MetaRule
{
    readonly TableRule[] rules;
    readonly Dictionary<int, int> offsetTable = new();

    public int RuleCount { get => rules.Length; }
    public readonly TableRule MasterRule;
    public int MasterConfigsCount { get => MasterRule.EnumerateConfigurations().Count(); }
    public int SegmentCount { get => offsetTable.Count; }
    public int PathBitCount
    {
        get
        {
            var ret = 0;
            foreach (var rule in rules)
                foreach (var path in rule.EnumerateConfigurations())
                    ret += path.Length;

            foreach (var path in MasterRule.EnumerateConfigurations())
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

    public IEnumerable<TableRule> Rules { get => rules; }

    int ruleIndex = -1;
    int currentIndex = -1;

    public CompressionRule(TableRule master, TableRule[] rules, Dictionary<int, int> offsetTable)
    {
        this.MasterRule = master;
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


    public sealed override Rule GetCurrentRule(int position)
        => rules[ruleIndex];

    public void Optimize()
    {
        MasterRule.Optimize();
        foreach (var rule in rules) rule.Optimize();
    }


}
