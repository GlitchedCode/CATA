namespace Test;

/// <summary>
/// End-to-end simulation tests verifying deterministic outcomes against known CA behaviours.
/// </summary>
public class SimulationIntegrationTests
{
    // ── Wolfram rule 30 ───────────────────────────────────────────────────────

    [Fact]
    public void WolframRule30_IsDeterministic()
    {
        // Two simulations with the same seed must produce identical histories.
        State[] Run(int seed)
        {
            var space = new Simulation.Container.Array<State>(20, new State(1, 0));
            var sim = new Model<Simulation.Container.Array<State>>(space);
            sim.Rule = new WolframRule(30);
            sim.Randomize(new Random(seed));
            for (int i = 0; i < 10; i++) sim.Advance();
            return sim.CurrentState.ToArray();
        }

        var a = Run(42);
        var b = Run(42);
        Assert.Equal(a.Select(s => s.Value), b.Select(s => s.Value));
    }

    [Fact]
    public void WolframRule0_AllCellsDie()
    {
        // Rule 0: every configuration maps to state 0.
        var space = new Simulation.Container.Array<State>(10, new State(1, 0));
        var sim = new Model<Simulation.Container.Array<State>>(space);
        sim.Rule = new WolframRule(0);
        sim.Randomize();
        sim.Advance();

        Assert.All(sim.CurrentState.ToArray(), s => Assert.Equal(0, s.Value));
    }

    [Fact]
    public void WolframRule255_AllCellsAlive()
    {
        // Rule 255: every configuration maps to state 1.
        var space = new Simulation.Container.Array<State>(10, new State(1, 0));
        var sim = new Model<Simulation.Container.Array<State>>(space);
        sim.Rule = new WolframRule(255);
        sim.Randomize();
        sim.Advance();

        Assert.All(sim.CurrentState.ToArray(), s => Assert.Equal(1, s.Value));
    }

    // ── Game of Life glider ───────────────────────────────────────────────────

    [Fact]
    public void GameOfLife_GliderSurvivesFourSteps()
    {
        // A standard glider on a 10x10 toroidal grid should still be alive
        // (same number of live cells) after 4 steps.
        int size = 10;
        var space = new Simulation.Container.Grid2D<State>(size, size, new State(1, 0));
        var sim = new Model<Simulation.Container.Grid2D<State>>(space);

        var neighborhood = new Moore(1);
        neighborhood.Rows = size;
        neighborhood.Columns = size;

        var lifeRule = BuildGoLRule(neighborhood);
        sim.Rule = lifeRule;

        // Place a standard glider at top-left
        //  .X.
        //  ..X
        //  XXX
        space.Set(0, 1, new State(1, 1));
        space.Set(1, 2, new State(1, 1));
        space.Set(2, 0, new State(1, 1));
        space.Set(2, 1, new State(1, 1));
        space.Set(2, 2, new State(1, 1));

        int initialLiveCells = CountLiveCells(sim.CurrentState, size);
        Assert.Equal(5, initialLiveCells);

        for (int i = 0; i < 4; i++) sim.Advance();

        // After 4 steps a glider returns to its original shape (shifted by 1,1)
        Assert.Equal(5, CountLiveCells(sim.CurrentState, size));
    }

    [Fact]
    public void GameOfLife_BlockIsStable()
    {
        // A 2x2 block is a still life: it must remain unchanged.
        int size = 8;
        var space = new Simulation.Container.Grid2D<State>(size, size, new State(1, 0));
        var sim = new Model<Simulation.Container.Grid2D<State>>(space);

        var neighborhood = new Moore(1);
        neighborhood.Rows = size;
        neighborhood.Columns = size;
        sim.Rule = BuildGoLRule(neighborhood);

        // Place 2x2 block at (3,3)
        space.Set(3, 3, new State(1, 1));
        space.Set(3, 4, new State(1, 1));
        space.Set(4, 3, new State(1, 1));
        space.Set(4, 4, new State(1, 1));

        for (int i = 0; i < 5; i++) sim.Advance();

        Assert.Equal(1, sim.CurrentState.Get(3, 3).Value);
        Assert.Equal(1, sim.CurrentState.Get(3, 4).Value);
        Assert.Equal(1, sim.CurrentState.Get(4, 3).Value);
        Assert.Equal(1, sim.CurrentState.Get(4, 4).Value);
        Assert.Equal(4, CountLiveCells(sim.CurrentState, size));
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    static TotalisticRule BuildGoLRule(Moore neighborhood)
    {
        var rule = new TotalisticRule(2, neighborhood, outer: true);
        rule.Increment(Config(dead: true, liveNeighbors: 3), 1);   // B3
        rule.Increment(Config(dead: false, liveNeighbors: 2), 1);  // S2
        rule.Increment(Config(dead: false, liveNeighbors: 3), 1);  // S3
        return rule;
    }

    static State[] Config(bool dead, int liveNeighbors)
    {
        var center = new State(1, dead ? 0 : 1);
        return new[] { center }
            .Concat(Enumerable.Range(0, liveNeighbors).Select(_ => new State(1, 1)))
            .Concat(Enumerable.Range(0, 8 - liveNeighbors).Select(_ => new State(1, 0)))
            .ToArray();
    }

    static int CountLiveCells(Simulation.Container.Grid2D<State> grid, int size)
    {
        int count = 0;
        for (int r = 0; r < size; r++)
            for (int c = 0; c < size; c++)
                if (grid.Get(r, c).Value == 1) count++;
        return count;
    }
}
