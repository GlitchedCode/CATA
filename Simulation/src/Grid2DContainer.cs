namespace Simulation;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;


public class Grid2DContainer<T>
{

    private readonly ConcurrentDictionary<int, T> map;
    public int Rows { get; private set; }
    public int Columns { get; private set; }

    public readonly T DefaultValue;

    // Initializes the ArrayList objects
    public Grid2DContainer(int rows, int cols, T sparseDefault)
    {
        if (rows < 0 | cols < 0) // sanity check
            throw new Exception("Invalid size values for TwoDimensionalArrayList");

        this.Rows = rows;
        this.Columns = cols;
        this.DefaultValue = sparseDefault;

        map = new(4, rows * cols);

        Resize(rows, cols);
    }

    // Resets the minimum capacity for all the ArrayList objects
    public void Resize(int rows, int cols)
    {
        if (rows < 0 | cols < 0) // sanity check
            throw new Exception("Invalid size values for TwoDimensionalArrayList");

        List<long> keys = new();
        foreach (var key in map.Keys)
        {
            int row = GetRowFromKey(key);
            int col = GetColumnFromKey(key);
            if (row >= rows | col >= cols)
                keys.Add(key);
        }

        foreach (var key in map.Keys)
            map.Remove(key, out var _);

        this.Rows = rows;
        this.Columns = cols;
    }

    // Gets element at (row, col) coordinates
    public T Get(int row, int col)
    {
        int key = row << 16 | col;
        try
        {
            return map[key];
        }
        catch
        {
            return DefaultValue;
        }
    }

    // Puts element at (row, col) coordinates
    public void Set(int row, int col, T element)
    {
        if (row >= Rows | col >= Columns | row < 0 | col < 0)
            return;

        map[row << 16 | col] = element;
    }

    public void Remove(int row, int col) =>
        map.Remove(row << 16 | col, out var _);

    public void Clear()
    {
        map.Clear();
        Resize(Rows, Columns);
    }

    public Grid2DView<T> GetView() => new Grid2DView<T>(this);

    private static int GetRowFromKey(int key) =>
        key >> 16;

    private static int GetColumnFromKey(int key) =>
        key & 0x0000FFFF;

    private static int GetKeyFromCoords(int row, int col) =>
        row << 16 | col;

}

public class Grid2DView<T>
{
    Grid2DContainer<T> container;

    public int Rows => container.Rows;
    public int Columns => container.Columns;

    public Grid2DView(Grid2DContainer<T> cont) => container = cont;

    public T Get(int row, int col) =>
        container.Get(row, col);


    private static int GetRowFromKey(int key) =>
        key >> 16;

    private static int GetColumnFromKey(int key) =>
        key & 0x0000FFFF;

    private static int GetKeyFromCoords(int row, int col) =>
        row << 16 | col;
}
