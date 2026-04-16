namespace Simulation;

using System;
using System.Collections.Generic;

public class Model<Space> where Space : Container.Array<State>
{
    public MetaRule Rule;

    int maxStateHistory;
    List<Container.Array<State>> stateHistory = new();
    public Space CurrentState {get; protected set;}

    public State DefaultState = new State(1, 0);

    UpdateMask _updateMask = new();
    public UpdateMask UpdateMask {
      get => _updateMask;
      set => _updateMask = value == null ? new() : value;
    }

    public Model(Space space, int maxStateHistory = 1)
    {
        if (maxStateHistory < 1)
            this.maxStateHistory = 1;
        else
            this.maxStateHistory = maxStateHistory;
        
        Rule = new TableRule(2);
        CurrentState = space;
        ResetState(CurrentState);
    }

    public void Advance()
    {
        stateHistory.Insert(0, CurrentState);
        var diff = stateHistory.Count - maxStateHistory;
        if (diff > 0)
            stateHistory.RemoveRange(maxStateHistory, diff);

        CurrentState = CurrentState.MakeNew() as Space;
        for (int i = 0; i < CurrentState.CellCount; ++i)
        {
            if(!UpdateMask.Get(i)) continue;
            var rule = Rule.GetCurrentRule(i);
            var configuration = rule.Neighborhood.Get(stateHistory.ToArray(), i);
            CurrentState.Set(i, rule.Get(configuration));
            rule.Advance();
        }
        
        Rule.Advance();
        UpdateMask.Advance();
    }

    public void Randomize() => Randomize(new Random());

    public void Randomize(Random rng)
    {
        for (int i = 0; i < CurrentState.CellCount; ++i)
            CurrentState.Set(i, new State(Rule.CurrentRule.BitsCount, rng.Next(Rule.CurrentRule.StatesCount)));
    }

    public void Set(int index, State state)
        => this.CurrentState.Set(index, state);

    public void ResetState(Container.Array<State> state = null)
    {
        var nullState = (int i) => new State(Rule.CurrentRule.BitsCount, 0);
        var okState = (int i) => state.Get(i);
        var getState = state == null ? nullState : okState;

        for (int i = 0; i < CurrentState.CellCount; ++i)
            Set(i, getState(i));
    }

    /// <summary>
    /// Resets the simulation to a known state with a given history.
    /// <paramref name="states"/> must be in chronological order (oldest first, newest last).
    /// The last element becomes the current state; all prior elements become the history
    /// (stored internally in newest-first order, matching the convention used by Advance).
    /// </summary>
    public void ResetHistory(IEnumerable<Container.Array<State>> states)
    {
        var list = states.ToList();
        // Store history in newest-first order to match what Advance() builds.
        stateHistory = new(list.Take(list.Count - 1).AsEnumerable().Reverse());
        ResetState(list.Last());
    }

    public void Resize(int cellCount)
    {
        CurrentState.Resize(cellCount);
        ResetState(CurrentState);
    }
}
