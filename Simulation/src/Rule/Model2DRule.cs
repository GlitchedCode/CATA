namespace Simulation;

/// <summary>
/// Adattatore che espone una Model<Grid2D> come Rule per l'automa esterno.
/// Calcola l'indice della configurazione locale → (row, col) in B.
/// </summary>
public class Model2DToRule : Rule
{
    readonly Model<Container.Grid2D<State>> simulation;
    Neighborhood _neighborhood;
    readonly int _cols;

    public Model2DToRule(
        Model<Container.Grid2D<State>> simulation,
        Neighborhood neighborhood,
        int cols)
    {
        this.simulation = simulation;
        this._neighborhood = neighborhood;
        this._cols = cols;
        this.StatesCount = simulation.Rule.CurrentRule.StatesCount;
    }

    int GetIndex(State[] cfg)
    {
        int ret = 0;
        for (int i = 0; i < cfg.Length; i++)
            ret += cfg[i].Value * Enumerable
                .Repeat(StatesCount, i)
                .Aggregate(1, (a, b) => a * b);
        return ret;
    }

    public override State Get(State[] configuration)
    {
        int idx = GetIndex(configuration);
        int row = idx / _cols;
        int col = idx % _cols;
        return simulation.CurrentState.Get(row, col);
    }

    public override void SetNeighborhood(Neighborhood v) => _neighborhood = v;
    public override Neighborhood GetNeighborhood()       => _neighborhood;
    public override int GetStatesCount()  => simulation.Rule.CurrentRule.StatesCount;
    public override int GetBitsCount()    => simulation.Rule.CurrentRule.BitsCount;
    public override int GetDefaultState() => simulation.Rule.CurrentRule.DefaultState;

    public override double[] Distribution(State[] configuration)
        => throw new NotImplementedException();
    public override IEnumerable<State[]> EnumerateConfigurations()
        => throw new NotImplementedException();
    public override void SetBitsCount(int v)    { }
    public override void SetDefaultState(int v) { }
    public override void SetStatesCount(int v)  { }
}

/// <summary>
/// Meta-regola gerarchica in cui l'automa interno B è una griglia 2D.
/// Le configurazioni locali di A mappano su celle di B tramite indice base-stati.
/// B può essere governato da qualsiasi regola 2D (GoL, altro totalistico, …).
/// </summary>
public class Model2DRule : MetaRule
{
    readonly Model<Container.Grid2D<State>> simulation;
    readonly Model2DToRule simRule;
    public int InnerRows { get; }
    public int InnerCols { get; }

    /// <param name="innerRule">Regola che governa B (es. GoL).</param>
    /// <param name="outerNeighborhood">Adiacenza di A; determina il numero di celle di B.</param>
    public Model2DRule(Rule innerRule, Neighborhood outerNeighborhood)
    {
        int cellCount = Enumerable
            .Repeat(innerRule.StatesCount, (int)outerNeighborhood.Count())
            .Aggregate(1, (a, b) => a * b);

        (InnerRows, InnerCols) = ComputeGridSize(cellCount);

        var space = new Container.Grid2D<State>(
            InnerRows, InnerCols, new State(1, 0), BoundaryCondition.Periodic);
        simulation = new Model<Container.Grid2D<State>>(space, 10);
        simulation.Rule = innerRule;

        simRule = new Model2DToRule(simulation, outerNeighborhood, InnerCols);
    }

    public override Rule GetCurrentRule(int position) => simRule;
    public override void Advance() => simulation.Advance();

    public void Randomize()               => simulation.Randomize();
    public void Randomize(Random rng)     => simulation.Randomize(rng);
    public void SetInnerCell(int row, int col, State state)
        => simulation.Set(row * InnerCols + col, state);

    /// <summary>
    /// Espone la griglia interna per visualizzazione o ispezione.
    /// </summary>
    public Container.Grid2D<State> InnerState
        => (Container.Grid2D<State>)simulation.CurrentState;

    // Calcola dimensioni (rows, cols) con rows*cols == cellCount e rows ≈ √cellCount.
    static (int rows, int cols) ComputeGridSize(int cellCount)
    {
        int rows = (int)Math.Floor(Math.Sqrt(cellCount));
        while (rows > 1 && cellCount % rows != 0)
            rows--;
        int cols = cellCount / rows;
        return (rows, cols);
    }
}
