
namespace Analysis;
using Simulation;

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

    public static TableRule SingleRule(Simulation.Container.Array<State>[] dynamics, Params paramsObj)
    {
        if (paramsObj.StatesCount < 2)
            throw new ArgumentException("StatesCount must be at least 2", nameof(paramsObj));

        if (dynamics.Length < paramsObj.LookBackAmount + 4)
            throw new Exception("too few simulation states");

        var cellCount = dynamics[0].CellCount;
        if (dynamics.Any(d => d.CellCount != cellCount))
            throw new ArgumentException("all dynamics frames must have the same CellCount", nameof(dynamics));

        TableRule ret = null;

        for (uint radius = paramsObj.StartingRadius; radius <= paramsObj.MaxRadius; radius++)
        {
            ret = new(paramsObj.StatesCount);
            var neighborhood = new Radius1D(radius, paramsObj.LookBackAmount);
            ret.Neighborhood = neighborhood;

            for (int i = 0; i < dynamics.Length - paramsObj.LookBackAmount - 1; ++i)
            {
                var segment = new ArraySegment<Simulation.Container.Array<State>>
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

    public static TableRule[] TimeSeries(Simulation.Container.Array<State>[] dynamics, Params paramsObj)
    {
        int count = Math.Max(0, dynamics.Length - (int)paramsObj.LookBackAmount - 4);
        var ret = new TableRule[count];

        for (int i = 0; i < count; ++i)
        {
            var segment = new ArraySegment<Simulation.Container.Array<State>>
                (dynamics, i, (int)paramsObj.LookBackAmount + 4);
            ret[i] = SingleRule(segment.ToArray(), paramsObj);
        }

        return ret;
    }
}
