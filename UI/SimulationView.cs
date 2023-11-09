using Godot;
using System;

public partial class SimulationView : Control
{
    [Export] Color deadColor = new Color(0, 0, 0);
    [Export] Color aliveColor = new Color(1, 1, 1);

    Grid2DView<bool> state = null;

    public void SetState(Grid2DView<bool> s)
    {
		state = s;
		QueueRedraw();
    }

    public override void _Draw()
    {
        var vRect = GetRect();
        DrawRect(vRect, deadColor);

        if (state != null)
        {
            var cellSize = new Vector2(vRect.Size.X / state.Columns,
                                       vRect.Size.Y / state.Rows);

            for (int r = 0; r < state.Rows; ++r)
                for (int c = 0; c < state.Columns; ++c)
                    if (state.Get(r, c))
                    {
                        var pos = new Vector2((c) * cellSize.X, (r) * cellSize.Y) + vRect.Position;
                        var rect = new Rect2(pos, cellSize);
                        DrawRect(rect, aliveColor);
                    }
        }
    }
}
