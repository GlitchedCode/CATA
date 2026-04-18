// LiveDemo — terminal live simulation viewer.
//
// Usage:
//   LiveDemo --mode 1d      [--rule N] [--meta] [--cells N] [--fps N]
//   LiveDemo --mode 2d      [--rule N] [--meta] [--size N]  [--fps N]
//   LiveDemo --mode density [--size N] [--fps N]
//   LiveDemo --mode meta2d  [--size N] [--fps N]
//
// Modes:
//   1d       Scrolling view of a 1D Wolfram (or meta-rule) CA, one row per generation.
//   2d       Live in-place view of a 2D CA (GoL default, or hierarchical --meta --rule N).
//   density  2D CA with density-adaptive neighbourhood (Moore vs VonNeumann by density).
//   meta2d   2D hierarchical CA where B is itself a 2D GoL grid. Shows A and B side by side.
//
// Keys (during run): Ctrl+C to quit, R to randomize, +/→ next rule, -/← prev rule (meta mode).

using Simulation;
using Simulation.Container;

// ── Argument parsing ──────────────────────────────────────────────────────────

string mode = "2d";
int rule1d  = 30;
bool meta   = false;
int cells1d = 0;   // 0 = auto (terminal width)
int size2d  = 0;   // 0 = auto
int fps     = 10;
int innerRuleIdx = 0;
LifeRule[] innerRuleDefs =
[
    new("GoL B3/S23",             new[]{3},       new[]{2,3}),
    new("HighLife B36/S23",       new[]{3,6},     new[]{2,3}),
    new("Day&Night B3678/S34678", new[]{3,6,7,8}, new[]{3,4,6,7,8}),
    new("Move B368/S245",         new[]{3,6,8},   new[]{2,4,5}),
    new("2x2 B36/S125",           new[]{3,6},     new[]{1,2,5}),
    new("Stains B3678/S235678",   new[]{3,6,7,8}, new[]{2,3,5,6,7,8}),
    new("LongLife B345/S5",       new[]{3,4,5},   new[]{5}),
];

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--mode"  when i + 1 < args.Length: mode    = args[++i]; break;
        case "--rule"  when i + 1 < args.Length: rule1d  = int.Parse(args[++i]); break;
        case "--cells" when i + 1 < args.Length: cells1d = int.Parse(args[++i]); break;
        case "--size"  when i + 1 < args.Length: size2d  = int.Parse(args[++i]); break;
        case "--fps"   when i + 1 < args.Length: fps     = int.Parse(args[++i]); break;
        case "--meta":                            meta    = true; break;
    }
}

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.CursorVisible = false;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

try
{
    if      (mode == "1d")      Run1D(cts.Token);
    else if (mode == "density") RunDensity(cts.Token);
    else if (mode == "meta2d")  RunMeta2D(cts.Token);
    else                        Run2D(cts.Token);
}
finally
{
    Console.CursorVisible = true;
    Console.ResetColor();
    Console.WriteLine();
}

// ── 1D scrolling view ─────────────────────────────────────────────────────────

void Run1D(CancellationToken ct)
{
    int width = cells1d > 0 ? cells1d : Math.Max(20, Console.WindowWidth - 8);

    var space = new Array<State>(width, new State(1, 0));
    var simulation = new Model<Array<State>>(space, meta ? 10 : 1);

    MetaRule activeRule;
    if (meta)
    {
        var higherOrder = new WolframRule(rule1d);
        var mr = new Model1DRule(higherOrder, new Radius1D(2));
        mr.Randomize();
        activeRule = mr;
    }
    else
    {
        activeRule = new WolframRule(rule1d);
    }

    simulation.Rule = activeRule;
    simulation.Randomize(new Random());

    Console.Clear();
    string label = meta ? $"Meta-rule (Wolfram {rule1d})" : $"Wolfram rule {rule1d}";
    Console.WriteLine($"\x1b[1m{label} — 1D Live Demo\x1b[0m  \x1b[2m(Ctrl+C to quit)\x1b[0m");

    int step = 0;
    int delayMs = fps > 0 ? 1000 / fps : 0;

    while (!ct.IsCancellationRequested)
    {
        PrintRow1D(simulation.CurrentState, step);
        simulation.Advance();
        step++;
        if (delayMs > 0)
            Thread.Sleep(delayMs);
    }
}

void PrintRow1D(Array<State> state, int step)
{
    var sb = new System.Text.StringBuilder();
    sb.Append($"\x1b[2m{step,5}\x1b[0m │");
    for (int i = 0; i < state.CellCount; i++)
        sb.Append(state.Get(i).Value == 1 ? "\x1b[97m█\x1b[0m" : "\x1b[90m·\x1b[0m");
    sb.Append("│");
    Console.WriteLine(sb.ToString());
}

// ── 2D live view ──────────────────────────────────────────────────────────────

void Run2D(CancellationToken ct)
{
    // Each cell is rendered as 2 chars wide ("██" or "  ") so rows fit the terminal.
    int cols = size2d > 0 ? size2d : (Console.WindowWidth  / 2) - 1;
    int rows = size2d > 0 ? size2d : Math.Max(10, Console.WindowHeight - 4);
    cols = Math.Max(10, cols);
    rows = Math.Max(10, rows);

    var rng = new Random();

    Model<Grid2D<State>> BuildSim()
    {
        var space = new Grid2D<State>(rows, cols, new State(1, 0));
        var sim = new Model<Grid2D<State>>(space, meta ? 10 : 1);

        if (meta)
        {
            var moore = new Moore(1, 0, rows, cols);
            var mr = new Model1DRule(new WolframRule(rule1d), moore);
            mr.SetInnerCell(mr.InnerCellCount / 2, new State(1, 1));
            sim.Rule = mr;
        }
        else
        {
            var neighborhood = new Moore(1);
            neighborhood.Rows = rows;
            neighborhood.Columns = cols;
            var lifeRule = new TotalisticRule(2, neighborhood, outer: true);
            lifeRule.Increment(Config2D(0, 3, 8), 1);  // B3
            lifeRule.Increment(Config2D(1, 2, 8), 1);  // S2
            lifeRule.Increment(Config2D(1, 3, 8), 1);  // S3
            sim.Rule = lifeRule;
        }

        sim.Randomize(rng);
        return sim;
    }

    var simulation = BuildSim();

    Console.Write("\x1b[2J");  // clear screen once

    int step = 0;
    int delayMs = fps > 0 ? 1000 / fps : 0;

    string label = meta
        ? $"Meta-rule 2D (Wolfram {rule1d}) — {rows}×{cols}"
        : $"Game of Life — {rows}×{cols}";

    while (!ct.IsCancellationRequested)
    {
        // Non-blocking key check
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(intercept: true);
            bool rebuild = false;
            if (key.Key == ConsoleKey.R)
            {
                rebuild = true;
            }
            else if (meta && (key.Key == ConsoleKey.RightArrow || key.KeyChar == '+'))
            {
                rule1d = (rule1d + 1) & 0xFF;
                rebuild = true;
            }
            else if (meta && (key.Key == ConsoleKey.LeftArrow || key.KeyChar == '-'))
            {
                rule1d = (rule1d + 255) & 0xFF;
                rebuild = true;
            }
            if (rebuild)
            {
                label = meta
                    ? $"Meta-rule 2D (Wolfram {rule1d}) — {rows}×{cols}"
                    : $"Game of Life — {rows}×{cols}";
                simulation = BuildSim();
                step = 0;
            }
        }

        DrawGrid2D((Grid2D<State>)simulation.CurrentState, step, rows, cols, label);
        simulation.Advance();
        step++;
        if (delayMs > 0)
            Thread.Sleep(delayMs);
    }
}

void DrawGrid2D(Grid2D<State> grid, int step, int rows, int cols, string label)
{
    Console.Write("\x1b[H");  // cursor to top-left

    // Header line
    string keys = meta
        ? "R=randomize  ←/- prev rule  →/+ next rule  Ctrl+C=quit"
        : "R=randomize  Ctrl+C=quit";
    Console.WriteLine($"\x1b[1m{label}\x1b[0m  \x1b[2mStep: {step,-6}  {keys}\x1b[0m");

    // Top border
    Console.Write("┌");
    Console.Write(new string('─', cols * 2));
    Console.WriteLine("┐");

    // Grid rows
    var sb = new System.Text.StringBuilder(cols * 10 + 4);
    for (int r = 0; r < rows; r++)
    {
        sb.Clear();
        sb.Append("│");
        for (int c = 0; c < cols; c++)
            sb.Append(grid.Get(r, c).Value == 1 ? "\x1b[92m██\x1b[0m" : "  ");
        sb.Append("│");
        Console.WriteLine(sb.ToString());
    }

    // Bottom border
    Console.Write("└");
    Console.Write(new string('─', cols * 2));
    Console.WriteLine("┘");
}

// ── 2D hierarchical meta-rule with 2D inner CA (B = life-like grid) ──────────
//
// A = outer 2D grid; B = 2D life-like CA of size InnerRows×InnerCols.
// Each local configuration of A maps to a cell in B via base-2 index.
// Both grids are shown side by side: A (left) and B (right).
// +/→ and -/← cycle B's rule through a curated list of long-lived rules.

void RunMeta2D(CancellationToken ct)
{
    int cols = size2d > 0 ? size2d : (Console.WindowWidth / 4) - 2;
    int rows = size2d > 0 ? size2d : Math.Max(10, Console.WindowHeight - 5);
    cols = Math.Max(10, cols);
    rows = Math.Max(10, rows);

    var rng = new Random();

    Model<Grid2D<State>>? sim = null;
    Model2DRule? metaRule = null;

    // Compute B dims once (depends only on A's neighbourhood, not inner rule)
    var mooreProbe = new Moore(1, 0, rows, cols);
    var probe = new Model2DRule(new WolframRule(0), mooreProbe);
    int bRows = probe.InnerRows;
    int bCols = probe.InnerCols;

    TotalisticRule BuildInnerRule()
    {
        var def = innerRuleDefs[innerRuleIdx];
        var bMoore = new Moore(1, 0, bRows, bCols);
        var r = new TotalisticRule(2, bMoore, outer: true);
        foreach (var b in def.Birth)   r.Increment(Config2D(0, b, 8), 1);
        foreach (var s in def.Survive) r.Increment(Config2D(1, s, 8), 1);
        return r;
    }

    void Build()
    {
        var moore = new Moore(1, 0, rows, cols);
        metaRule = new Model2DRule(BuildInnerRule(), moore);
        metaRule.Randomize(rng);

        var space = new Grid2D<State>(rows, cols, new State(1, 0));
        sim = new Model<Grid2D<State>>(space, 10);
        sim.Rule = metaRule;
        sim.Randomize(rng);
    }

    Build();
    Console.Write("\x1b[2J");
    int step = 0;
    int delayMs = fps > 0 ? 1000 / fps : 0;

    while (!ct.IsCancellationRequested)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.R)
            {
                Build(); step = 0;
            }
            else if (key.Key == ConsoleKey.RightArrow || key.KeyChar == '+')
            {
                innerRuleIdx = (innerRuleIdx + 1) % innerRuleDefs.Length;
                Build(); step = 0;
            }
            else if (key.Key == ConsoleKey.LeftArrow || key.KeyChar == '-')
            {
                innerRuleIdx = (innerRuleIdx + innerRuleDefs.Length - 1) % innerRuleDefs.Length;
                Build(); step = 0;
            }
        }

        DrawMeta2D((Grid2D<State>)sim!.CurrentState, metaRule!.InnerState,
                   step, rows, cols, bRows, bCols, innerRuleDefs[innerRuleIdx].Name);
        sim.Advance();
        step++;
        if (delayMs > 0)
            Thread.Sleep(delayMs);
    }
}

void DrawMeta2D(Grid2D<State> gridA, Grid2D<State> gridB,
                int step, int aRows, int aCols, int bRows, int bCols, string ruleName)
{
    Console.Write("\x1b[H");
    Console.WriteLine(
        $"\x1b[1mA ({aRows}×{aCols})  │  B: {ruleName} ({bRows}×{bCols})\x1b[0m  " +
        $"\x1b[2mStep:{step,-5} R=rand  ←/→=rule  Ctrl+C=quit\x1b[0m");

    int displayRows = Math.Max(aRows, bRows);

    // Top borders
    Console.Write("┌" + new string('─', aCols * 2) + "┐  ┌" + new string('─', bCols * 2) + "┐");
    Console.WriteLine();

    var sb = new System.Text.StringBuilder();
    for (int r = 0; r < displayRows; r++)
    {
        sb.Clear();
        // A column
        sb.Append("│");
        if (r < aRows)
            for (int c = 0; c < aCols; c++)
                sb.Append(gridA.Get(r, c).Value == 1 ? "\x1b[92m██\x1b[0m" : "  ");
        else
            sb.Append(new string(' ', aCols * 2));
        sb.Append("│  │");
        // B column
        if (r < bRows)
            for (int c = 0; c < bCols; c++)
                sb.Append(gridB.Get(r, c).Value == 1 ? "\x1b[93m██\x1b[0m" : "  ");
        else
            sb.Append(new string(' ', bCols * 2));
        sb.Append("│");
        Console.WriteLine(sb.ToString());
    }

    Console.Write("└" + new string('─', aCols * 2) + "┘  └" + new string('─', bCols * 2) + "┘");
    Console.WriteLine();
}

// ── Density-adaptive neighbourhood meta-rule (2D) ────────────────────────────

void RunDensity(CancellationToken ct)
{
    int cols = size2d > 0 ? size2d : (Console.WindowWidth  / 2) - 1;
    int rows = size2d > 0 ? size2d : Math.Max(10, Console.WindowHeight - 4);
    cols = Math.Max(10, cols);
    rows = Math.Max(10, rows);

    var rng = new Random();

    // Dense rule: GoL (Moore r=1, outer-totalistic B3/S23, 8 neighbours)
    var mooreN = new Moore(1, 0, rows, cols);
    var denseRule = new TotalisticRule(2, mooreN, outer: true);
    denseRule.Increment(Config2D(0, 3, 8), 1);  // B3
    denseRule.Increment(Config2D(1, 2, 8), 1);  // S2
    denseRule.Increment(Config2D(1, 3, 8), 1);  // S3

    // Sparse rule: VonNeumann r=1, outer-totalistic B2/S12 (4 neighbours)
    // Creates spreading/branching patterns in low-density zones.
    var vonN = new VonNeumann(1, 0, rows, cols);
    var sparseRule = new TotalisticRule(2, vonN, outer: true);
    sparseRule.Increment(Config2D(0, 2, 4), 1);  // B2
    sparseRule.Increment(Config2D(1, 1, 4), 1);  // S1
    sparseRule.Increment(Config2D(1, 2, 4), 1);  // S2

    var densityMeta = new DensityNeighborhoodMetaRule(
        denseRule, sparseRule, threshold: 0.3f, rows: rows, cols: cols);

    var space = new Grid2D<State>(rows, cols, new State(1, 0));
    var simulation = new Model<Grid2D<State>>(space);
    simulation.Rule = densityMeta;
    simulation.Randomize(rng);

    Console.Write("\x1b[2J");
    int step = 0;
    int delayMs = fps > 0 ? 1000 / fps : 0;
    string label = $"Density-adaptive neighbourhood — {rows}×{cols}";

    while (!ct.IsCancellationRequested)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.R)
            {
                simulation.Randomize(rng);
                step = 0;
            }
        }

        // Snapshot current state before Advance() moves it to history
        densityMeta.ObservedState = (Grid2D<State>)simulation.CurrentState;

        DrawGrid2D((Grid2D<State>)simulation.CurrentState, step, rows, cols, label);
        simulation.Advance();
        step++;
        if (delayMs > 0)
            Thread.Sleep(delayMs);
    }
}

// ── Helpers ───────────────────────────────────────────────────────────────────

// Build an outer-totalistic configuration: center state, liveNeighbors live,
// rest dead, totalNeighbors = total neighbor count (8 for Moore radius 1).
static State[] Config2D(int centerState, int liveNeighbors, int totalNeighbors)
{
    return new[] { new State(1, centerState) }
        .Concat(Enumerable.Repeat(new State(1, 1), liveNeighbors))
        .Concat(Enumerable.Repeat(new State(1, 0), totalNeighbors - liveNeighbors))
        .ToArray();
}

record LifeRule(string Name, int[] Birth, int[] Survive);
