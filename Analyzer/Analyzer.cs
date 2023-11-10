using System;

public class Analyzer
{
    public static Rule Analyze(Grid2DView<bool>[] dynamics)
    {
        Rule ret = new();
        var rows = dynamics[0].Rows;
        var columns = dynamics[0].Columns;

        for (int i = 0; i < dynamics.Length - 1; ++i)
        {
            var current = dynamics[i];
            var next = dynamics[i+1];

            for (int r = 0; r < rows; ++r)
                for (int c = 0; c < columns; ++c)
                {
                    var neighborhood = Simulation.VonNeumann(current, r, c);
                    var bit = ret.GetNeighborhoodBit(neighborhood);

                    var expected = next.Get(r, c);

                    if (ret.IsBitKnown(bit))
                    {
                        if (ret.Get(bit) != expected)
                            throw new Exception("Supplied dynamics are inconsistent.");
                    }
                    else
                    {
                        ret.Set(bit, expected);
                    }

                    if (ret.IsKnown()) return ret;
                }
        }

        return ret;
    }
}
