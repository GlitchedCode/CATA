namespace Simulation;

public class Radius1D : PastStateNeighborhood
{
    public uint Radius = 1;
    public uint LookBack = 0;

    public Radius1D(uint radius = 1, uint lookBack = 0)
    {
        Radius = radius;
        LookBack = lookBack;
    }

    public override uint Count1D()
        => (1 + (Radius * 2)) * (LookBack + 1);

    public override State[] Get1D(Container.Array<State>.View[] states, int index)
    {
        var defaultValue = states[0].DefaultValue;
        var ret = Enumerable.Range(1, (int)Count1D()).Select(_ => (State)defaultValue.Clone()).ToArray();
        var idx = 0;

        foreach (var state in states)
        {
            ret[idx] = state.Get(index);
            for (int i = 1; i <= Radius; i++)
            {
                ret[idx] = state.Get(index - i);
                idx++;
                ret[idx] = state.Get(index + i);
                idx++;
            }
        }

        return ret;
    }
}
