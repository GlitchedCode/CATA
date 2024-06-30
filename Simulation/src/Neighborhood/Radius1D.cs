namespace Simulation;

public class Radius1D : Neighborhood
{
    public uint Radius = 1;
    public uint LookBack = 0;

    public Radius1D(uint radius = 1, uint lookBack = 0)
    {
        Radius = radius;
        LookBack = lookBack;
    }

    public override uint Count()
        => (1 + (Radius * 2)) * (LookBack + 1);

    public override State[] Get(Container.Array<State>[] states, int index)
    {
        var segment = new ArraySegment<Container.Array<State>>
            (states, 0, Math.Min((int)LookBack + 1, states.Length));

        var defaultValue = states[0].DefaultValue;
        var ret = Enumerable.Range(1, (int)Count()).Select(_ => (State)defaultValue.Clone()).ToArray();
        var idx = 0;

        foreach (var state in segment)
        {
            ret[idx] = state.Get(index);
            idx++;
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
