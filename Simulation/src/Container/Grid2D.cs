namespace Simulation.Container;

using System;
using System.Collections.Generic;

public class Grid2D<T> : Simulation.Container.Array<T>
{
    public int Rows { get; private set; }
    public int Columns { get; private set; }


    public Grid2D(int rows, int cols, T sparseDefault,
                  BoundaryCondition boundary = BoundaryCondition.Fixed)
        : base(rows * cols, sparseDefault, boundary)
    {
        if (rows <= 0 | cols <= 0)
            throw new Exception("Invalid size values");

        this.Rows = rows;
        this.Columns = cols;

        Resize(rows, cols);
    }

    public Grid2D(int rows, int cols, Array<T> array) : base(array)
    {
        if (rows <= 0 | cols <= 0)
            throw new Exception("Invalid size values");

        this.Rows = rows;
        this.Columns = cols;
        Resize(rows, cols);
    }

    public void Resize(int rows, int cols)
    {
        if (rows <= 0 | cols <= 0)
            throw new Exception("Invalid size values");

        Resize(rows * cols);

        List<int> removed = new();
        foreach (var key in map.Keys)
        {
            int row = GetRowFromKey(key);
            int col = GetColumnFromKey(key);
            if (row >= rows | col >= cols)
                removed.Add(key);
        }

        foreach (var key in removed)
            map.Remove(key, out var _);

        this.Rows = rows;
        this.Columns = cols;
    }

    // Applica la BoundaryCondition a una singola dimensione.
    private int WrapDim(int idx, int max) => BoundaryCondition switch
    {
        BoundaryCondition.Periodic   => ((idx % max) + max) % max,
        BoundaryCondition.Reflective => ReflectIndex(idx, max),
        _                            => idx,   // Fixed: ritorna invariato, il bounds check è in Get
    };

    // Gets element at (row, col) coordinates
    public T Get(int row, int col)
    {
        if (BoundaryCondition == BoundaryCondition.Fixed)
        {
            if (row < 0 || row >= Rows || col < 0 || col >= Columns)
                return DefaultValue;
            return base.Get(GetKeyFromCoords(row, col));
        }

        row = WrapDim(row, Rows);
        col = WrapDim(col, Columns);
        return base.Get(GetKeyFromCoords(row, col));
    }

    // Puts element at (row, col) coordinates (solo entro i bordi reali)
    public void Set(int row, int col, T element)
    {
        if (row >= Rows | col >= Columns | row < 0 | col < 0)
            return;

        base.Set(GetKeyFromCoords(row, col), element);
    }

    public void Remove(int row, int col) =>
        base.Remove(GetKeyFromCoords(row, col));

    private int GetRowFromKey(int key) =>
        key / Columns;

    private int GetColumnFromKey(int key) =>
        key % Columns;

    private int GetKeyFromCoords(int row, int col) =>
        (row * Columns) + col;

    public override Array<T> MakeNew() =>
        new Grid2D<T>(Rows, Columns, DefaultValue, BoundaryCondition);

    public T[][] ToMatrix() {
      var ret = new List<T[]>();
      for(int r = 0; r < Rows; r++)
      {
        var row = new List<T>();
        for(int c = 0; c < Columns; c++)
          row.Add(Get(r,c));
        ret.Add(row.ToArray());
      }
      return ret.ToArray();
    }

}
