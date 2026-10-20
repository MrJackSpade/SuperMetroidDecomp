namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Typed palette-only OBJ attribute fragments. Enemy and cinematic state stores these
/// already shifted hardware fields, so callers can retain the cartridge representation
/// without spelling raw values such as <c>$0E00</c> throughout functional code.
/// </summary>
public static class SnesObjPalettes
{
    /// <summary>The palette-select bits of OBJ palettes 0 through 7, indexed by palette, with tile number and priority bits cleared.</summary>
    private static readonly SnesObjAttributeWord[] PaletteBits =
        [.. Enumerable.Range(0, 8).Select(palette => SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: palette, priority: 0))];

    /// <summary>The palette-select bits of OBJ <paramref name="palette"/>, with tile number and priority bits cleared.</summary>
    /// <param name="palette">The OBJ palette, 0 through 7.</param>
    public static SnesObjAttributeWord Palette(int palette) => PaletteBits[palette];
}
