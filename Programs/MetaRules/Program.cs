// See https://aka.ms/new-console-template for more information
using Simulation;
using Plotly.NET.CSharp;
using Plotly.NET.LayoutObjects;
using Plotly.NET;
using Plotly.NET.ImageExport;

public class MetaRulesProgram
{
  static int Main(string[] args)
  {
    var simulation = new Model1D(200, 10);

    var higherOrderRule = new WolframRule(89);
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
    states.Reverse();
    foreach (var s in states)
      mat.Add(s.Select((state) => (float)state.Value / ((float)rule.CurrentRule.StatesCount-1)).ToArray());
    
    var axis = new LinearAxis();
    axis.SetValue("showbackground", false);
    axis.SetValue("showspikes", false);
    axis.SetValue("showline", false);
    axis.SetValue("showgrid", false);
    axis.SetValue("showticklabels", false);
    axis.SetValue("showexponent", false);
    axis.SetValue("showdividers", false);
    axis.SetValue("showtickprefix", false);
    axis.SetValue("showticksuffix", false);

    var layout = new Layout();
    layout.SetValue("xaxis", axis);
    layout.SetValue("yaxis", axis);
    layout.SetValue("showlegend", false);
    

    Plotly.NET.CSharp.Chart.Heatmap<float, int, int, string>(
        zData: mat.ToArray(),
        ShowLegend: false,
        ShowScale: false
        )
      .WithLayout(layout)
      .SavePNG("test", Width: 1600, Height: 1600);

    return 0;
  }
}
