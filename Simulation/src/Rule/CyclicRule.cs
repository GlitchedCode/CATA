namespace Simulation;

public class CyclicRule : MetaRule
{
    Rule[] rotation;
    int currentIndex = 0;

    public override Rule GetCurrentRule(int position)
        => rotation[currentIndex];

    public CyclicRule(IEnumerable<Rule> rotation)
    {
        SetRotation(rotation);
    }

    public void SetRotation(IEnumerable<Rule> rotation)
    {
        this.rotation = rotation.ToArray();
        currentIndex = 0;
    }

    public override void Advance()
        => currentIndex = (currentIndex + 1) % rotation.Length;

}
