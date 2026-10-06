using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

internal enum CeresRidleyFadeKind { Eyes, Body }

/// <summary>Native $A6:E2AA..E469 channel fades. Eye and body channels share the same biased integer quantizer.</summary>
internal sealed class CeresRidleyFadeColorDefinitions
{
    private readonly CeresRidleyFadeKind kind;
    private readonly EyePaint eyePaint;
    private readonly CeresRidleyBodyPaintDefinitions? bodyPaint;

    /// <summary>Source eye paint at $A6:E2AA..E2AE (also Ridley palette slots 12..14 at $A6:E167..E16B). The selected warm highlight/middle/shadow paint uses saturated red, a shared red step, three green levels and no blue. Regenerating these categorical material choices without the source inputs would select different eye paint. Initial/explosion palette aliases are evidence only; their independent containers are not exempted.</summary>
    internal readonly record struct EyePaint(int RedStep, int HighlightGreen, int MiddleGreen, int ShadowGreen)
    {
        private const int MaximumRgb5 = (1 << 5) - 1;

        internal static EyePaint From(ReadOnlySpan<ushort> colors) => new(MaximumRgb5 - (colors[1] & 31), colors[0] >> 5 & 31,
            colors[1] >> 5 & 31, colors[2] >> 5 & 31);

        internal ushort Color(int shade)
        {
            int red = Math.Clamp(MaximumRgb5 - RedStep * shade, 0, MaximumRgb5);
            int green = shade switch { 0 => HighlightGreen, 1 => MiddleGreen, 2 => ShadowGreen, _ => throw new ArgumentOutOfRangeException(nameof(shade)) };
            return (ushort)(red | green << 5);
        }
    }
    private readonly Dictionary<int, ushort> overrides = new();
    private int Colors => kind == CeresRidleyFadeKind.Eyes
        ? CeresRidleyPaletteRomData.EyeFadeColorCount : CeresRidleyPaletteRomData.BodyFadeColorCount;
    private int Rows => kind == CeresRidleyFadeKind.Eyes
        ? CeresRidleyPaletteRomData.EyeFadeRowCount : CeresRidleyPaletteRomData.BodyFadeRowCount;

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

    internal void ValidateRow(int row)
    {
        if ((uint)row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));
    }

    private void Validate(int row, int color)
    {
        ValidateRow(row);
        if ((uint)color >= Colors) throw new ArgumentOutOfRangeException(nameof(color));
    }
}
