using Simulation;
using Simulation.Container;
using Visualization;

// Usage: Plot2D [--steps <count>] [--size <n>] [--output <path>]
// Generates a Game of Life simulation image (last step mosaic).

int steps = 100;
int size = 100;
string outputPath = "results/gol";

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--steps" && i + 1 < args.Length)
        steps = int.Parse(args[++i]);
    else if (args[i] == "--size" && i + 1 < args.Length)
        size = int.Parse(args[++i]);
    else if (args[i] == "--output" && i + 1 < args.Length)
        outputPath = args[++i];
}

var space = new Grid2D<State>(size, size, new State(1, 0));
var simulation = new Model<Grid2D<State>>(space);

var neighborhood = new Moore(1);
neighborhood.Rows = size;
neighborhood.Columns = size;

// Outer-totalistic Game of Life: B3 / S23
var lifeRule = new TotalisticRule(2, neighborhood, outer: true);

// Dead center: born with 3 live neighbours
lifeRule.Increment(BuildConfig(centerAlive: false, neighborSum: 3), 1);
// Alive center: survives with 2 or 3 live neighbours
lifeRule.Increment(BuildConfig(centerAlive: true, neighborSum: 2), 1);
lifeRule.Increment(BuildConfig(centerAlive: true, neighborSum: 3), 1);

simulation.Rule = lifeRule;
simulation.Randomize();

var stepStates = new List<State[]>();
stepStates.Add(Flatten(simulation.CurrentState, size));
for (int i = 0; i < steps; i++)
{
    Console.WriteLine($"Step {i + 1}/{steps}");
    simulation.Advance();
    stepStates.Add(Flatten(simulation.CurrentState, size));
}

// Save final state as a 2D heatmap
var finalMatrix = simulation.CurrentState
    .ToMatrix()
    .Select(row => row.Select(s => (float)s.Value).ToArray())
    .ToArray();

Console.WriteLine($"Saving {outputPath}.png ...");
ChartHelper.SaveHeatmap(finalMatrix, outputPath);
Console.WriteLine("Done.");

// ---- helpers ----

static State[] Flatten(Grid2D<State> grid, int size)
{
    var rows = new List<State>();
    for (int r = 0; r < size; r++)
        for (int c = 0; c < size; c++)
            rows.Add(grid.Get(r, c));
    return rows.ToArray();
}

static State[] BuildConfig(bool centerAlive, int neighborSum)
{
    // Outer-totalistic: first element = center, rest = uniform representation of sum
    // TotalisticRule with outerTotalistic=true uses (center, sum) as key
    var center = new State(1, centerAlive ? 1 : 0);
    var neighbors = Enumerable.Range(0, neighborSum)
        .Select(_ => new State(1, 1))
        .Concat(Enumerable.Range(0, 8 - neighborSum).Select(_ => new State(1, 0)))
        .ToArray();
    return new[] { center }.Concat(neighbors).ToArray();
}
