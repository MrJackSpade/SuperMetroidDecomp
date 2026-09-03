using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Packed OBJ palette fragments stored in enemy <c>PaletteIndex</c>/<c>GraphicsIndex</c>
/// words. The names intentionally follow the hardware palette number because individual
/// enemy families reuse the same field for unrelated art sets.
/// </summary>
public static class EnemyPaletteBits
{
    public static ushort Palette1 => SnesObjPalettes.Index1.PaletteBits;
    public static ushort Palette2 => SnesObjPalettes.Index2.PaletteBits;
    public static ushort Palette5 => SnesObjPalettes.Index5.PaletteBits;
    public static ushort Palette7 => SnesObjPalettes.Index7.PaletteBits;
}
