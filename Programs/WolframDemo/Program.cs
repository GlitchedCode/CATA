using Simulation;
using Plotly.NET.CSharp;
using Plotly.NET.ImageExport;

class WolframDemoProgram {
  static void Main(string[] args) {
    var simulation = new Model1D(300);
    var rule = new WolframRule(110);
    simulation.Rule = rule;
    simulation.Randomize();
    
    var states = new List<Simulation.State[]>();
    states.Add(simulation.GetCurrentStateView().ToArray());
    for (int i = 0; i < 500; i++) {
      Console.WriteLine(i);
      simulation.Advance();
      states.Add(simulation.GetCurrentStateView().ToArray());
    }

    var mat = new List<float[]>();
    states.Reverse();
    foreach (var s in states)
      mat.Add(s.Select((state) => (float)state.Value / ((float)rule.CurrentRule.StatesCount-1)).ToArray());

    Chart.Heatmap<float, int, int, string>(
        zData: mat.ToArray()
        ).SavePNG("test");

  }
}
