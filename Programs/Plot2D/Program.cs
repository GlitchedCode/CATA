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
    Directory.CreateDirectory(tmpDir);

    var simulation = new Model2D(100, 100);
  
    var neighborhood = new VonNeumann(1);
    var lifeRule = RuleBuilder.OuterTotalistic(2, neighborhood, new int[]{0,0,0,1,0,0,0,0,0,0,0,1,1,0,0,0,0,0});

    simulation.Rule = lifeRule;
    simulation.Randomize();

    for (int i = 0; i < 100; i++)
    {
      simulation.Advance();
      var stateMat = simulation.GetCurrentStateView().ToMatrix();
      var floatMat = stateMat.Select(r => r.Select(s => (float)s.Value));
      Chart.Heatmap<float,int,int,string>(zData:floatMat).SavePNG("test.png");
    }

    simulation.Randomize();
  }
}
