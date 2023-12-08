namespace Simulation.Container;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;


public class Grid2D<T>
{
    private readonly ConcurrentDictionary<int, T> map;
    public int Rows { get; private set; }
    public int Columns { get; private set; }

    public readonly T DefaultValue;

    // Initializes the ArrayList objects
    public Grid2D(int rows, int cols, T sparseDefault)
    {
        if (rows < 0 | cols < 0) // sanity check
            throw new Exception("Invalid size values");

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

    public void Clear() => map.Clear();

    public View GetView() => new View(this);

    private static int GetRowFromKey(int key) =>
        key >> 16;

    private static int GetColumnFromKey(int key) =>
        key & 0x0000FFFF;

    private static int GetKeyFromCoords(int row, int col) =>
        row << 16 | col;


    public class View
    {
        Grid2D<T> container;

        public int Rows => container.Rows;
        public int Columns => container.Columns;

        public View(Grid2D<T> cont) => container = cont;

        public T Get(int row, int col) =>
            container.Get(row, col);


        private static int GetRowFromKey(int key) =>
            key >> 16;

        private static int GetColumnFromKey(int key) =>
            key & 0x0000FFFF;

        private static int GetKeyFromCoords(int row, int col) =>
            row << 16 | col;
    }
}
