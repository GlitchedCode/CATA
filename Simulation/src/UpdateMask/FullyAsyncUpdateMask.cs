namespace Simulation;

public class FullyAsyncUpdateMask<T> : UpdateMask {
  Random _rng;
  public Random RNG {
    get => _rng;
    set => _rng = value == null ? new() : value;
  }

  public bool Random = false;

  private Container.Array<T> State;
  private int currentIdx = 0;

  public FullyAsyncUpdateMask(
      Container.Array<T> state,
      bool random = false, Random rng = null){
    State = state;
    Random = random;
    RNG = rng; 
  }

  public override void Advance()
  {
    if(Random)
      currentIdx = RNG.Next(State.CellCount);
    else 
      currentIdx = (currentIdx + 1) % State.CellCount;
  }

  public override bool Get(int idx)
  {
    return idx == currentIdx;
  }
}
