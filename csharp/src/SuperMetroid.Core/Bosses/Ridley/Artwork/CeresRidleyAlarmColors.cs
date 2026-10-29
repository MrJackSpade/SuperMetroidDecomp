using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Bosses.Ridley.Artwork.CeresRidleyAlarmColorDefinitions;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Native $A6:C1DF-C23E EMERGENCY colors, uniformly copied to CGRAM97..99 by
/// $A6:C19C-C1DE. The reflected gold-to-muted-red trajectory calculates every
/// sample. The five initial paint magnitudes and these specific channel rates are
/// selected text-color composition: replacing them invents different paint.
/// The middle shade is copied target data, absent from the nine $A6:C164 glyphs.
/// The separate NMI cadence and glyph pixels are not defined here.
/// </summary>
internal sealed class CeresRidleyAlarmColors
{
    /// <summary>Independent supplied colors that differ from the reflected sixteen-step trajectory.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>Compares supplied warning colors with calculated steps and retains only independently edited cells.</summary>
    internal CeresRidleyAlarmColors(RgbColor[][] steps)
    {
        for (int step = 0; step < CeresRidleyPaletteDefinitions.AlarmStepCount; step++)
        for (int color = 0; color < CeresRidleyPaletteDefinitions.AlarmColorCount; color++)
            if (Calculate(step, color) != steps[step][color])
                edits.Add(new ColourCell(step, color), steps[step][color]);
    }

    /// <summary>$A6:C1DF-C23E: the reflected phase of alarm step <paramref name="step"/>: sixteen steps ascend through phase eight, then revisit phases seven to one.</summary>
    internal static int Phase(int step)
    {
        if ((uint)step >= CeresRidleyPaletteDefinitions.AlarmStepCount)
            throw new ArgumentOutOfRangeException(nameof(step));
        return Math.Min(step, CeresRidleyPaletteDefinitions.AlarmStepCount - step);
    }

    /// <summary>Uses an edited cell when supplied; otherwise computes its bright, middle, or dark warning shade.</summary>
    internal RgbColor Resolve(int step, int color)
    {
        _ = Phase(step);
        if ((uint)color >= CeresRidleyPaletteDefinitions.AlarmColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        var key = new ColourCell(step, color);
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
        return new(red, green, blue);
    }
}
