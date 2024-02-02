namespace Simulation;

public class Model1DRule : MetaRule
{
  int defaultState;
  Model1D simulation;
  Neighborhood neighborhood;

  public Model1DRule(Rule rule, Neighborhood neighborhood)
  {
    var cellCount = Enumerable
      .Repeat(rule.StatesCount, (int)neighborhood.Count())
      .Aggregate(1, (a, b) => a * b);
    simulation = new Model1D(cellCount, 10);
    simulation.Rule = rule;
    Neighborhood = neighborhood;
  }

  public override int GetDefaultState() => defaultState;
  public override void SetDefaultState(int v) => defaultState = v;
  public override int GetStatesCount() => simulation.Rule.StatesCount;
  public override void SetStatesCount(int v)
  {
    throw new NotImplementedException();
  }

  public override int GetBitsCount() => simulation.Rule.BitsCount;
  public override void SetBitsCount(int v)
  {
    throw new NotImplementedException();
  }

  public override Neighborhood GetNeighborhood() => neighborhood;
  public override void SetNeighborhood(Neighborhood v)
  {
    neighborhood = v;

  }
  public override IEnumerable<State[]> EnumerateConfigurations()
  {
    return neighborhood.EnumerateConfigurations(StatesCount);
  }

  public override void Advance()
  {
    simulation.Advance();
  }

  int GetIndex(State[] cfg)
  {
    var ret = 0;
    for(int i = 0; i < cfg.Length; i++)
      ret += cfg[i].Value * Enumerable.Repeat(StatesCount, i)
        .Aggregate(1, (a, b) => a*b);
    return ret;
  }

  public override State Get(State[] configuration)
  {
    return simulation.GetCurrentStateView().Get(GetIndex(configuration));
  }

  public override double[] Distribution(State[] configuration)
  {
    throw new NotImplementedException();
  }

  public override double AverageDifference(SingleRule other)
  {
    throw new NotImplementedException();
  }

  public override double AverageVariance()
  {
    throw new NotImplementedException();
  }

  public void Randomize() => simulation.Randomize();
}
