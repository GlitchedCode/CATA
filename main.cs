using Godot;
using System;
using System.Collections.Generic;

public partial class main : Control
{
    SimulationView currentView, previousView;

    Simulation simulation;
    List<Grid2DView<bool>> states = new();

    Label chosenLabel, predictedLabel;

    public override void _Ready()
    {
        GetNode<Timer>("TurnTimer").Timeout += onTurnTimeout;
        currentView = GetNode<SimulationView>("Current");
        previousView = GetNode<SimulationView>("Previous");

        chosenLabel = GetNode<Label>("labels/rules/chosen");
        predictedLabel = GetNode<Label>("labels/rules/predicted");

        simulation = new(40, 40);
        simulation.Randomize();
        simulation.Rule = Rule.Random();
        chosenLabel.Text = $"chosen: {simulation.Rule.Bits}";

        states.Add(simulation.GetCurrentStateView());
        Advance();
    }

    void onTurnTimeout()
    {
        Advance();
        if (states.Count == 5)
        {
            var predicted = Analyzer.Analyze(states.ToArray());
            predictedLabel.Text = $"predicted: {predicted.Bits}";
        }
    }

    public void Advance()
    {
        simulation.Advance();
        var view = simulation.GetCurrentStateView();
        currentView.SetState(view);
        previousView.SetState(simulation.GetPreviousStateView());
        states.Add(view);
    }


}
