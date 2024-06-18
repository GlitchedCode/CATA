namespace Analysis;
using Simulation;

public class Analyzer2D
{
    public static bool Validate(TableRule rule, Simulation.Container.Grid2D<State>.View[] dynamics)
    {
        return false;
    }

    public static TableRule SingleRule(Simulation.Container.Grid2D<State>.View[] dynamics, int statesCount)
    {
        // TODO fanculo ci si deve rifare ad analyzer1d
        return null;
    }

    public static TableRule[] TimeSeries(Simulation.Container.Grid2D<State>.View[] dynamics, int statesCount)
    {
        // TODO fanculo ci si deve rifare ad analyzer1d
        return null;
    }


}
