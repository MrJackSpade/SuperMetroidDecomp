using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Assets;
using static SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork.CeresRidleyAlarmColorDefinitions;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>
/// The EMERGENCY colors, uniformly copied to colours 97..99 by
/// the alarm-palette update. The reflected gold-to-muted-red trajectory calculates every
/// sample. The five initial paint magnitudes and these specific channel rates are
/// selected text-color composition: replacing them invents different paint.
/// The middle shade is copied target data, absent from the nine EMERGENCY glyphs.
/// The separate frame-counter cadence and glyph pixels are not defined here.
/// </summary>
internal sealed class CeresRidleyAlarmColors
{
    /// <summary>Independent supplied colors that differ from the reflected sixteen-step trajectory.</summary>
    internal readonly Dictionary<PaintColour, RgbColor> edits;

    /// <summary>Keeps the independently edited warning cells.</summary>
    /// <param name="edits">The supplied colors the import found differing from the calculated steps.</param>
    internal CeresRidleyAlarmColors(Dictionary<PaintColour, RgbColor> edits) => this.edits = edits;

    /// <summary>The reflected phase of alarm step <paramref name="step"/>: sixteen steps ascend through phase eight, then revisit phases seven to one.</summary>
    internal static int Phase(int step)
    {
        if ((uint)step >= CeresRidleyColorFormat.AlarmStepCount)
            throw new ArgumentOutOfRangeException(nameof(step));
        return Math.Min(step, CeresRidleyColorFormat.AlarmStepCount - step);
    }

    /// <summary>Uses an edited cell when supplied; otherwise computes its bright, middle, or dark warning shade.</summary>
    internal RgbColor Resolve(int step, int color)
    {
        _ = Phase(step);
        if ((uint)color >= CeresRidleyColorFormat.AlarmColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        var key = new PaintColour(step, color);
        return edits.TryGetValue(key, out RgbColor value) ? value : Calculate(step, color);
    }

    /// <summary>Builds the three copied target colors from the rising and reflected phase's channel rates.</summary>
    private static RgbColor Calculate(int step, int color)
    {
        int phase = Phase(step), halfStep = (phase + 1) / 2;
        int blue = phase * 6 / 5;
        (int red, int green) = color switch
        {
            0 => new RedGreenLevels(31 - halfStep, BrightGreen - phase),
            1 => new RedGreenLevels(MiddleRed, MiddleGreen - halfStep),
            2 => new RedGreenLevels(DarkRed + phase * 4 / 5, DarkGreen + phase / 8),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
        return new FadeLevelColour(red, green, blue).ToColour();
    }
}
