using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

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
    /// <summary>Green channel's row-zero value for the bright alarm color.</summary>
    private const int BrightGreen = 22;
    /// <summary>Fixed red channel retained by the middle alarm shade across the reflected animation.</summary>
    private const int MiddleRed = 23;
    /// <summary>Green channel's row-zero value for the middle alarm shade before its half-step fade.</summary>
    private const int MiddleGreen = 14;
    /// <summary>Red channel's row-zero value for the dark alarm shade before its rising transition.</summary>
    private const int DarkRed = 12;
    /// <summary>Green channel's row-zero value for the dark alarm shade before its slow transition.</summary>
    private const int DarkGreen = 5;
    /// <summary>Only imported samples that differ from the calculated native trajectory are retained here.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Stores only palette samples whose imported RGB15 words differ from the calculated alarm animation.</summary>
    /// <param name="rows">Sixteen rows of three native alarm colors in the reflected gold-to-muted-red sequence.</param>
    internal CeresRidleyAlarmColorDefinitions(ushort[][] rows)
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

    /// <summary>Returns an imported override when present, otherwise calculates the native alarm color for the requested sample.</summary>
    /// <param name="row">Animation row in the sixteen-step forward-and-reverse sequence.</param>
    /// <param name="color">Color slot: bright, middle, or dark.</param>
    /// <returns>The RGB15 word installed for that row and color slot.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The row or color slot is outside the compiled animation.</exception>
    internal ushort Resolve(int row, int color)
    {
        _ = SourceRow(row);
        if ((uint)color >= CeresRidleyPaletteRomData.AlarmColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        int key = row * CeresRidleyPaletteRomData.AlarmColorCount + color;
        return edits.TryGetValue(key, out ushort value) ? value : Calculate(row, color);
    }

    /// <summary>Calculates the RGB15 channels from the reflected phase using the native per-shade rates.</summary>
    /// <param name="row">Animation row used to derive the forward-and-reverse phase.</param>
    /// <param name="color">Color slot whose channel trajectory is calculated.</param>
    /// <returns>The packed five-bit-per-channel RGB color word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color slot is not one of the three alarm shades.</exception>
    private static ushort Calculate(int row, int color)
    {
        int phase = SourceRow(row), halfStep = (phase + 1) / 2;
        int blue = phase * 6 / 5;
        (int red, int green) = color switch
        {
            0 => (31 - halfStep, BrightGreen - phase),
            1 => (MiddleRed, MiddleGreen - halfStep),
            2 => (DarkRed + phase * 4 / 5, DarkGreen + phase / 8),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
        return (ushort)(red | green << 5 | blue << 10);
    }
}
