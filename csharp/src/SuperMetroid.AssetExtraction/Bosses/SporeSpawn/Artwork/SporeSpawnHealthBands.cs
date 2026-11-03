using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.SporeSpawn;

namespace SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The four health bands. The healthy and critical bands are authored paint.
/// In the two intermediate bands the shell and glow colors nine to fifteen interpolate those
/// bands to nearest integer over three intervals and the transparent slot repeats the
/// critical band's; colors one to eight are painted per band. Only supplied deviations from
/// the calculation are stored, so edited endpoint bands drive every intermediate band.
/// </summary>
internal sealed class SporeSpawnHealthBands
{
    /// <summary>Shell and glow colors from ink 9 onward interpolate between healthy and critical.</summary>
    private const int FirstInterpolatedColor = 9;
    /// <summary>Healthy endpoint supplies the first health band and interpolation source.</summary>
    private readonly RgbColor[] healthy;
    /// <summary>Critical endpoint supplies the last health band and transparent ink zero.</summary>
    private readonly RgbColor[] critical;
    /// <summary>Authored inks 1 through 8 from the two middle health bands.</summary>
    private readonly RgbColor[][] intermediatePaint;
    /// <summary>Middle-band calculated inks retained only when the supplied RGB5 value differs.</summary>
    internal readonly Dictionary<PaintColour, RgbColor> intermediateEdits;

    /// <summary>Keeps the endpoint bands, the painted middle inks and the independent interpolation edits.</summary>
    /// <param name="healthy">The healthy band, the first band and the interpolation source.</param>
    /// <param name="critical">The critical band, the last band and the transparent ink's source.</param>
    /// <param name="intermediatePaint">Inks 1 through 8 of each middle band.</param>
    /// <param name="intermediateEdits">Middle-band calculated inks whose supplied colour differs from the interpolation.</param>
    internal SporeSpawnHealthBands(RgbColor[] healthy, RgbColor[] critical, RgbColor[][] intermediatePaint,
        Dictionary<PaintColour, RgbColor> intermediateEdits)
    {
        this.healthy = healthy;
        this.critical = critical;
        this.intermediatePaint = intermediatePaint;
        this.intermediateEdits = intermediateEdits;
    }

    /// <summary>Chooses authored paint or reconstructs the requested middle-band ink from editable endpoints.</summary>
    internal RgbColor Resolve(int frame, int color)
    {
        int last = SporeSpawnColorDefinitions.HealthFrameCount - 1;
        if (frame == 0) return healthy[color];
        if (frame == last) return critical[color];
        if (!IsCalculated(color)) return intermediatePaint[frame - 1][color - 1];
        return intermediateEdits.TryGetValue(new PaintColour(frame, color), out RgbColor edit)
            ? edit : Calculate(frame, color);
    }

    /// <summary>Identifies transparent ink zero and shell/glow inks that derive from the endpoint bands.</summary>
    internal static bool IsCalculated(int color) => color is 0 or >= FirstInterpolatedColor;

    /// <summary>Interpolates an RGB5 shell/glow ink to nearest integer over the three health intervals; ink zero repeats critical.</summary>
    private RgbColor Calculate(int frame, int color)
    {
        if (color == 0) return critical[0];
        int intervals = SporeSpawnColorDefinitions.HealthFrameCount - 1;
        return healthy[color].Zip(critical[color], (_, start, end) =>
            (start * (intervals - frame) + end * frame + intervals / 2) / intervals);
    }
}
