using Simulation;
using Visualization;

// Usage: MetaRules [--rule <number>] [--steps <count>] [--output <path>]
// Generates a hierarchical (meta-rule) CA simulation image.

int ruleNumber = 89;
int steps = 200;
string outputPath = "results/meta89";

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--rule" && i + 1 < args.Length)
    {
        ruleNumber = int.Parse(args[++i]);
        outputPath = $"results/meta{ruleNumber}";
    }
    else if (args[i] == "--steps" && i + 1 < args.Length)
        steps = int.Parse(args[++i]);
    else if (args[i] == "--output" && i + 1 < args.Length)
        outputPath = args[++i];
}

var space = new Simulation.Container.Array<State>(100, new State(1, 0));
var simulation = new Model<Simulation.Container.Array<State>>(space, 10);

var higherOrderRule = new WolframRule(ruleNumber);
var rule = new Model1DRule(higherOrderRule, new Radius1D(2));
rule.Randomize();
simulation.Rule = rule;
simulation.Randomize();

var stepStates = new List<State[]>();
stepStates.Add(simulation.CurrentState.ToArray());
for (int i = 0; i < steps; i++)
{
    simulation.Advance();
    stepStates.Add(simulation.CurrentState.ToArray());
}

Console.WriteLine($"Saving {outputPath}.png ...");
ChartHelper.SaveHeatmap(stepStates, rule.CurrentRule.StatesCount, outputPath);
Console.WriteLine("Done.");
