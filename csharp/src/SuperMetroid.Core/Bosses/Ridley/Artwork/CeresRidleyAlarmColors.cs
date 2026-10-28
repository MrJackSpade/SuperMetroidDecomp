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
    /// <summary>Independent supplied colors that differ from the reflected sixteen-row trajectory.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>Compares supplied warning colors with calculated rows and retains only independently edited cells.</summary>
    internal CeresRidleyAlarmColors(RgbColor[][] rows)
    {
        for (int row = 0; row < CeresRidleyPaletteDefinitions.AlarmRowCount; row++)
        for (int color = 0; color < CeresRidleyPaletteDefinitions.AlarmColorCount; color++)
            if (Calculate(row, color) != rows[row][color])
                edits.Add(new ColourCell(row, color), rows[row][color]);
    }

    /// <summary>$A6:C1DF-C23E: sixteen rows ascend through row8, then revisit rows7..1.</summary>
    internal static int SourceRow(int row)
    {
        if ((uint)row >= CeresRidleyPaletteDefinitions.AlarmRowCount)
            throw new ArgumentOutOfRangeException(nameof(row));
        return Math.Min(row, CeresRidleyPaletteDefinitions.AlarmRowCount - row);
    }

    /// <summary>Uses an edited cell when supplied; otherwise computes its bright, middle, or dark warning shade.</summary>
    internal RgbColor Resolve(int row, int color)
    {
        _ = SourceRow(row);
        if ((uint)color >= CeresRidleyPaletteDefinitions.AlarmColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        var key = new ColourCell(row, color);
        return edits.TryGetValue(key, out RgbColor value) ? value : Calculate(row, color);
    }

    /// <summary>Builds the three copied target colors from the rising and reflected phase's channel rates.</summary>
    private static RgbColor Calculate(int row, int color)
    {
        int phase = SourceRow(row), halfStep = (phase + 1) / 2;
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
