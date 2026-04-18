namespace Simulation;

/// <summary>
/// Meta-regola che varia la funzione di adiacenza in base alla densità locale.
/// Per ogni cella, conta le celle vive in una finestra 5×5; se la densità
/// supera la soglia usa la regola "densa" (tipicamente Moore), altrimenti
/// usa la regola "sparsa" (tipicamente VonNeumann o raggio minore).
/// Implementa m: (t, p, L) → (r, a) variando sia r che a per cella.
/// </summary>
public class DensityNeighborhoodMetaRule : MetaRule
{
    readonly Rule _denseRule;
    readonly Rule _sparseRule;
    readonly float _threshold;
    readonly int _rows;
    readonly int _cols;

    /// <summary>
    /// Snapshot dello stato precedente da cui leggere la densità.
    /// Deve essere aggiornato dal chiamante prima di ogni Advance().
    /// </summary>
    public Container.Grid2D<State>? ObservedState { get; set; }

    /// <param name="denseRule">Regola usata dove densità >= soglia.</param>
    /// <param name="sparseRule">Regola usata dove densità &lt; soglia.</param>
    /// <param name="threshold">Frazione di celle vive nella finestra 5×5 [0,1].</param>
    /// <param name="rows">Righe della griglia esterna.</param>
    /// <param name="cols">Colonne della griglia esterna.</param>
    public DensityNeighborhoodMetaRule(
        Rule denseRule, Rule sparseRule,
        float threshold, int rows, int cols)
    {
        _denseRule  = denseRule;
        _sparseRule = sparseRule;
        _threshold  = threshold;
        _rows       = rows;
        _cols       = cols;
    }

    public override Rule GetCurrentRule(int position)
    {
        if (ObservedState is null) return _denseRule;

        int row = position / _cols;
        int col = position % _cols;

        int alive = 0;
        const int R = 2;
        for (int dr = -R; dr <= R; dr++)
        for (int dc = -R; dc <= R; dc++)
            alive += ObservedState.Get(row + dr, col + dc).Value;

        float density = alive / 25f;
        return density >= _threshold ? _denseRule : _sparseRule;
    }

    public override void Advance() { }
}
