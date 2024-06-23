namespace Simulation;
   

public class WolframRule : Rule
{
  Radius1D _neighborhood;
  int _rule;

  public override int GetDefaultState() => 0;
  public override void SetDefaultState(int v) {}
  public override int GetStatesCount() => 2;
  public override void SetStatesCount(int v) {}
  public override int GetBitsCount() => 1;
  public override void SetBitsCount(int v) {}
  public override Neighborhood GetNeighborhood() => _neighborhood;
  public override void SetNeighborhood(Neighborhood v) {}
  public override IEnumerable<State[]> EnumerateConfigurations() => _neighborhood.EnumerateConfigurations(2);

  public WolframRule(int rule)
  {
    _neighborhood = new Radius1D(1);
    _rule = rule;
  }

  public override State Get(State[] configuration)
  {
    int state = configuration[1].Value << 2 | 
      configuration[0].Value << 1 | 
      configuration[2].Value;
    return new State(1, _rule >> state & 1);
  }

  public override double[] Distribution(State[] configuration)
  {
    int state = configuration[1].Value << 2 | 
      configuration[0].Value << 1 | 
      configuration[2].Value;
    var result = _rule >> state & 1;
    return new double[] { 1.0 - result, result };
  }
}
