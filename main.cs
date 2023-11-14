using Godot;
using System;
using System.Collections.Generic;
using Simulation;

public partial class main : Control
{
    SimulationView originalView, recreatedView;

    Model originalSimulation, recreatedSimulation;
    List<Grid2DView<bool>> states = new();

    Label diffLabel;

    public override void _Ready()
    {
        GetNode<Godot.Timer>("TurnTimer").Timeout += onTurnTimeout;
        originalView = GetNode<SimulationView>("Original");
        recreatedView = GetNode<SimulationView>("Recreated");

        diffLabel = GetNode<Label>("labels/rules/difference");

        originalSimulation = new(40, 40);
        originalSimulation.Randomize();
        originalSimulation.Rule = Rule.Random();

        recreatedSimulation = new(40, 40);

        Console.WriteLine($"diff before analyze: {originalSimulation.Rule.Difference(recreatedSimulation.Rule)}");

        states.Add(originalSimulation.GetCurrentStateView());
        Advance();
    }

    void onTurnTimeout()
    {
        Advance();
        if (states != null && states.Count == 20)
        {
            var predicted = Analyzer.Analyze(states.ToArray());
            recreatedSimulation.Rule = predicted;
            originalSimulation.Randomize();
            recreatedSimulation.ResetState(originalSimulation.GetCurrentStateView());
            diffLabel.Text = $"difference: {predicted.Difference(originalSimulation.Rule)}";
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
