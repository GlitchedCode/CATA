namespace Simulation;

using System;

public class Model
{
    public Rule Rule;
    Grid2DContainer<bool> state;
    Grid2DContainer<bool> previousState;

    public Model(int rows, int cols)
    {
        Rule = new();
        state = new(rows, cols, false);
        previousState = new(rows, cols, false);
    }

    public void Advance()
    {
        previousState = state;
        state = new(previousState.Rows, previousState.Columns, false);
        for (int r = 0; r < state.Rows; ++r)
            for (int c = 0; c < state.Columns; ++c)
            {
                // von neumann
                var configuration = Rule.Neighborhood.Get(previousState.GetView(), r, c);
                var configKey = Neighborhood.Encode(configuration);
                state.Set(r, c, Rule.Get(configKey));
            }
    }

    public void Randomize()
    {
        Random rng = new Random();

        for (int r = 0; r < state.Rows; ++r)
            for (int c = 0; c < state.Columns; ++c)
                state.Set(r, c, (rng.Next() % 3) == 0);
    }

    public void Set(int row, int column, bool state)
        => this.state.Set(row, column, state);

    public void ResetState(Grid2DView<bool> state = null)
    {
        var nullState = (int r, int c) => false;
        var okState = (int r, int c) => state.Get(r, c);
        var getState = state == null ? nullState : okState;

        for (int r = 0; r < state.Rows; ++r)
            for (int c = 0; c < state.Columns; ++c)
                Set(r, c, getState(r, c));
    }

    public void Resize(int rows, int cols)
    {
        state.Resize(rows, cols);
        previousState.Resize(rows, cols);
    }

    public Grid2DView<bool> GetCurrentStateView() => state.GetView();
    public Grid2DView<bool> GetPreviousStateView() => previousState.GetView();
}

