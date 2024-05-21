using System.Collections.Generic;
using Simulation;
namespace Analysis;

public class OuterTotalisticTest : CATest {
  
  public Neighborhood neighborhood; 
  public bool outer = false;

  public  bool Test(Simulation.Container.Array<State>.View[] dynamics)
  {
    var cellCount = dynamics[0].CellCount;
    var encountered = new Dictionary<(int, int), int>();

    for(int i = 0; i < dynamics.Length - 1; i++)
    {
      for (int j = 0; j < cellCount; ++j)
      {
        var config = neighborhood.Get(dynamics[i], j);
        var next = dynamics[i+1].Get(j).Value;
        var total = (int)config.Skip(1).Sum(state => state.Value);
        var key = (config[0].Value, total);
        if(encountered.TryGetValue(key, out int expected))
        {
          if(expected != next)
            return false;
        }
        else
          encountered[key] = next;
      }
    }

    return true;
  }
} 
