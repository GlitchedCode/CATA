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

    Resize(cellCount);
  }

  public Array(Array<T> other)
  {
    map = other.map;
    DefaultValue = other.DefaultValue;
    CellCount = other.CellCount;
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


  public virtual Array<T> MakeNew() {
    return new Array<T>(CellCount, DefaultValue);
  } 

  public object Clone()
  {
    var ret = new Array<T>(CellCount, DefaultValue);

    foreach (var k in map.Keys)
      ret.Set(k, Get(k));

    return ret;
  }

  public T[] ToArray()
  {
    var vals = new List<T>();
    for(int i = 0; i < CellCount; i++)
      vals.Add(Get(i));
    return vals.ToArray();
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

  public static void PrintMany(IEnumerable<Array<T>> states)
  {
    foreach (var state in states)
      state.Print();
  }

  public static void PrintMany(IEnumerable<Array<State>> states, string charmap)
  {
    foreach (var state in states)
      state.Print(charmap);
  }
}
