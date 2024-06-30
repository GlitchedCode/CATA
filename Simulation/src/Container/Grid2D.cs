namespace Simulation.Container;

using System;
using System.Collections.Generic;

public class Grid2D<T> : Simulation.Container.Array<T>
{
    public int Rows { get; private set; }
    public int Columns { get; private set; }


    // Initializes the ArrayList objects
    public Grid2D(int rows, int cols, T sparseDefault) : base(rows * cols, sparseDefault)
    {
        if (rows <= 0 | cols <= 0) // sanity check
            throw new Exception("Invalid size values");

        this.Rows = rows;
        this.Columns = cols;

        Resize(rows, cols);
    }

    public Grid2D(int rows, int cols, Array<T> array) : base(array)
    {
        if (rows <= 0 | cols <= 0) // sanity check
            throw new Exception("Invalid size values");

        this.Rows = rows;
        this.Columns = cols;
        Resize(rows, cols);
    }

    // Resets the minimum capacity for all the ArrayList objects
    public void Resize(int rows, int cols)
    {
        if (rows <= 0 | cols <= 0) // sanity check
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

    // Gets element at (row, col) coordinates
    public T Get(int row, int col)
    {
        if (row >= Rows | col >= Columns | row < 0 | col < 0)
            return DefaultValue;

        return base.Get(GetKeyFromCoords(row, col));
    }

    // Puts element at (row, col) coordinates
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
        new Grid2D<T>(Rows, Columns, DefaultValue);

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
