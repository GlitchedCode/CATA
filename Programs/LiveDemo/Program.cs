// LiveDemo — terminal live simulation viewer.
//
// Usage:
//   LiveDemo --mode 1d [--rule N] [--meta] [--cells N] [--fps N]
//   LiveDemo --mode 2d [--size N] [--fps N]
//
// Modes:
//   1d   Scrolling view of a 1D Wolfram (or meta-rule) CA, one row per generation.
//   2d   Live in-place view of a 2D Game of Life CA.
//
// Keys (during run): Ctrl+C to quit, R to randomize.

using Simulation;
using Simulation.Container;

// ── Argument parsing ──────────────────────────────────────────────────────────

string mode = "2d";
int rule1d  = 30;
bool meta   = false;
int cells1d = 0;   // 0 = auto (terminal width)
int size2d  = 0;   // 0 = auto
int fps     = 10;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--mode"  when i + 1 < args.Length: mode   = args[++i]; break;
        case "--rule"  when i + 1 < args.Length: rule1d = int.Parse(args[++i]); break;
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
    if (mode == "1d") Run1D(cts.Token);
    else              Run2D(cts.Token);
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

    var space = new Grid2D<State>(rows, cols, new State(1, 0));
    var simulation = new Model<Grid2D<State>>(space);

    var neighborhood = new Moore(1);
    neighborhood.Rows = rows;
    neighborhood.Columns = cols;

    var lifeRule = new TotalisticRule(2, neighborhood, outer: true);
    lifeRule.Increment(Config2D(0, 3, 8), 1);  // B3
    lifeRule.Increment(Config2D(1, 2, 8), 1);  // S2
    lifeRule.Increment(Config2D(1, 3, 8), 1);  // S3

    simulation.Rule = lifeRule;
    simulation.Randomize(new Random());

    Console.Write("\x1b[2J");  // clear screen once

    int step = 0;
    int delayMs = fps > 0 ? 1000 / fps : 0;

    while (!ct.IsCancellationRequested)
    {
        DrawGrid2D((Grid2D<State>)simulation.CurrentState, step, rows, cols);
        simulation.Advance();
        step++;
        if (delayMs > 0)
            Thread.Sleep(delayMs);
    }
}

void DrawGrid2D(Grid2D<State> grid, int step, int rows, int cols)
{
    Console.Write("\x1b[H");  // cursor to top-left

    // Header line
    Console.WriteLine($"\x1b[1mGame of Life — 2D Live Demo\x1b[0m  " +
                      $"\x1b[2mStep: {step,-6}  Grid: {rows}×{cols}  " +
                      $"(Ctrl+C to quit)\x1b[0m");

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
