namespace Simulation;

using System;

public class Model1D
{
    public Rule Rule;
    Container.Array<State> currentState;
    Container.Array<State> previousGridState;

    public State DefaultState = new State(1, 0);

    public Model1D(int cellCount)
    {
        Rule = new(2);
        currentState = new(cellCount, DefaultState);
        previousGridState = new(cellCount, DefaultState);
        ResetState();
    }

    public void Advance()
    {
        previousGridState = currentState;
        currentState = new(previousGridState.CellCount, DefaultState);
        for (int i = 0; i < currentState.CellCount; ++i)
        {
            // von neumann
            var configuration = Rule.Neighborhood.Get1D(previousGridState.GetView(), i);
            var configKey = Neighborhood.Encode(configuration);
            currentState.Set(i, Rule.Get(configKey));
        }
    }

    public void Randomize()
    {
        Random rng = new Random();

        for (int i = 0; i < currentState.CellCount; ++i)
            currentState.Set(i, new State(Rule.BitsCount, rng.Next(Rule.StatesCount)));
    }

    public void Set(int index, State state)
        => this.currentState.Set(index, state);

    public void ResetState(Container.Array<State>.View state = null)
    {
        var nullState = (int i) => new State(Rule.BitsCount, 0);
        var okState = (int i) => state.Get(i);
        var getState = state == null ? nullState : okState;

        for (int i = 0; i < state.CellCount; ++i)
            Set(i, getState(i));
    }

    public void Resize(int cellCount)
    {
        currentState.Resize(cellCount);
        ResetState();
    }

    public Container.Array<State>.View GetCurrentStateView() => currentState.GetView();
    public Container.Array<State>.View GetPreviousStateView() => previousGridState.GetView();
}
