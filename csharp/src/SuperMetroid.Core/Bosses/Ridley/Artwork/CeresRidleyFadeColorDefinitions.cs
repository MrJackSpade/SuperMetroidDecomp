using SuperMetroid.Core.Assets;


namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Native $A6:E2AA..E469 channel fades. Eye and body channels share the same biased integer quantizer.</summary>
internal sealed class CeresRidleyFadeColorDefinitions
{
    /// <summary>Chooses eye or body row counts and their opposing fade phase directions.</summary>
    private readonly CeresRidleyFadeKind kind;
    /// <summary>Independent warm eye endpoint colors used when the eye fade is selected.</summary>
    private readonly EyePaint eyePaint;
    /// <summary>Selected body endpoint paints used only for body fade rows.</summary>
    private readonly CeresRidleyBodyPaintDefinitions? bodyPaint;

    /// <summary>Source eye paint at $A6:E2AA..E2AE (also Ridley palette slots 12..14 at $A6:E167..E16B). The selected warm highlight/middle/shadow paint uses saturated red, a shared red step, three green levels and no blue. Regenerating these categorical material choices without the source inputs would select different eye paint. Initial/explosion palette aliases are evidence only; their independent containers are not exempted.</summary>
    internal readonly record struct EyePaint(int RedStep, int HighlightGreen, int MiddleGreen, int ShadowGreen)
    {
        /// <summary>Highest SNES five-bit color channel value used to derive the eye reds.</summary>
        private const int MaximumRgb5 = (1 << 5) - 1;

        /// <summary>Extracts the selected red step and three green levels from the eye endpoint row.</summary>
        internal static EyePaint From(ReadOnlySpan<Bgr555> colors) => new(MaximumRgb5 - colors[1].Red, colors[0].Green,
            colors[1].Green, colors[2].Green);

        /// <summary>Calculates one of the three warm eye shades from the selected material inputs.</summary>
        internal Bgr555 Color(int shade)
        {
            int red = Math.Clamp(MaximumRgb5 - RedStep * shade, 0, MaximumRgb5);
            int green = shade switch { 0 => HighlightGreen, 1 => MiddleGreen, 2 => ShadowGreen, _ => throw new ArgumentOutOfRangeException(nameof(shade)) };
            return new(red, green, 0);
        }
    }
    /// <summary>Imported colors that differ from the common quantized fade formula.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> overrides = new();
    /// <summary>Number of colors in a row of the selected fade table.</summary>
    private int Colors => kind == CeresRidleyFadeKind.Eyes
        ? CeresRidleyPaletteDefinitions.EyeFadeColorCount : CeresRidleyPaletteDefinitions.BodyFadeColorCount;
    /// <summary>Number of native fade steps for the selected table.</summary>
    private int Rows => kind == CeresRidleyFadeKind.Eyes
        ? CeresRidleyPaletteDefinitions.EyeFadeRowCount : CeresRidleyPaletteDefinitions.BodyFadeRowCount;

    /// <summary>Retains selected endpoint paints and exceptions to the shared fade formula.</summary>
    internal CeresRidleyFadeColorDefinitions(CeresRidleyFadeKind kind, Bgr555[][] rows)
    {
        this.kind = kind;
        if (kind == CeresRidleyFadeKind.Eyes) eyePaint = EyePaint.From(rows[0]);
        else bodyPaint = new(rows[Rows - 1]);
        for (int row = 0; row < Rows; row++)
        for (int color = 0; color < Colors; color++)
            if (rows[row][color] != Calculate(row, color))
                overrides[new ColourCell(row, color)] = rows[row][color];
    }

    /// <summary>Returns a supplied exception color when present, otherwise the calculated fade shade.</summary>
    internal Bgr555 Resolve(int row, int color)
    {
        Validate(row, color);
        var key = new ColourCell(row, color);
        return overrides.TryGetValue(key, out Bgr555 edited) ? edited : Calculate(row, color);
    }

    /// <summary>Scale each RGB5 channel through fifteen intervals. Both select a one-unit numerator rounding bias; this is a common fade composition rule, not a claim about SNES hardware or the original authoring tool.</summary>
    private Bgr555 Calculate(int row, int color)
    {
        Bgr555 endpoint = kind == CeresRidleyFadeKind.Eyes ? eyePaint.Color(color)
            : (bodyPaint ?? throw new InvalidOperationException("A body fade requires its body paint.")).Color(color);
        int phase = kind == CeresRidleyFadeKind.Eyes ? Rows - 1 - row : row;
        return Scale(endpoint, phase);
    }

    /// <summary>Shared fifteen-interval channel quantizer used by native eye/body, retreat and zoom colors.</summary>
    internal static Bgr555 Scale(Bgr555 endpoint, int phase)
    {
        const int intervals = 15;
        const int bias = 1;
        return endpoint.Map((_, channel) => (channel * phase + bias) / intervals);
    }

    /// <summary>Rejects fade rows beyond the selected eye or body table.</summary>
    internal void ValidateRow(int row)
    {
        if ((uint)row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));
    }

    /// <summary>Checks both the table-specific row range and the color count within each row.</summary>
    private void Validate(int row, int color)
    {
        ValidateRow(row);
        if ((uint)color >= Colors) throw new ArgumentOutOfRangeException(nameof(color));
    }
}
