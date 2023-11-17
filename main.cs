using Godot;
using System;
using System.Collections.Generic;
using Simulation;

public partial class main : Control
{
    SimulationView originalView, recreatedView;

    Model originalSimulation, recreatedSimulation;
    List<Grid2DView<bool>> states;

    Label diffLabel;

    [Export] VideoStream targetVideo;

    public override void _Ready()
    {
        GetNode<Godot.Timer>("TurnTimer").Timeout += onTurnTimeout;
        originalView = GetNode<SimulationView>("Original");
        recreatedView = GetNode<SimulationView>("Recreated");

        diffLabel = GetNode<Label>("labels/rules/difference");

        originalSimulation = new(100, 100);
        //originalSimulation.Randomize();
        originalSimulation.Rule = Rule.Random(new VonNeumann(1));

        recreatedSimulation = new(100, 100);



        if (targetVideo != null)
        {
            originalSimulation.Rule = AnalyzeVideo();
        }
        else
        {
            states = new();
            states.Add(originalSimulation.GetCurrentStateView());
        }

        Advance();
    }

    Rule AnalyzeVideo()
    {
        const int FRAMERATE = 10;
        const double eps = 10e-2;
        var player = targetVideo._InstantiatePlayback();
        var frames = player._GetLength() / FRAMERATE;

        player._Seek(0);
        var size = player._GetTexture().GetImage().GetSize();

        List<Grid2DView<bool>> timeSeries = new();

        for (int frame = 0; frame < frames; frame++)
        {
            player._Seek(((double)frame / (double)FRAMERATE) + eps);
            var img = player._GetTexture().GetImage();
            var grid = new Grid2DContainer<bool>(size.Y, size.X, false);

            for (int x = 0; x < size.X; x++)
                for (int y = 0; y < size.Y; y++)
                {
                    var value = img.GetPixel(x, y).R > 0.5;
                    grid.Set(y, x, value);
                }

            timeSeries.Add(grid.GetView());
        }

        return Analyzer.Analyze(timeSeries.ToArray());
    }

    void onTurnTimeout()
    {
        Advance();
        if (states != null && states.Count == 5)
        {
            var originalRule = originalSimulation.Rule;
            Console.WriteLine($"diff before analyze: {originalRule.AverageDifference(recreatedSimulation.Rule)}");
            var predicted = Analyzer.Analyze(states.ToArray());
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
    }

    public void Advance()
    {
        originalSimulation.Advance();
        recreatedSimulation.Advance();

        var view = originalSimulation.GetCurrentStateView();
        originalView.SetState(view);
        recreatedView.SetState(recreatedSimulation.GetCurrentStateView());
        if (states != null) states.Add(view);
    }


}
