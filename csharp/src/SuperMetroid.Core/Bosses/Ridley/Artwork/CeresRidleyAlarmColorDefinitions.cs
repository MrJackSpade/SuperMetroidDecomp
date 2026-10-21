using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Native $A6:C1DF-C23E EMERGENCY colors, uniformly copied to CGRAM97..99 by
/// $A6:C19C-C1DE. The reflected gold-to-muted-red trajectory calculates every
/// sample. The five initial paint magnitudes and these specific channel rates are
/// selected text-color composition: replacing them invents different paint.
/// The middle shade is copied target data, absent from the nine $A6:C164 glyphs.
/// The separate NMI cadence and glyph pixels are not defined here.
/// </summary>
internal sealed class CeresRidleyAlarmColorDefinitions
{
    /// <summary>Initial green component of the bright gold warning text.</summary>
    private const int BrightGreen = 22;
    /// <summary>Fixed red component of the middle alarm shade.</summary>
    private const int MiddleRed = 23;
    /// <summary>Initial green component of the middle alarm shade.</summary>
    private const int MiddleGreen = 14;
    /// <summary>Initial red component of the dark warning shade.</summary>
    private const int DarkRed = 12;
    /// <summary>Initial green component of the dark warning shade.</summary>
    private const int DarkGreen = 5;
    /// <summary>Independent supplied colors that differ from the reflected sixteen-row trajectory.</summary>
    internal readonly Dictionary<int, Bgr555> edits = [];

    /// <summary>Compares supplied warning colors with calculated rows and retains only independently edited cells.</summary>
    internal CeresRidleyAlarmColorDefinitions(Bgr555[][] rows)
    {
        for (int row = 0; row < CeresRidleyPaletteRomData.AlarmRowCount; row++)
        for (int color = 0; color < CeresRidleyPaletteRomData.AlarmColorCount; color++)
            if (Calculate(row, color) != rows[row][color])
                edits.Add(row * CeresRidleyPaletteRomData.AlarmColorCount + color, rows[row][color]);
    }

    /// <summary>$A6:C1DF-C23E: sixteen rows ascend through row8, then revisit rows7..1.</summary>
    internal static int SourceRow(int row)
    {
        if ((uint)row >= CeresRidleyPaletteRomData.AlarmRowCount)
            throw new ArgumentOutOfRangeException(nameof(row));
        return Math.Min(row, CeresRidleyPaletteRomData.AlarmRowCount - row);
    }

    /// <summary>Uses an edited cell when supplied; otherwise computes its bright, middle, or dark warning shade.</summary>
    internal Bgr555 Resolve(int row, int color)
    {
        _ = SourceRow(row);
        if ((uint)color >= CeresRidleyPaletteRomData.AlarmColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        int key = row * CeresRidleyPaletteRomData.AlarmColorCount + color;
        return edits.TryGetValue(key, out Bgr555 value) ? value : Calculate(row, color);
    }

    /// <summary>Builds the three copied target colors from the rising and reflected phase's channel rates.</summary>
    private static Bgr555 Calculate(int row, int color)
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
