

namespace Simulation
{
  public class RuleBuilder {
    static public Rule Totalistic(int stateCount, Neighborhood neighborhood, int[] outputs)
    {
      var ret = new SingleRule(stateCount);
      ret.Neighborhood = neighborhood;
      var neighborCount = neighborhood.Count();

      var expectedOutputs = neighborCount * (stateCount - 1);
      if(outputs.Count() < expectedOutputs)
        throw new ArithmeticException("Wrong output count for supplied neighborhood and state count");

      foreach(var config in neighborhood.EnumerateConfigurations(stateCount))
      {
        var total = (int)config.Sum((s) => s.Value);
        ret.Set(config, outputs[total]);
      }

      return ret;
    }

    static public Rule OuterTotalistic(int stateCount, Neighborhood neighborhood, int[] outputs)
    {
      var ret = new SingleRule(stateCount);
      ret.Neighborhood = neighborhood;
      var neighborCount = neighborhood.Count();

      var expectedOutputs = (neighborCount - 1) * (stateCount - 1) * stateCount;
      if(outputs.Count() < expectedOutputs)
        throw new ArithmeticException("Wrong output count for supplied neighborhood and state count");

      foreach(var config in neighborhood.EnumerateConfigurations(stateCount))
      {
        var state = config[0].Value;
        var total = (int)config.Skip(1).Sum((s) => s.Value);
        ret.Set(config, outputs[total * (state + 1)]);
      }

      return ret;
    }

  }
}
