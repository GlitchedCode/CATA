using Godot;
using System;
using Simulation;

public partial class SimulationView : Control
{
    [Export] Color deadColor = new Color(0, 0, 0);
    [Export] Color aliveColor = new Color(1, 1, 1);

    Grid2DView<State> gridState = null;

    public void SetState(Grid2DView<State> s)
    {
        gridState = s;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var vRect = GetRect();
        DrawRect(vRect, deadColor);

        if (gridState != null)
        {
            var cellSize = new Vector2(vRect.Size.X / gridState.Columns,
                                       vRect.Size.Y / gridState.Rows);

            for (int r = 0; r < gridState.Rows; ++r)
                for (int c = 0; c < gridState.Columns; ++c)
                {
                    var state = gridState.Get(r, c);
                    var value = state.Value;
                    if (value != 0)
                    {
                        var pos = new Vector2((c) * cellSize.X, (r) * cellSize.Y) + vRect.Position;
                        var rect = new Rect2(pos, cellSize);
                        DrawRect(rect, aliveColor);
                    }
                }
        }
    }
}
