namespace Simulation;

public abstract class MetaRule
{
    public Rule CurrentRule { get => GetCurrentRule(0); }

    public abstract void Advance();
    public abstract Rule GetCurrentRule(int position);
}
