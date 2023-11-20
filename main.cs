using Godot;
using System;
using System.Collections.Generic;
using Simulation;

public partial class main : Control
{
    SimulationView originalView, recreatedView;

    Model originalSimulation, recreatedSimulation;
    List<Grid2DView<State>> gridStates;

    Label diffLabel;

    [Export] SpriteFrames targetFrames;

    public override void _Ready()
    {
        GetNode<Godot.Timer>("TurnTimer").Timeout += onTurnTimeout;
        originalView = GetNode<SimulationView>("Original");
        recreatedView = GetNode<SimulationView>("Recreated");

        diffLabel = GetNode<Label>("labels/rules/difference");

        originalSimulation = new(100, 100);
        //originalSimulation.Randomize();

        recreatedSimulation = new(100, 100);



        if (targetFrames != null)
        {
            originalSimulation.Rule = AnalyzeVideo();
        }
        else
        {
            gridStates = new();
            gridStates.Add(originalSimulation.GetCurrentStateView());
            originalSimulation.Rule = RandomRule.Make(new VonNeumann(1), 2);
        }

        Advance();
    }

    Rule AnalyzeVideo()
    {
        var frames = targetFrames.GetFrameCount("default");
        var size = targetFrames.GetFrameTexture("default", 0).GetImage().GetSize();

        List<Grid2DView<State>> timeSeries = new();

        for (int frame = 0; frame < frames; frame++)
        {
            var img = targetFrames.GetFrameTexture("default", frame).GetImage();
            var grid = new Grid2DContainer<State>(size.Y, size.X, new State(1, 0));

            for (int x = 0; x < size.X; x++)
                for (int y = 0; y < size.Y; y++)
                {
                    var value = img.GetPixel(x, y).R > 0.7;
                    grid.Set(y, x, new State(1, value ? 1 : 0));
                }

            timeSeries.Add(grid.GetView());
        }

        return Analyzer.Analyze(timeSeries.ToArray());
    }

    void onTurnTimeout()
    {
        Advance();
        if (gridStates != null && gridStates.Count == 5)
        {
            var originalRule = originalSimulation.Rule;
            Console.WriteLine($"diff before analyze: {originalRule.AverageDifference(recreatedSimulation.Rule)}");
            var predicted = Analyzer.Analyze(gridStates.ToArray());
            recreatedSimulation.Rule = predicted;
            Console.WriteLine($"diff after analyze: {recreatedSimulation.Rule.AverageDifference(originalRule)}");
            originalSimulation.Randomize();
            recreatedSimulation.ResetState(originalSimulation.GetCurrentStateView());
            diffLabel.Text = $"difference: {recreatedSimulation.Rule.AverageDifference(originalRule)}";
            gridStates = null;


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
    }

    public void Advance()
    {
        originalSimulation.Advance();
        recreatedSimulation.Advance();

        var view = originalSimulation.GetCurrentStateView();
        originalView.SetState(view);
        recreatedView.SetState(recreatedSimulation.GetCurrentStateView());
        if (gridStates != null) gridStates.Add(view);
    }


}
