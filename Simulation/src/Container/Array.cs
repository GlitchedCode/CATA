namespace Simulation.Container;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class Array<T> : ICloneable
{
  protected readonly ConcurrentDictionary<int, T> map;
  public int CellCount { get; private set; }

  public readonly T DefaultValue;

  public BoundaryCondition BoundaryCondition { get; set; } = BoundaryCondition.Fixed;

  public Array(int cellCount, T sparseDefault,
               BoundaryCondition boundary = BoundaryCondition.Fixed)
  {
    if (cellCount < 0)
      throw new Exception("Invalid cell count");

    this.CellCount = cellCount;
    this.DefaultValue = sparseDefault;
    this.BoundaryCondition = boundary;

    map = new(4, cellCount);

    Resize(cellCount);
  }

  public Array(Array<T> other)
  {
    DefaultValue = other.DefaultValue;
    CellCount = other.CellCount;
    BoundaryCondition = other.BoundaryCondition;

    map = new ConcurrentDictionary<int, T>(other.map);
    Resize(CellCount);
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

  // Risolve un indice eventualmente fuori range secondo la BoundaryCondition.
  // Restituisce -1 se Fixed e l'indice è fuori range.
  private int ResolveIndex(int index)
  {
    if (index >= 0 && index < CellCount) return index;
    return BoundaryCondition switch
    {
      BoundaryCondition.Periodic   => ((index % CellCount) + CellCount) % CellCount,
      BoundaryCondition.Reflective => ReflectIndex(index, CellCount),
      _                            => -1,
    };
  }

  // Rispecchia l'indice i attorno ai bordi [0, n-1].
  protected static int ReflectIndex(int i, int n)
  {
    if (n <= 1) return 0;
    int period = 2 * (n - 1);
    i = ((i % period) + period) % period;
    return i < n ? i : period - i;
  }

  public T Get(int index)
  {
    int resolved = ResolveIndex(index);
    if (resolved < 0) return DefaultValue;
    return map.TryGetValue(resolved, out var v) ? v : DefaultValue;
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
    return new Array<T>(CellCount, DefaultValue, BoundaryCondition);
  }

  public object Clone()
  {
    var ret = new Array<T>(CellCount, DefaultValue, BoundaryCondition);

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
