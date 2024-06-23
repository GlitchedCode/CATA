namespace Simulation;

public class Model1DToRule : Rule {
  Model1D simulation;
  Neighborhood _neighborhood;
  
  public Model1DToRule(Model1D simulation, Neighborhood neighborhood) {
    this._neighborhood = neighborhood;
    this.simulation = simulation;
    this.StatesCount = simulation.Rule.CurrentRule.StatesCount;
  }

  int GetIndex(State[] cfg)
  {
    var ret = 0;
    for(int i = 0; i < cfg.Length; i++)
      ret += cfg[i].Value * Enumerable.Repeat(StatesCount, i)
        .Aggregate(1, (a, b) => a*b);
    return ret;
  }  
  
  public override State Get(State[] configuration) {
    return simulation.GetCurrentStateView().Get(GetIndex(configuration));
  }

  public override void SetNeighborhood(Neighborhood v) => _neighborhood = v;
  public override Neighborhood GetNeighborhood() => _neighborhood;
  public override int GetStatesCount() => simulation.Rule.CurrentRule.StatesCount;
  public override int GetBitsCount() => simulation.Rule.CurrentRule.BitsCount;
  public override int GetDefaultState() => simulation.Rule.CurrentRule.DefaultState;

  public override double[] Distribution(State[] configuration) => throw new NotImplementedException();
  public override IEnumerable<State[]> EnumerateConfigurations() => throw new NotImplementedException();
  public override void SetBitsCount(int v) {}
  public override void SetDefaultState(int v) {}
  public override void SetStatesCount(int v) {}
}

public class Model1DRule : MetaRule
{
  int defaultState;
  Model1D simulation;
  Model1DToRule simRule;

  public Model1DRule(Rule rule, Neighborhood neighborhood)
  {
    var cellCount = Enumerable
      .Repeat(rule.StatesCount, (int)neighborhood.Count())
      .Aggregate(1, (a, b) => a * b);
    simulation = new Model1D(cellCount, 10);
    simulation.Rule = rule;
    simRule = new(simulation, neighborhood);
  }

  public void SetInnerNeighborhood(Neighborhood v) => simRule.Neighborhood = v;
  public override void Advance() => simulation.Advance();
  public void Randomize() => simulation.Randomize();
  public override Rule GetCurrentRule(int position) => simRule;
}
