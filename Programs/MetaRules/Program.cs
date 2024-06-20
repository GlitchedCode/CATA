// See https://aka.ms/new-console-template for more information
using Simulation;
using Plotly.NET.CSharp;
using Plotly.NET.ImageExport;

public class MetaRulesProgram
{
  static int Main(string[] args)
  {
    var simulation = new Model1D(200, 10);

    var higherOrderRule = TableRule.Random(new Radius1D(1), 2);
    var rule = new Model1DRule(higherOrderRule, new Radius1D(2));
    rule.Randomize();
    simulation.Rule = rule;
    simulation.Randomize();

    var states = new List<Simulation.State[]>();
    states.Add(simulation.GetCurrentStateView().ToArray());
    for(int i = 0; i < 200; i++)
    {
      simulation.Advance();
      rule.Advance();
      states.Add(simulation.GetCurrentStateView().ToArray());
    }

    var mat = new List<float[]>();
    foreach (var s in states)
      mat.Add(s.Select((state) => (float)state.Value / ((float)rule.CurrentRule.StatesCount-1)).ToArray());

    Chart.Heatmap<float, int, int, string>(
      zData: mat.ToArray()
    ).SavePNG("test");

    return 0;
  }
}
