using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Packed OBJ palette fragments stored in enemy <c>PaletteIndex</c>/<c>GraphicsIndex</c>
/// words. The names intentionally follow the hardware palette number because individual
/// enemy families reuse the same field for unrelated art sets.
/// </summary>
public static class EnemyPaletteBits
{
    /// <summary>OBJ palette one encoded as $0200 in attribute bits 9..11, selecting CGRAM colors 144..159; tile, priority, and flip fields are clear, and no colors are loaded.</summary>
    public static ushort Palette1 => SnesObjPalettes.Index1.PaletteBits;
    /// <summary>OBJ palette two encoded as $0400 in attribute bits 9..11, selecting CGRAM colors 160..175; this is a packed enemy palette fragment, not the unshifted selector two.</summary>
    public static ushort Palette2 => SnesObjPalettes.Index2.PaletteBits;
    /// <summary>OBJ palette five encoded as $0A00 in attribute bits 9..11, selecting CGRAM colors 208..223; enemy and projectile families independently own the artwork placed in those slots.</summary>
    public static ushort Palette5 => SnesObjPalettes.Index5.PaletteBits;
    /// <summary>OBJ palette seven encoded as $0E00 in attribute bits 9..11, selecting CGRAM colors 240..255; the fragment supplies no tile-page, priority, or reflection bits and does not imply one particular enemy palette image.</summary>
    public static ushort Palette7 => SnesObjPalettes.Index7.PaletteBits;
}
