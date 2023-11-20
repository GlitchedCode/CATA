namespace Simulation;

using System;


public class Analyzer
{
    public static bool Validate(Rule rule, Grid2DView<State>[] dynamics)
    {
        return false;
    }

    public static Rule Analyze(Grid2DView<State>[] dynamics)
    {
        var rows = dynamics[0].Rows;
        var columns = dynamics[0].Columns;

        Rule ret = new(2);
        var neighborhood = new VonNeumann(1);
        neighborhood.Radius = 1;
        ret.Neighborhood = neighborhood;

        for (int i = 0; i < dynamics.Length - 1; ++i)
        {
            var current = dynamics[i];
            var next = dynamics[i + 1];

            for (int r = 0; r < rows; ++r)
                for (int c = 0; c < columns; ++c)
                {
                    var configuration = ret.Neighborhood.Get(current, r, c);
                    var configKey = Neighborhood.Encode(configuration);

                    var expected = next.Get(r, c);
                    ret.Increment(configKey, expected.Value);
                }
        }

        return ret;
    }
}

