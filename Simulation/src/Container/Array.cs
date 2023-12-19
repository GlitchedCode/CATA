namespace Simulation.Container;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class Array<T> : ICloneable
{
    private readonly ConcurrentDictionary<int, T> map;
    public int CellCount { get; private set; }

    public readonly T DefaultValue;

    public Array(int cellCount, T sparseDefault)
    {
        if (cellCount < 0)
            throw new Exception("Invalid cell count");

        this.CellCount = cellCount;
        this.DefaultValue = sparseDefault;

        map = new(4, cellCount);

        Resize(cellCount);
    }

    public void Resize(int cellCount)
    {
        if (cellCount < 0)
            throw new Exception("Invalid cell count");

        List<int> removed = new();
        foreach (var key in map.Keys)
            if (key >= cellCount || key < 0)
                removed.Add(key);

        foreach (var key in removed)
            map.Remove(key, out var _);

        this.CellCount = cellCount;
    }

    public T Get(int index)
    {
        try
        {
            return map[index];
        }
        catch
        {
            return DefaultValue;
        }
    }

    public void Set(int index, T element)
    {
        if (index < 0 || index >= CellCount)
            return;

        map[index] = element;
    }

    public void Remove(int index) => map.Remove(index, out var _);

    public void Clear() => map.Clear();

    public View GetView() => new View(this);


    public object Clone()
    {
        var ret = new Array<T>(CellCount, DefaultValue);

        foreach (var k in map.Keys)
            ret.Set(k, Get(k));

        return ret;
    }


    public class View
    {
        Array<T> container;

        public int CellCount => container.CellCount;

        public View(Array<T> cont) => container = cont;

        public T Get(int index) => container.Get(index);
    }


    public static void PrintMany(IEnumerable<Array<T>> states)
    {
        foreach (var state in states)
        {
            for (int i = 0; i < state.CellCount; i++)
                Console.Write(state.Get(i).ToString());
            Console.WriteLine();
        }
    }

    public static void PrintMany(IEnumerable<Array<State>> states, string charmap)
    {
        foreach (var state in states)
        {
            for (int i = 0; i < state.CellCount; i++)
                Console.Write(charmap[state.Get(i).Value]);
            Console.WriteLine();
        }
    }
}
