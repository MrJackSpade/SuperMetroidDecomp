using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

internal enum CeresRidleyFadeKind { Eyes, Body }

/// <summary>Exact floor-scaled subset of native $A6:E2AA..E469; endpoints and differing native samples remain explicitly required.</summary>
internal sealed class CeresRidleyFadeColorDefinitions
{
    private readonly CeresRidleyFadeKind kind;
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
        for (int row = 0; row < Rows; row++)
        for (int color = 0; color < Colors; color++)
            if (RequiresNativeBasis(row, color)) basis[row * Colors + color] = rows[row][color];
        for (int row = 0; row < Rows; row++)
        for (int color = 0; color < Colors; color++)
            if (!RequiresNativeBasis(row, color) && rows[row][color] != Calculate(row, color))
                overrides[row * Colors + color] = rows[row][color];
    }

    /// <summary>
    /// REQUIRED native basis: eye endpointE2AA..E2AE and body endpointE454..E468,
    /// plus eye samplesE2B0/E2EE/E2FC/E300 and body samplesE33A/E362/E36C/E36E/
    /// E3A8/E3BC/E3C4/E404/E410/E436/E438/E440/E444/E446/E448/E44E/E450.
    /// These21 differing samples have no retention justification; a failed interpolation
    /// relationship does not establish nonsense. They remain independently unresolved.
    /// </summary>
    internal bool RequiresNativeBasis(int row, int color)
    {
        Validate(row, color);
        if (row == EndpointRow) return true;
        int start = kind == CeresRidleyFadeKind.Eyes
            ? CeresRidleyPaletteRomData.EyeFadeColors : CeresRidleyPaletteRomData.BodyFadeColors;
        int address = (start & 0xFFFF) + (row * Colors + color) * sizeof(ushort);
        return address is 0xE2B0 or 0xE2EE or 0xE2FC or 0xE300
            or 0xE33A or 0xE362 or 0xE36C or 0xE36E or 0xE3A8 or 0xE3BC
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

    /// <summary>Calculate one exact-subset sample by separately truncating each RGB5 channel toward black over the native fifteen intervals.</summary>
    private ushort Calculate(int row, int color)
    {
        ushort endpoint = basis[EndpointRow * Colors + color];
        int phase = kind == CeresRidleyFadeKind.Eyes ? Rows - 1 - row : row;
        int red = (endpoint & 31) * phase / (Rows - 1);
        int green = ((endpoint >> 5) & 31) * phase / (Rows - 1);
        int blue = ((endpoint >> 10) & 31) * phase / (Rows - 1);
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
