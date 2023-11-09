using Godot;
using System;

public partial class main : Control
{
    SimulationView currentView, previousView;

    Simulation simulation;

    public override void _Ready()
    {
        GetNode<Timer>("TurnTimer").Timeout += onTurnTimeout;
        currentView = GetNode<SimulationView>("Current");
        previousView = GetNode<SimulationView>("Previous");

        simulation = new(20, 20);
        simulation.Randomize();
        simulation.Rule = Rule.Random();
        Advance();

    }

    void onTurnTimeout()
    {
        Advance();
    }

    public void Advance()
    {
        simulation.Advance();
        currentView.SetState(simulation.GetCurrentStateView());
        previousView.SetState(simulation.GetPreviousStateView());
    }


}
