using Godot;
using System.Text.Json;

public partial class Demo1D : Control
{
    Simulation1DView originalView, recreatedView;

    Model1D originalSimulation, recreatedSimulation;
    List<Simulation.Container.Array<State>.View> states;

    Label diffLabel;

    [Export] SpriteFrames targetFrames;

    public override void _Ready()
    {
        GetNode<Godot.Timer>("TurnTimer").Timeout += onTurnTimeout;
        originalView = GetNode<Simulation1DView>("Original");
        recreatedView = GetNode<Simulation1DView>("Recreated");

        diffLabel = GetNode<Label>("labels/rules/difference");

        originalSimulation = new(100);
        originalSimulation.Randomize();

        recreatedSimulation = new(100);


        if (targetFrames != null)
        {
            originalSimulation.Rule = AnalyzeVideo();
        }
        else
        {
            states = new();
            states.Add(originalSimulation.GetCurrentStateView());
            originalSimulation.Rule = RandomRule.Make1D(new VonNeumann(1), 2);
        }

        Advance();
    }

    Rule AnalyzeVideo()
    {
        /*
        var stateCount = 2;
        var bitCount = (int)Math.Ceiling(Math.Log2(stateCount));
        var stateWidth = 1d / (double)stateCount;

        var frames = targetFrames.GetFrameCount("default");
        var size = targetFrames.GetFrameTexture("default", 0).GetImage().GetSize();

        List<Simulation.Container.Grid2D<State>.View> timeSeries = new();

        for (int frame = 0; frame < frames; frame++)
        {
            var img = targetFrames.GetFrameTexture("default", frame).GetImage();
            var grid = new Simulation.Container.Grid2D<State>(size.Y, size.X, new State(1, 0));

            for (int x = 0; x < size.X; x++)
                for (int y = 0; y < size.Y; y++)
                {
                    var value = img.GetPixel(x, y).R;
                    var state = Math.Clamp((int)Math.Floor(value / stateWidth), 0, stateCount - 1);
                    grid.Set(y, x, new State(bitCount, state));
                }

            timeSeries.Add(grid.GetView());
        }

        var ruleTimeSeries = Analyzer2D.TimeSeries(timeSeries.ToArray(), stateCount);

        string fileName = "ruleData.json";
        string jsonString = JsonSerializer.Serialize(ruleTimeSeries);
        File.WriteAllText(fileName, jsonString);

        // plot della probabilità che lo stato successivo sia 1
        // per ciascuna configurazione in relazione al tempo
        var plt = new ScottPlot.Plot();

        var hood = ruleTimeSeries[0].Neighborhood;
        var xData = Enumerable.Range(0, ruleTimeSeries.Count())
            .Select(x => (double)x).ToArray();

        foreach (var k in hood.Enumerate2DConfigurations(stateCount))
        {
            var list = new List<double>();

            for(int i = 0; i < ruleTimeSeries.Count(); i++)
                list.Add(ruleTimeSeries[i].Distribution(k)[stateCount - 1]);

            plt.AddScatterLines(xData, list.ToArray());
        }

        plt.SaveFig("activation_probability_by_turn.png");

        var ret = ruleTimeSeries[0];
        return ruleTimeSeries.Skip(1).Aggregate(ret,
                                                (x, y) => x + y,
                                                r => r);
        */
        return new Rule(2, 0);
    }

    void onTurnTimeout()
    {
        Advance();
        /*
        if (states != null && states.Count == 5)
        {
            var originalRule = originalSimulation.Rule;
            Console.WriteLine($"diff before analyze: {originalRule.AverageDifference(recreatedSimulation.Rule)}");
            var predicted = Analyzer2D.SingleRule(states.ToArray(), 2);
            recreatedSimulation.Rule = predicted;
            Console.WriteLine($"diff after analyze: {recreatedSimulation.Rule.AverageDifference(originalRule)}");
            originalSimulation.Randomize();
            recreatedSimulation.ResetState(originalSimulation.GetCurrentStateView());
            diffLabel.Text = $"difference: {recreatedSimulation.Rule.AverageDifference(originalRule)}";
            states = null;


            void print(Rule r)
            {
                foreach (var config in r.ConfigurationKeys)
                {
                    Console.Write(config.GetHashCode());
                    Console.Write(" ");
                    Console.WriteLine(string.Join(" ", r.Distribution(config)));
                }
            }

            Console.WriteLine("original");
            print(originalSimulation.Rule);
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine("recreated");
            print(recreatedSimulation.Rule);
        }
        */
    }

    public void Advance()
    {
        originalSimulation.Advance();
        recreatedSimulation.Advance();

        var view = originalSimulation.GetCurrentStateView();
        originalView.SetState(view, originalSimulation.Rule.StatesCount);
        recreatedView.SetState(recreatedSimulation.GetCurrentStateView(), originalSimulation.Rule.StatesCount);
        if (states != null) states.Add(view);
    }


}
