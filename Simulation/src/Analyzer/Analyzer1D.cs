namespace Simulation;

public class Analyzer1D
{
    public class Params
    {
        public int StatesCount = 2;
        public uint StartingRadius = 8;
        public uint MaxRadius = 16;
        public double VarianceThreshold = 1;
        public uint LookBackAmount = 1;
    }

    public static SingleRule SingleRule(Container.Array<State>.View[] dynamics, Params paramsObj)
    {
        if (dynamics.Length < paramsObj.LookBackAmount + 4)
            throw new Exception("too few simulation states");

        var cellCount = dynamics[0].CellCount;

        SingleRule ret = null;

        for (uint radius = paramsObj.StartingRadius; radius <= paramsObj.MaxRadius; radius++)
        {
            ret = new(paramsObj.StatesCount);
            var neighborhood = new Radius1D(radius, paramsObj.LookBackAmount);
            ret.Neighborhood = neighborhood;

            for (int i = 0; i < dynamics.Length - paramsObj.LookBackAmount - 1; ++i)
            {
                var segment = new ArraySegment<Container.Array<State>.View>
                    (dynamics, i, (int)paramsObj.LookBackAmount + 1);

                var nextIdx = i + paramsObj.LookBackAmount + 1;

                for (int j = 0; j < cellCount; ++j)
                {
                    var config = ret.Neighborhood.Get(segment.ToArray(), j);
                    var expected = dynamics[nextIdx].Get(j);
                    ret.Increment(config, expected.Value);
                }
            }

            if (ret.AverageVariance() <= paramsObj.VarianceThreshold)
                break;
        }

        return ret;
    }

    public static SingleRule[] TimeSeries(Container.Array<State>.View[] dynamics, Params paramsObj)
    {
        var ret = new SingleRule[dynamics.Length - 1 - paramsObj.LookBackAmount];

        for (int i = 0; i < dynamics.Length - paramsObj.LookBackAmount - 4; ++i)
        {
            var segment = new ArraySegment<Container.Array<State>.View>
                (dynamics, i, (int)paramsObj.LookBackAmount + 4);
            ret[i] = SingleRule(segment.ToArray(), paramsObj);
        }

        return ret;
    }
}
