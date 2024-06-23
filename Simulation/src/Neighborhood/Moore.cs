namespace Simulation;

public class Moore : Neighborhood2D 
{
  public uint Radius = 1;

  public Moore(uint radius = 1, uint lookBack = 0, int rows = 1, int cols = 1)
  {
    this.Rows = rows;
    this.Columns = cols;
    this.Radius = radius;
    this.LookBack = lookBack;
  }

    public override uint Count()
    {
      uint ret = 1;
      for(uint i = 1; i <= Radius; i++) ret += i * 8;
      return ret * (LookBack + 1);
    }

    public override State[] Get(Container.Array<State>.View[] states, int index)
    {
      var count = Count();
      State[] configuration = new State[count];
      var segment = new ArraySegment<Container.Array<State>.View>
        (states, 0, Math.Min((int)LookBack + 1, states.Length));

      int idx = 0;
      
      int row = GetRowFromKey(index);
      int column = GetColumnFromKey(index);
     
      foreach (var state in segment)
      {
        configuration[idx] = state.Get(index);
        idx++;

        for (int i = 1; i <= Radius; i++)
        {
          // top and bottom
          for (int j = -(i-1); j < i; j++)
          {
            configuration[idx] = state.Get(GetKeyFromCoords(row + i, column + j));
            idx++;

            configuration[idx] = state.Get(GetKeyFromCoords(row - i, column + j));
            idx++;
          }

          // left and right
          for (int j = -(i-2); j < i+1; j++)
          {
            configuration[idx] = state.Get(GetKeyFromCoords(row + j, column + i));
            idx++;

            configuration[idx] = state.Get(GetKeyFromCoords(row + j, column - i));
            idx++;
          }

        }
      }

      return configuration;
    }

    public override State[] Get(Container.Grid2D<State>[] states, int row, int column)
    {
      var count = Count();
      State[] configuration = new State[count];
      var segment = new ArraySegment<Container.Grid2D<State>>
        (states, 0, Math.Min((int)LookBack + 1, states.Length));

      int idx = 0;

      foreach (var state in segment)
      {
        configuration[idx] = state.Get(row, column);
        idx++;

        for (int i = 1; i <= Radius; i++)
        {
          // top and bottom
          for (int j = -(i-1); j < i; j++)
          {
            configuration[idx] = state.Get(row + i, column + j);
            idx++;

            configuration[idx] = state.Get(row - i, column + j);
            idx++;
          }

          // left and right
          for (int j = -(i-2); j < i+1; j++)
          {
            configuration[idx] = state.Get(row + j, column + i);
            idx++;

            configuration[idx] = state.Get(row + j, column - i);
            idx++;
          }

        }
      }

      return configuration;
    }
}
