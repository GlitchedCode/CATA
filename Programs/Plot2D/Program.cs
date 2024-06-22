using Simulation;
using Plotly.NET.CSharp;
using Plotly.NET.ImageExport;
using System.IO;

class Plot2DProgram
{
  void SaveStateToPNG(string path, int[][] zData)
  {

  }

  static void Main(string[] args)
  {
    // var tmpDir = Path.GetTempPath() + "/cata/";
    var tmpDir = "./cata/";
    try { Directory.Delete(tmpDir, true); } catch { }
    Directory.CreateDirectory(tmpDir);

    var simulation = new Model2D(100, 100);
  
    var neighborhood = new Moore(1);
    var lifeRule = new TotalisticRule(2, neighborhood, true);
    var two = new State[] { 
      new State(1, 1), 
      new State(1, 1),
      new State(1, 1), // 2
      new State(1, 0), 
      new State(1, 0),
      new State(1, 0),
      new State(1, 0),
      new State(1, 0),
      new State(1, 0),

    };
    var three = new State[] { 
      new State(1, 0), 
      new State(1, 1),
      new State(1, 1),
      new State(1, 1), // 3
      new State(1, 0),
      new State(1, 0),
      new State(1, 0),
      new State(1, 0),
      new State(1, 0),
    };

    // B3
    lifeRule.Increment(three, 1);
    // S23
    three[0].Value = 1;
    lifeRule.Increment(two, 1);
    lifeRule.Increment(three, 1);

    simulation.Rule = lifeRule;
    simulation.Randomize();

    for (int i = 0; i < 100; i++)
    {
      Console.WriteLine("Step " + i);
      simulation.Advance();
      Console.WriteLine("Step " + i + " done");
      var stateMat = simulation.GetCurrentStateView().ToMatrix();
      var floatMat = stateMat.Select(r => r.Select(s => (float)s.Value));
      Chart.Heatmap<float,int,int,string>(zData:floatMat).SavePNG(tmpDir + i);
    }

    simulation.Randomize();
  }
}
