using Simulation;
using Plotly.NET.LayoutObjects;
using Plotly.NET.ImageExport;
using Plotly.NET;
using Plotly.NET.CSharp;

class WolframDemoProgram {

  static void Main(string[] args) {
    var simulation = new Model1D(300);
    var rule = new WolframRule(30);
    simulation.Rule = rule;
    simulation.Randomize();
    
    var states = new List<Simulation.State[]>();
    states.Add(simulation.GetCurrentStateView().ToArray());
    for (int i = 0; i < 300; i++) {
      Console.WriteLine(i);
      simulation.Advance();
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
        ShowScale: false, 
        XGap: 10,
        YGap: 10
        )
      .WithLayout(layout)
      .SavePNG("test");

  }
}
