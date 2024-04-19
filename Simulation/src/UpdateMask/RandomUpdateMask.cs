
namespace Simulation
{

  public class RandomUpdateMask : UpdateMask {

    Random _rng;
    public Random RNG {
      get => _rng;
      set => _rng = value == null ? new() : value;
    }

    double _probability;
    public double Probability {
      get => _probability;
      set => _probability = Math.Max(0, Math.Min(1, value));
    }

    public RandomUpdateMask(float probability, Random rng = null){
      RNG = rng; 
    }

    public override bool Get(int idx)
    {
      return RNG.NextDouble() > Probability;
    }
  }

}
