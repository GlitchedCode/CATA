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
        var stateCount = 4;
        var bitCount = (int)Math.Ceiling(Math.Log2(stateCount));
        var stateWidth = 1d / (double)stateCount;

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
                    var value = img.GetPixel(x, y).R;
                    var state = Math.Clamp((int)Math.Floor(value / stateWidth), 0, stateCount - 1);
                    grid.Set(y, x, new State(bitCount, state));
                }

            timeSeries.Add(grid.GetView());
        }

        var ruleTimeSeries = Analyzer.TimeSeries(timeSeries.ToArray(), stateCount);

        var ret = ruleTimeSeries[0];
        return ruleTimeSeries.Skip(0).Aggregate(ret,
                                                (x, y) => x + y,
                                                r => r);
    }

    void onTurnTimeout()
    {
        Advance();
        if (gridStates != null && gridStates.Count == 5)
        {
            var originalRule = originalSimulation.Rule;
            Console.WriteLine($"diff before analyze: {originalRule.AverageDifference(recreatedSimulation.Rule)}");
            var predicted = Analyzer.SingleRule(gridStates.ToArray(), 2);
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
        originalView.SetState(view, originalSimulation.Rule.StatesCount);
        recreatedView.SetState(recreatedSimulation.GetCurrentStateView(), originalSimulation.Rule.StatesCount);
        if (gridStates != null) gridStates.Add(view);
    }


}
