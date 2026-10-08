namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Typed palette-only OBJ attribute fragments. Enemy and cinematic state stores these
/// already shifted hardware fields, so callers can retain the cartridge representation
/// without spelling raw values such as <c>$0E00</c> throughout functional code.
/// </summary>
public static class SnesObjPalettes
{
    /// <summary>Palette-select bits for OBJ palette 1, with tile number and priority bits cleared.</summary>
    public static readonly SnesObjAttributeWord Index1 =
        SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: 1, priority: 0);

    /// <summary>Palette-select bits for OBJ palette 2, with tile number and priority bits cleared.</summary>
    public static readonly SnesObjAttributeWord Index2 =
        SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: 2, priority: 0);

    /// <summary>Palette-select bits for OBJ palette 3, with tile number and priority bits cleared.</summary>
    public static readonly SnesObjAttributeWord Index3 =
        SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: 3, priority: 0);

    /// <summary>Palette-select bits for OBJ palette 4, with tile number and priority bits cleared.</summary>
    public static readonly SnesObjAttributeWord Index4 =
        SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: 4, priority: 0);

    /// <summary>Palette-select bits for OBJ palette 5, with tile number and priority bits cleared.</summary>
    public static readonly SnesObjAttributeWord Index5 =
        SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: 5, priority: 0);

    /// <summary>Palette-select bits for OBJ palette 6, with tile number and priority bits cleared.</summary>
    public static readonly SnesObjAttributeWord Index6 =
        SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: 6, priority: 0);

    /// <summary>Palette-select bits for OBJ palette 7, with tile number and priority bits cleared.</summary>
    public static readonly SnesObjAttributeWord Index7 =
        SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: 7, priority: 0);
}
