using Simulation;
using Visualization;

// Usage: WolframDemo [--rule <number>] [--steps <count>] [--cells <count>] [--output <path>]
// Generates a 1D Wolfram elementary CA simulation image.

int ruleNumber = 30;
int steps = 300;
int cells = 300;
string outputPath = $"results/wolfram_30";

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--rule" && i + 1 < args.Length)
    {
        ruleNumber = int.Parse(args[++i]);
        outputPath = $"results/wolfram_{ruleNumber}";
    }
    else if (args[i] == "--steps" && i + 1 < args.Length)
        steps = int.Parse(args[++i]);
    else if (args[i] == "--cells" && i + 1 < args.Length)
        cells = int.Parse(args[++i]);
    else if (args[i] == "--output" && i + 1 < args.Length)
        outputPath = args[++i];
}

var space = new Simulation.Container.Array<State>(cells, new State(1, 0));
var simulation = new Model<Simulation.Container.Array<State>>(space);
var rule = new WolframRule(ruleNumber);
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
ChartHelper.SaveHeatmap(stepStates, rule.StatesCount, outputPath);
Console.WriteLine("Done.");
