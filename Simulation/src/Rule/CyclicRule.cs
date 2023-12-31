namespace Simulation;

public class CyclicRule : MetaRule
{
    SingleRule[] rotation;
    int currentIndex = 0;
    public SingleRule CurrentRule { get => rotation[currentIndex]; }

    public CyclicRule(IEnumerable<SingleRule> rotation)
    {
        SetRotation(rotation);
    }

    public void SetRotation(IEnumerable<SingleRule> rotation)
    {
        this.rotation = rotation.ToArray();
        currentIndex = 0;
    }

    public override void Advance()
        => currentIndex = (currentIndex + 1) % rotation.Length;

    public override int GetDefaultState() => CurrentRule.GetDefaultState();
    public override void SetDefaultState(int v) => CurrentRule.SetDefaultState(v);
    public override int GetStatesCount() => CurrentRule.GetStatesCount();
    public override void SetStatesCount(int v) => CurrentRule.SetStatesCount(v);
    public override int GetBitsCount() => CurrentRule.GetBitsCount();
    public override void SetBitsCount(int v) => CurrentRule.SetBitsCount(v);
    public override Neighborhood1D GetNeighborhood() => CurrentRule.GetNeighborhood();
    public override void SetNeighborhood(Neighborhood1D v) => CurrentRule.SetNeighborhood(v);


    public override State Get(State[] config) => CurrentRule.Get(config);
    public override double[] Distribution(State[] config) => CurrentRule.Distribution(config);
}
