namespace Simulation;

public class SimulationUpdateMask : UpdateMask {

  public Model1D<Container.Array<State>> ReferenceModel {get; protected set;}

  public SimulationUpdateMask(Model1D<Container.Array<State>> reference)
  {
    if(reference == null)
      throw new NullReferenceException("reference model cannot be null");
    ReferenceModel = reference;
  }

  public override void Advance()
  {
    ReferenceModel.Advance();
  }

  public override bool Get(int idx)
  {
    return ReferenceModel.CurrentState.Get(idx).Value != 0;
  }
}
