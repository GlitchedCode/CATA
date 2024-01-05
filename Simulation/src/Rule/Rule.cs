namespace Simulation;

public abstract class Rule
{
    public int DefaultState
    {
        get => GetDefaultState();
        set => SetDefaultState(value);
    }
    public int StatesCount
    {
        get => GetStatesCount();
        set => SetStatesCount(value);
    }
    public int BitsCount
    {
        get => GetBitsCount();
        set => SetBitsCount(value);
    }

    public Neighborhood Neighborhood
    {
        get => GetNeighborhood();
        set => SetNeighborhood(value);
    }

    ////////////////////////////////////////////////////////////
    // public IEnumerable<ConfigurationKey> ConfigurationKeys //
    // {                                                      //
    //     get => GetConfigurationKeys();                     //
    // }                                                      //
    ////////////////////////////////////////////////////////////

    public abstract int GetDefaultState();
    public abstract void SetDefaultState(int v);
    public abstract int GetStatesCount();
    public abstract void SetStatesCount(int v);
    public abstract int GetBitsCount();
    public abstract void SetBitsCount(int v);
    public abstract Neighborhood GetNeighborhood();
    public abstract void SetNeighborhood(Neighborhood v);
    public abstract IEnumerable<State[]> EnumerateConfigurations();


    public abstract State Get(State[] configuration);
    public abstract double[] Distribution(State[] configuration);
    public abstract double AverageDifference(SingleRule other);
    public abstract double AverageVariance();
}
