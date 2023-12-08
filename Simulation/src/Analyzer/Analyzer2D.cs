namespace Simulation;

using System;


public class Analyzer2D
{
    public static bool Validate(Rule rule, Container.Grid2D<State>.View[] dynamics)
    {
        return false;
    }

    public static Rule SingleRule(Container.Grid2D<State>.View[] dynamics, int statesCount)
    {
        var rows = dynamics[0].Rows;
        var columns = dynamics[0].Columns;

        Rule ret = new(statesCount);
        var neighborhood = new VonNeumann(1);
        ret.Neighborhood = neighborhood;

        for (int i = 0; i < dynamics.Length - 1; ++i)
        {
            var current = dynamics[i];
            var next = dynamics[i + 1];

            for (int r = 0; r < rows; ++r)
                for (int c = 0; c < columns; ++c)
                {
                    var configuration = ret.Neighborhood.Get2D(current, r, c);
                    var configKey = Neighborhood.Encode(configuration);

                    var expected = next.Get(r, c);
                    ret.Increment(configKey, expected.Value);
                }
        }

        return ret;
    }

    public static Rule[] TimeSeries(Container.Grid2D<State>.View[] dynamics, int statesCount)
    {
        var rows = dynamics[0].Rows;
        var columns = dynamics[0].Columns;

        var ret = new Rule[dynamics.Length - 1];

        for (int i = 0; i < dynamics.Length - 1; ++i)
        {
            Rule rule = new(statesCount);
            var neighborhood = new VonNeumann(1);
            rule.Neighborhood = neighborhood;

            var current = dynamics[i];
            var next = dynamics[i + 1];

            for (int r = 0; r < rows; ++r)
                for (int c = 0; c < columns; ++c)
                {
                    var configuration = rule.Neighborhood.Get2D(current, r, c);
                    var configKey = Neighborhood.Encode(configuration);

                    var expected = next.Get(r, c);
                    rule.Increment(configKey, expected.Value);
                }

            ret[i] = rule;
        }

        return ret;
    }


}
