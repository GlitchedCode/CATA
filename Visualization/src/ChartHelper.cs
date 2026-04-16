namespace Visualization;

using Simulation;
using Plotly.NET;
using Plotly.NET.CSharp;
using Plotly.NET.LayoutObjects;
using Plotly.NET.ImageExport;

public static class ChartHelper
{
    /// <summary>
    /// Builds a Plotly layout with all axis decorations hidden (no ticks, labels, grid).
    /// </summary>
    public static Layout BuildCleanLayout()
    {
        static LinearAxis BlankAxis()
        {
            var ax = new LinearAxis();
            ax.SetValue("showbackground", false);
            ax.SetValue("showspikes", false);
            ax.SetValue("showline", false);
            ax.SetValue("showgrid", false);
            ax.SetValue("showticklabels", false);
            ax.SetValue("showexponent", false);
            ax.SetValue("showdividers", false);
            ax.SetValue("showtickprefix", false);
            ax.SetValue("showticksuffix", false);
            return ax;
        }

        var layout = new Layout();
        layout.SetValue("xaxis", BlankAxis());
        layout.SetValue("yaxis", BlankAxis());
        layout.SetValue("showlegend", false);
        return layout;
    }

    /// <summary>
    /// Converts a sequence of CA state rows into a normalised float matrix for heatmap rendering.
    /// Each state value is divided by (statesCount - 1) to map to [0, 1].
    /// Rows are reversed so that the first step appears at the top of the image.
    /// </summary>
    public static float[][] ToHeatmapMatrix(IEnumerable<State[]> steps, int statesCount)
    {
        float divisor = Math.Max(1, statesCount - 1);
        var rows = steps.Select(s => s.Select(st => (float)st.Value / divisor).ToArray()).ToList();
        rows.Reverse();
        return rows.ToArray();
    }

    /// <summary>
    /// Saves a CA step sequence as a PNG heatmap at the given path (no extension needed).
    /// </summary>
    public static void SaveHeatmap(
        IEnumerable<State[]> steps,
        int statesCount,
        string outputPath,
        int width = 1600,
        int height = 1600)
    {
        var matrix = ToHeatmapMatrix(steps, statesCount);
        SaveHeatmap(matrix, outputPath, width, height);
    }

    /// <summary>
    /// Saves a pre-computed float matrix as a PNG heatmap at the given path (no extension needed).
    /// </summary>
    public static void SaveHeatmap(
        float[][] matrix,
        string outputPath,
        int width = 1600,
        int height = 1600)
    {
        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        Plotly.NET.CSharp.Chart.Heatmap<float, int, int, string>(
                zData: matrix,
                ShowLegend: false,
                ShowScale: false)
            .WithLayout(BuildCleanLayout())
            .SavePNG(outputPath, Width: width, Height: height);
    }
}
