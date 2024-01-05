namespace Simulation;

public abstract class Neighborhood2D : Neighborhood
{
    int _rows = 1;
    public int Rows
    {
        get => _rows;
        set => _rows = Math.Max(1, value);
    }
    int _columns = 1;
    public int Columns
    {
        get => _columns;
        set => _columns = Math.Max(1, value);
    }
    protected int GetRowFromKey(int key) =>
            key / Columns;

    protected int GetColumnFromKey(int key) =>
        key % Columns;

    protected int GetKeyFromCoords(int row, int col) =>
        (row * Columns) + col;

    public abstract State[] Get(Container.Grid2D<State>[] states, int row, int column);
}