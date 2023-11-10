using System;

public class Simulation
{
    public Rule Rule;
    Grid2DContainer<bool> state;
    Grid2DContainer<bool> previousState;

    public Simulation(int rows, int cols)
    {
        Rule = new(0);
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
                var neighborhood = VonNeumann(previousState.GetView(), r, c);
                state.Set(r, c, Rule.GetNeighborhoodNext(neighborhood));
            }
    }

    public void Randomize()
    {
        Random rng = new Random();

        for (int r = 0; r < state.Rows; ++r)
            for (int c = 0; c < state.Columns; ++c)
                state.Set(r, c, (rng.Next() % 3) == 0);
    }

    public void Resize(int rows, int cols)
    {
        state.Resize(rows, cols);
        previousState.Resize(rows, cols);
    }

    public Grid2DView<bool> GetCurrentStateView() => state.GetView();
    public Grid2DView<bool> GetPreviousStateView() => previousState.GetView();

    public static bool[] VonNeumann(Grid2DView<bool> state, int r, int c)
    {
        bool[] neighborhood = { false, false, false, false, false };
        neighborhood[0] = state.Get(r, c);
        neighborhood[1] = state.Get(r - 1, c);
        neighborhood[2] = state.Get(r, c + 1);
        neighborhood[3] = state.Get(r, c - 1);
        neighborhood[4] = state.Get(r + 1, c);
        return neighborhood;
    }
}
