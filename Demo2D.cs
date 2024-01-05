using Godot;
using System.Text.Json;
using System.IO;

public partial class Demo2D : Control
{
    Simulation2DView originalView, recreatedView;

    Model2D originalSimulation, recreatedSimulation;
    List<Simulation.Container.Grid2D<State>.View> gridStates;

    Label diffLabel;

    [Export] SpriteFrames targetFrames;

    public override void _Ready()
    {
        GetNode<Godot.Timer>("TurnTimer").Timeout += onTurnTimeout;
        originalView = GetNode<Simulation2DView>("Original");
        recreatedView = GetNode<Simulation2DView>("Recreated");

        diffLabel = GetNode<Label>("labels/rules/difference");

        originalSimulation = new(100, 100, 100);
        originalSimulation.Randomize();
        gridStates = new() { originalSimulation.GetCurrentStateView() };
        originalSimulation.Rule = SingleRule.Random(new VonNeumann(1), 2);

        Advance();
    }

    Rule AnalyzeVideo()
    {
        var stateCount = 4;
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
        var graphDir = "graphs";
        var di = Directory.CreateDirectory(graphDir);
        foreach (FileInfo file in di.GetFiles())
            file.Delete();
        foreach (DirectoryInfo dir in di.GetDirectories())
            dir.Delete(true);

        var hood = ruleTimeSeries[0].Neighborhood;
        var xData = Enumerable.Range(0, ruleTimeSeries.Count())
            .Select(x => (double)x).ToArray();

        var cfgIdx = 0;
        foreach (var config in ruleTimeSeries[0].EnumerateConfigurations())
        {
            var list = new List<double>();

            var min = 1d;
            var max = 0d;

            for (int i = 0; i < ruleTimeSeries.Count(); i++)
            {
                var p = ruleTimeSeries[i].Distribution(config)[stateCount - 1];
                if (p < min) min = p;
                if (p > max) max = p;
                list.Add(p);
            }

            if (min > 0.05 ^ max < 0.95)
            {
                var plt = new ScottPlot.Plot();
                plt.Title(String.Join(", ", config));
                plt.AddScatterLines(xData, list.ToArray());
                plt.SaveFig($"{graphDir}/{cfgIdx}.png");
                cfgIdx++;
            }
        }

        var ret = ruleTimeSeries[0];
        return ruleTimeSeries.Skip(1).Aggregate(ret,
                                                (x, y) => x + y,
                                                r => r);
    }

    void onTurnTimeout()
    {
        Advance();
        if (false && gridStates != null && gridStates.Count == 5)
        {
            var originalRule = originalSimulation.Rule;
            Console.WriteLine($"diff before analyze: {originalRule.AverageDifference(recreatedSimulation.Rule as SingleRule)}");
            var predicted = Analyzer2D.SingleRule(gridStates.ToArray(), 2);
            recreatedSimulation.Rule = predicted;
            Console.WriteLine($"diff after analyze: {recreatedSimulation.Rule.AverageDifference(originalRule as SingleRule)}");
            originalSimulation.Randomize();
            recreatedSimulation.ResetState(originalSimulation.GetCurrentStateView());
            diffLabel.Text = $"difference: {recreatedSimulation.Rule.AverageDifference(originalRule as SingleRule)}";
            gridStates = null;


            void print(Rule r)
            {
                foreach (var config in r.EnumerateConfigurations())
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
    }

    public void Advance()
    {
        originalSimulation.Advance();
        //recreatedSimulation.Advance();

        var view = originalSimulation.GetCurrentStateView();
        originalView.SetState(view, originalSimulation.Rule.StatesCount);
        //recreatedView.SetState(recreatedSimulation.GetCurrentStateView(), originalSimulation.Rule.StatesCount);
        if (gridStates != null) gridStates.Add(view);
    }


}
