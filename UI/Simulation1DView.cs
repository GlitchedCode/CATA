using Godot;

using ArrayStateView = Simulation.Container.Array<Simulation.State>.View;

public partial class Simulation1DView : Control
{
    [Export] Color deadColor = new Color(0, 0, 0);
    [Export] Color aliveColor = new Color(1, 1, 1);

    List<ArrayStateView> history = new();
    int stateCount;

    public void SetState(ArrayStateView s, int stateCount)
    {
        history.Insert(0, s);

        var diff = history.Count - s.CellCount;
        if(diff > 0)
            history.RemoveRange(s.CellCount, diff);

        this.stateCount = stateCount;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var vRect = GetRect();
        DrawRect(vRect, deadColor);

        if (history != null)
        {
            var cellCount = history[0].CellCount;
            var cellSize = new Vector2(vRect.Size.X / cellCount,
                                       vRect.Size.Y / cellCount);

            for (int r = 0; r < history.Count; ++r)
                for (int c = 0; c < history[0].CellCount; ++c)
                {
                    var state = history[r].Get(c);
                    var value = state.Value;
                    if (value > 0)
                    {
                        var pos = new Vector2(c * cellSize.X,
                                (cellCount - r - 1) * cellSize.Y) + vRect.Position;
                        var rect = new Rect2(pos, cellSize);

                        var t = (float)value / (float)(stateCount - 1);
                        var color = deadColor.Lerp(aliveColor, t);

                        DrawRect(rect, color);
                    }
                }
        }
    }
}
