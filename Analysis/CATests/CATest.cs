namespace Analysis;
using Simulation;

public interface CATest {

  bool Test(Simulation.Container.Array<State>.View[] dynamics);
}
