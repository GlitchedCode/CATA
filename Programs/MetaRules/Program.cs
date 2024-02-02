// See https://aka.ms/new-console-template for more information
using Simulation;

public class MetaRulesProgram
{
  static int Main(string[] args)
  {
    var simulation = new Model1D(200, 10);
    simulation.Randomize();

    var higherOrderRule = SingleRule.Random(new Radius1D(1), 2);
    var rule = new Model1DRule(higherOrderRule, new Radius1D(2));
    rule.Randomize();
    simulation.Rule = rule;

    for(int i = 0; i < 80; i++)
    {
      simulation.GetCurrentStateView().Print(".vx#");
      simulation.Advance();
      rule.Advance();
    }
    simulation.GetCurrentStateView().Print(".vx#");


    return 0;
  }
}
