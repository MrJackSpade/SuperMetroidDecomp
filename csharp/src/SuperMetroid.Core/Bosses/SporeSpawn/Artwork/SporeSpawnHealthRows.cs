using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// $A5:E379..E3F8, the four health rows. The healthy and critical rows are authored paint.
/// In the two intermediate rows the shell and glow colors nine to fifteen interpolate those
/// rows to nearest integer over three intervals and the transparent slot repeats the
/// critical row's; colors one to eight are painted per row. Only supplied deviations from
/// the calculation are stored, so edited endpoint rows drive every intermediate step.
/// </summary>
internal sealed class SporeSpawnHealthRows
{
    /// <summary>Shell and glow colors from ink 9 onward interpolate between healthy and critical.</summary>
    private const int FirstInterpolatedColor = 9;
    /// <summary>Inks 1 through 8 remain painted separately in each intermediate health row.</summary>
    private const int LastPaintedColor = 8;
    /// <summary>Healthy endpoint supplies the first boss row and interpolation source.</summary>
    private readonly RgbColor[] healthy;
    /// <summary>Critical endpoint supplies the last boss row and transparent ink zero.</summary>
    private readonly RgbColor[] critical;
    /// <summary>Authored inks 1 through 8 from the two middle health rows.</summary>
    private readonly RgbColor[][] intermediatePaint;
    /// <summary>Middle-row calculated inks retained only when the supplied RGB5 value differs.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> intermediateEdits = new();

    /// <summary>Splits four supplied health rows into endpoints, painted middle inks and independent interpolation edits.</summary>
    internal SporeSpawnHealthRows(RgbColor[][] supplied)
    {
        int last = SporeSpawnColorDefinitions.HealthFrameCount - 1;
        healthy = supplied[0];
        critical = supplied[last];
        intermediatePaint = new RgbColor[last - 1][];
        for (int frame = 1; frame < last; frame++)
        {
            intermediatePaint[frame - 1] = supplied[frame].AsSpan(1, LastPaintedColor).ToArray();
            for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
                if (IsCalculated(color) && supplied[frame][color] != Calculate(frame, color))
                    intermediateEdits.Add(new ColourCell(frame, color), supplied[frame][color]);
        }
    }

    /// <summary>Chooses authored paint or reconstructs the requested middle-row ink from editable endpoints.</summary>
    internal RgbColor Resolve(int frame, int color)
    {
        int last = SporeSpawnColorDefinitions.HealthFrameCount - 1;
        if (frame == 0) return healthy[color];
        if (frame == last) return critical[color];
        if (!IsCalculated(color)) return intermediatePaint[frame - 1][color - 1];
        return intermediateEdits.TryGetValue(new ColourCell(frame, color), out RgbColor edit)
            ? edit : Calculate(frame, color);
    }

    /// <summary>Identifies transparent ink zero and shell/glow inks that derive from endpoint rows.</summary>
    private static bool IsCalculated(int color) => color is 0 or >= FirstInterpolatedColor;

    /// <summary>Interpolates an RGB5 shell/glow ink to nearest integer over the three health intervals; ink zero repeats critical.</summary>
    private RgbColor Calculate(int frame, int color)
    {
        if (color == 0) return critical[0];
        int intervals = SporeSpawnColorDefinitions.HealthFrameCount - 1;
        return healthy[color].Zip(critical[color], (_, start, end) =>
            (start * (intervals - frame) + end * frame + intervals / 2) / intervals);
    }
}
