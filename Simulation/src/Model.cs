namespace Simulation;

using System;

public class Model
{
    public Rule Rule;
    Grid2DContainer<State> gridState;
    Grid2DContainer<State> previousGridState;

    public State DefaultState = new State(1, 0);

    public Model(int rows, int cols)
    {
        Rule = new(2);
        gridState = new(rows, cols, DefaultState);
        previousGridState = new(rows, cols, DefaultState);
    }

    public void Advance()
    {
        previousGridState = gridState;
        gridState = new(previousGridState.Rows, previousGridState.Columns, DefaultState);
        for (int r = 0; r < gridState.Rows; ++r)
            for (int c = 0; c < gridState.Columns; ++c)
            {
                // von neumann
                var configuration = Rule.Neighborhood.Get(previousGridState.GetView(), r, c);
                var configKey = Neighborhood.Encode(configuration);
                gridState.Set(r, c, Rule.Get(configKey));
            }
    }

    public void Randomize()
    {
        Random rng = new Random();

        for (int r = 0; r < gridState.Rows; ++r)
            for (int c = 0; c < gridState.Columns; ++c)
                gridState.Set(r, c, new State(Rule.BitsCount, rng.Next(Rule.StatesCount)));
    }

    public void Set(int row, int column, State state)
        => this.gridState.Set(row, column, state);

    public void ResetState(Grid2DView<State> state = null)
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
        gridState.Resize(rows, cols);
        previousGridState.Resize(rows, cols);
    }

    public Grid2DView<State> GetCurrentStateView() => gridState.GetView();
    public Grid2DView<State> GetPreviousStateView() => previousGridState.GetView();
}

