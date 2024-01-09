namespace Simulation;

public abstract class StateTable
{
    int _default = 0;
    public int DefaultState
    {
        get => _default;
        set => _default = Math.Max(0, value);
    }

    public StateCounter this[State[] configuration]
    {
        get => Get(configuration);
    }

    public int Count
    {
        get => GetCount();
    }

    public abstract int GetCount();
    public abstract bool Contains(State[] configuration);
    public abstract StateCounter Get(State[] configuration);

    public abstract void Increment(State[] configuration, int stateValue, uint amount = 0);
    public abstract void Set(State[] configuration, int stateValue);
    public abstract void Unset(State[] configuration);

    public abstract void Clear();

    public abstract IEnumerable<State[]> EnumerateConfigurations();
}
