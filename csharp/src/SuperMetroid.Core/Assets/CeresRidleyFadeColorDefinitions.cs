using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

internal enum CeresRidleyFadeKind
{
    /// <summary>Selects the separately authored eye-channel fade layout and eye paint model.</summary>
    Eyes,
    /// <summary>Selects the body-channel fade layout and body paint model.</summary>
    Body
}

/// <summary>Native $A6:E2AA..E469 channel fades. Eye and body channels share the same biased integer quantizer.</summary>
internal sealed class CeresRidleyFadeColorDefinitions
{
    /// <summary>Chooses the eye or body fade dimensions and endpoint-color calculation.</summary>
    private readonly CeresRidleyFadeKind kind;
    /// <summary>Endpoint paint parameters used to calculate the categorical eye fade colors.</summary>
    private readonly EyePaint eyePaint;
    /// <summary>Endpoint paint parameters used to calculate body fade colors.</summary>
    private readonly CeresRidleyBodyPaintDefinitions? bodyPaint;

    /// <summary>Source eye paint at $A6:E2AA..E2AE (also Ridley palette slots 12..14 at $A6:E167..E16B). The selected warm highlight/middle/shadow paint uses saturated red, a shared red step, three green levels and no blue. Regenerating these categorical material choices without the source inputs would select different eye paint. Initial/explosion palette aliases are evidence only; their independent containers are not exempted.</summary>
    /// <param name="RedStep">Per-shade reduction from maximum RGB5 red.</param>
    /// <param name="HighlightGreen">Green component retained for the brightest eye shade.</param>
    /// <param name="MiddleGreen">Green component retained for the middle eye shade.</param>
    /// <param name="ShadowGreen">Green component retained for the darkest eye shade.</param>
    internal readonly record struct EyePaint(int RedStep, int HighlightGreen, int MiddleGreen, int ShadowGreen)
    {
        /// <summary>Largest representable component in a five-bit SNES RGB channel.</summary>
        private const int MaximumRgb5 = (1 << 5) - 1;

        /// <summary>Derives the compact eye-paint parameters from the three source RGB555 shade colors.</summary>
        /// <param name="colors">Source palette words ordered highlight, middle, then shadow.</param>
        /// <returns>Shared red falloff and the independently retained green component for each shade.</returns>
        internal static EyePaint From(ReadOnlySpan<ushort> colors) => new(MaximumRgb5 - (colors[1] & 31), colors[0] >> 5 & 31,
            colors[1] >> 5 & 31, colors[2] >> 5 & 31);

        /// <summary>Creates the RGB555 word for one of the three retained eye shades.</summary>
        /// <param name="shade">Zero for highlight, one for middle, or two for shadow.</param>
        /// <returns>Packed eye color with the calculated red and source-authored green.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The shade is not one of the three eye colors.</exception>
        internal ushort Color(int shade)
        {
            int red = Math.Clamp(MaximumRgb5 - RedStep * shade, 0, MaximumRgb5);
            int green = shade switch { 0 => HighlightGreen, 1 => MiddleGreen, 2 => ShadowGreen, _ => throw new ArgumentOutOfRangeException(nameof(shade)) };
            return (ushort)(red | green << 5);
        }
    }
    /// <summary>Source colors that differ from the shared fade calculation, indexed by row and color slot.</summary>
    private readonly Dictionary<int, ushort> overrides = new();
    /// <summary>Color slots per fade row for the selected eye or body palette.</summary>
    private int Colors => kind == CeresRidleyFadeKind.Eyes
        ? CeresRidleyPaletteRomData.EyeFadeColorCount : CeresRidleyPaletteRomData.BodyFadeColorCount;
    /// <summary>Number of fade rows in the selected eye or body palette.</summary>
    private int Rows => kind == CeresRidleyFadeKind.Eyes
        ? CeresRidleyPaletteRomData.EyeFadeRowCount : CeresRidleyPaletteRomData.BodyFadeRowCount;

    /// <summary>Captures palette edits as overrides while deriving the remaining colors from the selected fade model.</summary>
    /// <param name="kind">Whether rows use the eye or body fade layout.</param>
    /// <param name="rows">Authored RGB555 rows whose entries are compared with calculated fade values.</param>
    internal CeresRidleyFadeColorDefinitions(CeresRidleyFadeKind kind, ushort[][] rows)
    {
        this.kind = kind;
        if (kind == CeresRidleyFadeKind.Eyes) eyePaint = EyePaint.From(rows[0]);
        else bodyPaint = new(rows[Rows - 1]);
        for (int row = 0; row < Rows; row++)
        for (int color = 0; color < Colors; color++)
            if (rows[row][color] != Calculate(row, color))
                overrides[row * Colors + color] = rows[row][color];
    }

    /// <summary>Returns an authored override when present, otherwise calculates the selected fade color.</summary>
    /// <param name="row">Zero-based row in the selected fade table.</param>
    /// <param name="color">Zero-based color slot within that row.</param>
    /// <returns>The resolved RGB555 palette word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The row or color index is outside the selected table dimensions.</exception>
    internal ushort Resolve(int row, int color)
    {
        Validate(row, color);
        int key = row * Colors + color;
        return overrides.TryGetValue(key, out ushort edited) ? edited : Calculate(row, color);
    }

    /// <summary>Scale each RGB5 channel through fifteen intervals. Both select a one-unit numerator rounding bias; this is a common fade composition rule, not a claim about SNES hardware or the original authoring tool.</summary>
    private ushort Calculate(int row, int color)
    {
        ushort endpoint = kind == CeresRidleyFadeKind.Eyes ? eyePaint.Color(color) : bodyPaint!.Color(color);
        int phase = kind == CeresRidleyFadeKind.Eyes ? Rows - 1 - row : row;
        return Scale(endpoint, phase);
    }

    /// <summary>Shared fifteen-interval channel quantizer used by native eye/body, retreat and zoom colors.</summary>
    internal static ushort Scale(ushort endpoint, int phase)
    {
        const int intervals = 15;
        const int bias = 1;
        int red = ((endpoint & 31) * phase + bias) / intervals;
        int green = (((endpoint >> 5) & 31) * phase + bias) / intervals;
        int blue = (((endpoint >> 10) & 31) * phase + bias) / intervals;
        return (ushort)(red | green << 5 | blue << 10);
    }

    /// <summary>Checks that a row index is valid for the selected eye or body fade table.</summary>
    /// <param name="row">Zero-based row to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the selected fade table.</exception>
    internal void ValidateRow(int row)
    {
        if ((uint)row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));
    }

    /// <summary>Checks both coordinates before indexing the sparse override table or calculating a color.</summary>
    /// <param name="row">Zero-based fade row.</param>
    /// <param name="color">Zero-based color slot within the row.</param>
    /// <exception cref="ArgumentOutOfRangeException">Either coordinate is outside the selected fade table.</exception>
    private void Validate(int row, int color)
    {
        ValidateRow(row);
        if ((uint)color >= Colors) throw new ArgumentOutOfRangeException(nameof(color));
    }
}
