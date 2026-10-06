using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

internal enum CeresRidleyFadeKind { Eyes, Body }

/// <summary>Native $A6:E2AA..E469 channel fades. Eye channels share a biased integer quantizer; body residuals remain required.</summary>
internal sealed class CeresRidleyFadeColorDefinitions
{
    private readonly CeresRidleyFadeKind kind;
    private readonly EyePaint eyePaint;

    /// <summary>Source eye paint at $A6:E2AA..E2AE (also Ridley palette slots 12..14 at $A6:E167..E16B). The selected warm highlight/middle/shadow paint uses saturated red, a shared red step, three green levels and no blue. Regenerating these categorical material choices without the source inputs would select different eye paint. Initial/explosion palette aliases are evidence only; their independent containers are not exempted.</summary>
    private readonly record struct EyePaint(int RedStep, int HighlightGreen, int MiddleGreen, int ShadowGreen)
    {
        private const int MaximumRgb5 = (1 << 5) - 1;

        internal static EyePaint From(ushort[] colors) => new(MaximumRgb5 - (colors[1] & 31), colors[0] >> 5 & 31,
            colors[1] >> 5 & 31, colors[2] >> 5 & 31);

        internal ushort Color(int shade)
        {
            int red = Math.Clamp(MaximumRgb5 - RedStep * shade, 0, MaximumRgb5);
            int green = shade switch { 0 => HighlightGreen, 1 => MiddleGreen, 2 => ShadowGreen, _ => throw new ArgumentOutOfRangeException(nameof(shade)) };
            return (ushort)(red | green << 5);
        }
    }
    private readonly Dictionary<int, ushort> basis = new();
    private readonly Dictionary<int, ushort> overrides = new();
    private int Colors => kind == CeresRidleyFadeKind.Eyes
        ? CeresRidleyPaletteRomData.EyeFadeColorCount : CeresRidleyPaletteRomData.BodyFadeColorCount;
    private int Rows => kind == CeresRidleyFadeKind.Eyes
        ? CeresRidleyPaletteRomData.EyeFadeRowCount : CeresRidleyPaletteRomData.BodyFadeRowCount;
    private int EndpointRow => kind == CeresRidleyFadeKind.Eyes ? 0 : Rows - 1;

    internal CeresRidleyFadeColorDefinitions(CeresRidleyFadeKind kind, ushort[][] rows)
    {
        this.kind = kind;
        if (kind == CeresRidleyFadeKind.Eyes) eyePaint = EyePaint.From(rows[0]);
        for (int row = 0; row < Rows; row++)
        for (int color = 0; color < Colors; color++)
            if (RequiresNativeBasis(row, color)) basis[row * Colors + color] = rows[row][color];
        for (int row = 0; row < Rows; row++)
        for (int color = 0; color < Colors; color++)
            if (!RequiresNativeBasis(row, color) && rows[row][color] != Calculate(row, color))
                overrides[row * Colors + color] = rows[row][color];
    }

    /// <summary>
    /// REQUIRED body basis: endpointE454..E468,
    /// plus body samplesE33A/E362/E36C/E36E/
    /// E3A8/E3BC/E3C4/E404/E410/E436/E438/E440/E444/E446/E448/E44E/E450.
    /// These17 differing body samples have no retention justification; a failed interpolation
    /// relationship does not establish nonsense. They remain independently unresolved.
    /// </summary>
    internal bool RequiresNativeBasis(int row, int color)
    {
        Validate(row, color);
        if (kind == CeresRidleyFadeKind.Eyes) return false;
        if (row == EndpointRow) return true;
        int start = kind == CeresRidleyFadeKind.Eyes
            ? CeresRidleyPaletteRomData.EyeFadeColors : CeresRidleyPaletteRomData.BodyFadeColors;
        int address = (start & 0xFFFF) + (row * Colors + color) * sizeof(ushort);
        return address is 0xE33A or 0xE362 or 0xE36C or 0xE36E or 0xE3A8 or 0xE3BC
            or 0xE3C4 or 0xE404 or 0xE410 or 0xE436 or 0xE438 or 0xE440
            or 0xE444 or 0xE446 or 0xE448 or 0xE44E or 0xE450;
    }

    internal ushort Resolve(int row, int color)
    {
        Validate(row, color);
        int key = row * Colors + color;
        if (basis.TryGetValue(key, out ushort required)) return required;
        return overrides.TryGetValue(key, out ushort edited) ? edited : Calculate(row, color);
    }

    /// <summary>Scale each RGB5 channel through fifteen intervals. Eyes select a one-unit numerator rounding bias; this is a common fade composition rule, not a claim about SNES hardware or the original authoring tool.</summary>
    private ushort Calculate(int row, int color)
    {
        ushort endpoint = kind == CeresRidleyFadeKind.Eyes ? eyePaint.Color(color) : basis[EndpointRow * Colors + color];
        int phase = kind == CeresRidleyFadeKind.Eyes ? Rows - 1 - row : row;
        int bias = kind == CeresRidleyFadeKind.Eyes ? 1 : 0;
        int red = ((endpoint & 31) * phase + bias) / (Rows - 1);
        int green = (((endpoint >> 5) & 31) * phase + bias) / (Rows - 1);
        int blue = (((endpoint >> 10) & 31) * phase + bias) / (Rows - 1);
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
