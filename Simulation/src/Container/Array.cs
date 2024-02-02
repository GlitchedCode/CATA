namespace Simulation.Container;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class Array<T> : ICloneable
{
  protected readonly ConcurrentDictionary<int, T> map;
  public int CellCount { get; private set; }

  public readonly T DefaultValue;

  public Array(int cellCount, T sparseDefault)
  {
    if (cellCount < 0)
      throw new Exception("Invalid cell count");

    this.CellCount = cellCount;
    this.DefaultValue = sparseDefault;

    map = new(4, cellCount);
    _view = new View(this);

    Resize(cellCount);
  }

  public Array(Array<T> other)
  {
    map = other.map;
    DefaultValue = other.DefaultValue;
    CellCount = other.CellCount;
    _view = new View(this);
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
    if(map.ContainsKey(index))
      return map[index];
    else
      return DefaultValue;
  }

  public void Set(int index, T element)
  {
    if (index < 0 || index >= CellCount)
      return;

    map[index] = element;
  }

  public void Remove(int index) => map.Remove(index, out var _);

  public virtual void Clear() => map.Clear();

  private View _view;
  public View GetView() => _view;


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
    public T DefaultValue => container.DefaultValue;

    public View(Array<T> cont) => container = cont;

    public T Get(int index) => container.Get(index);

    public void Print(string charmap = null)
    {
      if(charmap != null)
        container.Print(charmap);
      else
        container.Print();
    }
  }

  public void Print()
  {
    string line = "";
    for (int i = 0; i < CellCount; i++)
      line += Get(i).ToString();
    Console.WriteLine(line);
  }

  public void Print(string charmap)
  {
    var self = this as Array<State>;
    string line = "";
    for (int i = 0; i < CellCount; i++)
      line += charmap[self.Get(i).Value];
    Console.WriteLine(line);
  }

  public static void PrintMany(IEnumerable<Array<T>.View> states)
  {
    foreach (var state in states)
      state.Print();
  }

  public static void PrintMany(IEnumerable<Array<State>.View> states, string charmap)
  {
    foreach (var state in states)
      state.Print(charmap);
  }
}
