
namespace Simulation;

public class TotalisticRule : Rule
{

  int _StatesCount;
  int _DefaultState;
  Neighborhood _Neighborhood;
  Random rng = new();
  Dictionary<int, StateCounter> counters = new();

  public bool Outer { get; private set; }

  public override int GetDefaultState() => _DefaultState;
  public override void SetDefaultState(int v) => _DefaultState = v;
  public override int GetStatesCount() => _StatesCount;
  public override void SetStatesCount(int v) => _StatesCount = v;
  public override int GetBitsCount() => (int)Math.Ceiling(Math.Log2(_StatesCount));
  public override void SetBitsCount(int v) => throw new NotImplementedException();
  public override IEnumerable<State[]> EnumerateConfigurations() => throw new NotImplementedException();

  public override Neighborhood GetNeighborhood() => _Neighborhood;
  public override void SetNeighborhood(Neighborhood v) {
    if(v == null) throw new ArgumentNullException();
    _Neighborhood = v;
  }

  public TotalisticRule(int statesCount, Neighborhood neighborhood, bool outer = false) {
    _StatesCount = statesCount;
    _DefaultState = 0;
    Outer = outer;
    _Neighborhood = neighborhood;
  }
  
  int idx(State[] configuration) {
    var cfg = Outer ? configuration.Skip(1) : configuration; 
    var idx = 0;
    foreach(var state in configuration)
      idx += state.Value;

    return Outer ? idx * (configuration[0].Value + 1) : idx;
  }

  public void Set(State[] configuration, double[] distribution) {
    if(distribution.Length != _StatesCount) throw new ArgumentException("Invalid distribution length, should equal state count");
    var i = idx(configuration);
    if(!counters.ContainsKey(i)) counters[i] = new StateCounter(_StatesCount);
    counters[i].Reset();
    for(int j = 0; j < _StatesCount; j++)
      counters[i].Increment(j, (uint)(distribution[j] * 1000));
  }

  public void Increment(State[] configuration, int state) {
    var i = idx(configuration);
    if(!counters.ContainsKey(i)) counters[i] = new StateCounter(_StatesCount);
    counters[i].Increment(state);
  }

  public override State Get(State[] configuration)
  {
    var i = idx(configuration);
    if(!counters.ContainsKey(i)) return new State(BitsCount, DefaultState); 
    return new State(BitsCount, counters[i].Get());
  }

  public override double[] Distribution(State[] configuration)
  {
    var i = idx(configuration);
    if(!counters.ContainsKey(i)) return Enumerable.Repeat(0.0, _StatesCount).ToArray();
    return counters[i].Distribution();
  } 
}
