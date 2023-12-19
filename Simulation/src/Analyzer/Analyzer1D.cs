namespace Simulation;

public class Analyzer1D
{
    public struct Params
    {
        int StatesCount;
        int StartingRadius;
        int MaxRadius;
        int VarianceThreshold;
    }

    public static bool Validate(Rule rule, Container.Array<State>.View[] dynamics)
    {
        return false;
    }

    public static Rule SingleRule(Container.Array<State>.View[] dynamics, int statesCount)
    {
        var cellCount = dynamics[0].CellCount;

        Rule ret = new(statesCount);
        var neighborhood = new VonNeumann(1);
        ret.Neighborhood = neighborhood;

        for (int i = 0; i < dynamics.Length - 1; ++i)
        {
            var current = dynamics[i];
            var next = dynamics[i + 1];

            for (int j = 0; j < cellCount; ++j)
            {
                var config = ret.Neighborhood.Get1D(current, j);
                var key = Neighborhood.Encode(config);

                var expected = next.Get(j);
                ret.Increment(key, expected.Value);
            }
        }

        return ret;
    }

    public static Rule[] TimeSeries(Container.Array<State>.View[] dynamics, int statesCount)
    {
        var ret = new Rule[dynamics.Length - 1];

        for (int i = 0; i < dynamics.Length - 1; ++i)
        {
            var series = new Container.Array<State>.View[] { dynamics[i], dynamics[i + 1] };
            ret[i] = SingleRule(series, statesCount);
        }

        return ret;
    }
}
