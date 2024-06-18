namespace Simulation;

using System;

public class Model2D
{
    public int Rows { get; protected set; }
    public int Columns { get; protected set; }

    MetaRule _rule = null;
    public MetaRule Rule
    {
        get => _rule;
        set
        {
            _rule = value;
        }
    }

    int maxStateHistory;
    List<Container.Grid2D<State>> stateHistory = new();
    Container.Grid2D<State> currentState;

    public State DefaultState = new State(1, 0);

    public Model2D(int rows, int cols, int maxStateHistory = 1)
    {
        if (maxStateHistory < 1)
            this.maxStateHistory = 1;
        else
            this.maxStateHistory = maxStateHistory;

        Rule = new TableRule(2);
        currentState = new(rows, cols, DefaultState);
        Resize(rows, cols);
    }

    public void Advance()
    {
        stateHistory.Add(currentState);
        var diff = stateHistory.Count - maxStateHistory;
        if (diff > 0)
            stateHistory.RemoveRange(0, diff);

        currentState = new(stateHistory[0].Rows, stateHistory[0].Columns, DefaultState);

        for (int r = 0; r < currentState.Rows; ++r)
            for (int c = 0; c < currentState.Columns; ++c)
            {
                // von neumann
                State[] configuration = Neighborhood.Get(stateHistory.ToArray(), r, c);
                currentState.Set(r, c, Rule.Get(configuration));
            }
    }

    public void Randomize()
    {
        Random rng = new Random();

        for (int r = 0; r < currentState.Rows; ++r)
            for (int c = 0; c < currentState.Columns; ++c)
                currentState.Set(r, c, new State(Rule.BitsCount, rng.Next(Rule.StatesCount)));
    }

    public void Set(int row, int column, State state)
        => this.currentState.Set(row, column, state);

    public void ResetState(Container.Grid2D<State>.View state = null)
    {
        var nullState = (int r, int c) => new State(Rule.BitsCount, 0);
        var okState = (int r, int c) => state.Get(r, c);
        var getState = state == null ? nullState : okState;

        for (int r = 0; r < state.Rows; ++r)
            for (int c = 0; c < state.Columns; ++c)
                Set(r, c, getState(r, c));
    }

    public void Resize(int rows, int cols)
    {
        currentState.Resize(rows, cols);
        ResetState(currentState.GetView());
        Rows = rows;
        Columns = cols;
    }

    public Container.Grid2D<State>.View GetCurrentStateView() => currentState.GetView();
}

