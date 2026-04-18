namespace Simulation.Container;

public enum BoundaryCondition
{
    /// <summary>
    /// Celle fuori griglia restituiscono il valore di default (bordo assorb./fisso).
    /// </summary>
    Fixed,

    /// <summary>
    /// Bordo periodico (toroidale): gli indici si avvolgono ciclicamente.
    /// </summary>
    Periodic,

    /// <summary>
    /// Bordo riflessivo (specchio): gli indici vengono rispecchiati attorno al bordo.
    /// </summary>
    Reflective,
}
