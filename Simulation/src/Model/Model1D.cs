namespace Simulation;

using System;
using System.Collections.Generic;

public class Model1D
{
    public Rule Rule;

    int maxStateHistory;
    List<Container.Array<State>.View> stateHistory = new();
    Container.Array<State> currentState;

    public State DefaultState = new State(1, 0);

    UpdateMask _updateMask = new();
    public UpdateMask UpdateMask {
      get => _updateMask;
      set => _updateMask = value == null ? new() : value;
    }

    public Model1D(int cellCount, int maxStateHistory = 1)
    {
        if (maxStateHistory < 1)
            this.maxStateHistory = 1;
        else
            this.maxStateHistory = maxStateHistory;

        Rule = new SingleRule(2);
        currentState = new(cellCount, DefaultState);
        ResetState(currentState.GetView());
    }

    public void Advance()
    {
        stateHistory.Insert(0, currentState.GetView());
        var diff = stateHistory.Count - maxStateHistory;
        if (diff > 0)
            stateHistory.RemoveRange(maxStateHistory, diff);

        currentState = new(stateHistory[0].CellCount, DefaultState);
        for (int i = 0; i < currentState.CellCount; ++i)
        {
            if(!UpdateMask.Get(i)) continue;
            var configuration = Rule.Neighborhood.Get(stateHistory.ToArray(), i);
            currentState.Set(i, Rule.Get(configuration));
        }

        UpdateMask.Advance();
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

        for (int i = 0; i < currentState.CellCount; ++i)
            Set(i, getState(i));
    }

    public void ResetHistory(IEnumerable<Container.Array<State>.View> states)
    {
        stateHistory = new(states.Take(states.Count() - 1));
        ResetState(states.Last());
    }

    public void Resize(int cellCount)
    {
        currentState.Resize(cellCount);
        ResetState(currentState.GetView());
    }

    public Container.Array<State>.View GetCurrentStateView() => currentState.GetView();
}
