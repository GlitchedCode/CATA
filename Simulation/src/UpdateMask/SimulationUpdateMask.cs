namespace Simulation;

public class SimulationUpdateMask : UpdateMask {

  public Model1D ReferenceModel {get; protected set;}

  public SimulationUpdateMask(Model1D reference)
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
    return ReferenceModel.GetCurrentStateView().Get(idx).Value != 0;
  }
}
