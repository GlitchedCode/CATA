using System.Collections.Generic;
using Simulation;
namespace Analysis;

public class TotalisticTest : CATest {
  
  public Neighborhood neighborhood; 

  public  bool Test(Simulation.Container.Array<State>.View[] dynamics)
  {  
    var cellCount = dynamics[0].CellCount;
    var encountered = new Dictionary<int, int>();
    
    for(int i = 0; i < dynamics.Length - 1; i++)
    {
      for (int j = 0; j < cellCount; ++j)
      {
        var config = neighborhood.Get(dynamics[i], j);
        var next = dynamics[i+1].Get(j).Value;
        var total = (int)config.Sum(state => state.Value);
        if(encountered.TryGetValue(total, out int expected))
        {
          if(expected != next)
            return false;
        }
        else
          encountered[total] = next;
      }
    }

    return true;
  }
} 
