// GenerateFigures — produces all thesis figures for the CATA C# experiments.
//
// Outputs (relative to --output dir, default "results"):
//   rule30.png        — Wolfram rule 30 (matches assets/rule30.png)
//   rule110.png       — Wolfram rule 110 (matches assets/rule110.png)
//   meta89.png        — Hierarchical meta-rule with Wolfram 89
//   meta110.png       — Hierarchical meta-rule with Wolfram 110
//   meta30.png        — Hierarchical meta-rule with Wolfram 30 (chaotic)
//   meta73.png        — Hierarchical meta-rule with Wolfram 73 (complex)
//   meta2d_30.png     — 2D hierarchical meta-rule; A=50x50 grid, B driven by Wolfram 30
//   density_adapt.png — 2D density-adaptive neighbourhood meta-rule
//   life.png          — Game of Life after 26 steps (matches thesis caption, assets/life.png)
//
// Usage: GenerateFigures [--output <dir>] [--seed <N>]
//
// To regenerate thesis assets directly:
//   dotnet run --project Programs/GenerateFigures -- --output ../../assets --seed 42

using Simulation;
using Simulation.Container;
using Visualization;

string outputDir = "results";
int seed = 42;
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--output" && i + 1 < args.Length) outputDir = args[++i];
    if (args[i] == "--seed"   && i + 1 < args.Length) seed      = int.Parse(args[++i]);
}

Directory.CreateDirectory(outputDir);

Console.WriteLine("=== GenerateFigures ===");
Console.WriteLine($"Output directory: {outputDir}");
Console.WriteLine($"Random seed:      {seed}");
Console.WriteLine();

// Each figure gets its own seeded RNG derived from the master seed so figures
// are independent but the full set is reproducible from a single --seed value.
WolframFigure(30,  new Random(seed + 0));
WolframFigure(110, new Random(seed + 1));
MetaRuleFigure(89,  new Random(seed + 2));
MetaRuleFigure(110, new Random(seed + 3));
GameOfLifeFigure(   new Random(seed + 4));
MetaRuleFigure(30,  new Random(seed + 5));
MetaRuleFigure(73,  new Random(seed + 6));
MetaRule2DFigure(30, new Random(seed + 7));
DensityAdaptiveFigure(  new Random(seed + 8));

Console.WriteLine();
Console.WriteLine("All figures generated.");

// ── Wolfram 1D elementary CA ──────────────────────────────────────────────────

void WolframFigure(int ruleNumber, Random rng, int cells = 300, int steps = 300)
{
    Console.Write($"Wolfram rule {ruleNumber} ({cells} cells, {steps} steps) ... ");
    var space = new Array<State>(cells, new State(1, 0));
    var simulation = new Model<Array<State>>(space);
    var rule = new WolframRule(ruleNumber);
    simulation.Rule = rule;
    simulation.Randomize(rng);

    var history = CollectSteps(simulation, steps, () => simulation.CurrentState.ToArray());

    string path = Path.Combine(outputDir, $"rule{ruleNumber}");
    ChartHelper.SaveHeatmap(history, rule.StatesCount, path);
    Console.WriteLine($"saved {path}.png");
}

// ── Hierarchical meta-rule simulation (1D outer CA) ───────────────────────────

void MetaRuleFigure(int wolframRule, Random rng, int cells = 100, int steps = 200)
{
    Console.Write($"Meta-rule 1D (Wolfram {wolframRule}) ({cells} cells, {steps} steps) ... ");
    var space = new Array<State>(cells, new State(1, 0));
    var simulation = new Model<Array<State>>(space, 10);

    var higherOrderRule = new WolframRule(wolframRule);
    var rule = new Model1DRule(higherOrderRule, new Radius1D(2));

    // Single live cell at the centre of B gives deterministic seed structure.
    int centre = rule.InnerCellCount / 2;
    rule.SetInnerCell(centre, new State(1, 1));

    simulation.Rule = rule;
    simulation.Randomize(rng);

    var history = CollectSteps(simulation, steps, () => simulation.CurrentState.ToArray());

    string path = Path.Combine(outputDir, $"meta{wolframRule}");
    ChartHelper.SaveHeatmap(history, rule.CurrentRule.StatesCount, path);
    Console.WriteLine($"saved {path}.png");
}

// ── Hierarchical meta-rule simulation (2D outer CA) ───────────────────────────
//
// A is a 2D binary grid (Moore radius-1 neighbourhood, k=9).
// B is a 1D CA with 2^9 = 512 cells — one cell per local configuration of A.
// B is driven by the given Wolfram rule, so A's rule table evolves each step.

void MetaRule2DFigure(int wolframRule, Random rng, int size = 50, int steps = 60)
{
    Console.Write($"Meta-rule 2D (Wolfram {wolframRule}) ({size}x{size}, {steps} steps) ... ");
    var space = new Grid2D<State>(size, size, new State(1, 0));
    var simulation = new Model<Grid2D<State>>(space, 10);

    var moore = new Moore(1, 0, size, size);
    var rule = new Model1DRule(new WolframRule(wolframRule), moore);

    // B initialised with a single live cell at the centre.
    int centre = rule.InnerCellCount / 2;
    rule.SetInnerCell(centre, new State(1, 1));

    simulation.Rule = rule;
    simulation.Randomize(rng);

    for (int i = 0; i < steps; i++)
        simulation.Advance();

    var matrix = simulation.CurrentState
        .ToMatrix()
        .Select(row => row.Select(s => (float)s.Value).ToArray())
        .ToArray();

    string path = Path.Combine(outputDir, $"meta2d_{wolframRule}");
    ChartHelper.SaveHeatmap(matrix, path);
    Console.WriteLine($"saved {path}.png");
}

// ── Density-adaptive neighbourhood meta-rule (2D) ────────────────────────────
//
// Dense zones (local density >= 0.3 in 5×5 window): Moore r=1, GoL B3/S23.
// Sparse zones: VonNeumann r=1, B2/S12.

void DensityAdaptiveFigure(Random rng, int size = 80, int steps = 60)
{
    Console.Write($"Density-adaptive ({size}×{size}, {steps} steps) ... ");

    var mooreN = new Moore(1, 0, size, size);
    var denseRule = new TotalisticRule(2, mooreN, outer: true);
    denseRule.Increment(Config(0, 3, 8), 1);
    denseRule.Increment(Config(1, 2, 8), 1);
    denseRule.Increment(Config(1, 3, 8), 1);

    var vonN = new VonNeumann(1, 0, size, size);
    var sparseRule = new TotalisticRule(2, vonN, outer: true);
    sparseRule.Increment(Config(0, 2, 4), 1);
    sparseRule.Increment(Config(1, 1, 4), 1);
    sparseRule.Increment(Config(1, 2, 4), 1);

    var densityMeta = new DensityNeighborhoodMetaRule(
        denseRule, sparseRule, threshold: 0.3f, rows: size, cols: size);

    var space = new Grid2D<State>(size, size, new State(1, 0));
    var simulation = new Model<Grid2D<State>>(space);
    simulation.Rule = densityMeta;
    simulation.Randomize(rng);

    for (int i = 0; i < steps; i++)
    {
        densityMeta.ObservedState = (Grid2D<State>)simulation.CurrentState;
        simulation.Advance();
    }

    var matrix = simulation.CurrentState
        .ToMatrix()
        .Select(row => row.Select(s => (float)s.Value).ToArray())
        .ToArray();

    string path = Path.Combine(outputDir, "density_adapt");
    ChartHelper.SaveHeatmap(matrix, path);
    Console.WriteLine($"saved {path}.png");
}

// ── Game of Life (2D) — 26 steps matches thesis caption ──────────────────────

void GameOfLifeFigure(Random rng, int size = 100, int steps = 26)
{
    Console.Write($"Game of Life ({size}×{size}, {steps} steps) ... ");
    var space = new Grid2D<State>(size, size, new State(1, 0));
    var simulation = new Model<Grid2D<State>>(space);

    var neighborhood = new Moore(1);
    neighborhood.Rows = size;
    neighborhood.Columns = size;

    var lifeRule = new TotalisticRule(2, neighborhood, outer: true);
    lifeRule.Increment(Config(0, 3, 8), 1);  // B3
    lifeRule.Increment(Config(1, 2, 8), 1);  // S2
    lifeRule.Increment(Config(1, 3, 8), 1);  // S3

    simulation.Rule = lifeRule;
    simulation.Randomize(rng);

    for (int i = 0; i < steps; i++)
        simulation.Advance();

    var matrix = simulation.CurrentState
        .ToMatrix()
        .Select(row => row.Select(s => (float)s.Value).ToArray())
        .ToArray();

    string path = Path.Combine(outputDir, "life");
    ChartHelper.SaveHeatmap(matrix, path);
    Console.WriteLine($"saved {path}.png");
}

// ── Shared helpers ────────────────────────────────────────────────────────────

static List<State[]> CollectSteps<TSpace>(
    Model<TSpace> simulation,
    int steps,
    Func<State[]> snapshot)
    where TSpace : Array<State>
{
    var history = new List<State[]> { snapshot() };
    for (int i = 0; i < steps; i++)
    {
        simulation.Advance();
        history.Add(snapshot());
    }
    return history;
}

// Build an outer-totalistic configuration: center state, liveNeighbors live,
// rest dead; totalNeighbors = total neighbor count (8 for Moore radius 1).
static State[] Config(int centerState, int liveNeighbors, int totalNeighbors) =>
    new[] { new State(1, centerState) }
        .Concat(Enumerable.Repeat(new State(1, 1), liveNeighbors))
        .Concat(Enumerable.Repeat(new State(1, 0), totalNeighbors - liveNeighbors))
        .ToArray();
