using System;

public interface Neighborhood
{
    public int Count(int radius);
    public T[] Get<T>(Grid2DView<T> state, int row, int column, int radius = -1);
}

public class VonNeumann : Neighborhood
{
    public uint Radius = 1;

    public VonNeumann(uint radius = 1) => this.Radius = radius;

    public int Count(int radius)
    {
        var ret = 1 + (radius * 4);
        for (int i = 1; i < radius; i++) ret += i;
        return ret;
    }

    public T[] Get<T>(Grid2DView<T> state, int row, int column, int radius = -1)
    {
        if(radius < 0) radius = (int)this.Radius;

        var count = Count(radius);
        T[] neighborhood = new T[count];

        neighborhood[0] = state.Get(row, column);
        int idx = 1;
        // cross
        for (int i = 1; i <= radius; i++)
        {
            Console.WriteLine(i);
            neighborhood[idx] = state.Get(row + i, column);
            idx++;
            neighborhood[idx] = state.Get(row - i, column);
            idx++;
            neighborhood[idx] = state.Get(row, column + i);
            idx++;
            neighborhood[idx] = state.Get(row, column - i);
            idx++;
        }

        // quadrants
        for (int c = 1; c < radius; c++)
            for (int r = c - radius; r < 0; r++)
            {
                neighborhood[idx] = state.Get(row + r, column + c);
                idx++;
                neighborhood[idx] = state.Get(row + r, column - c);
                idx++;
                neighborhood[idx] = state.Get(row - r, column + c);
                idx++;
                neighborhood[idx] = state.Get(row - r, column - c);
                idx++;
            }

        return neighborhood;
    }
}
