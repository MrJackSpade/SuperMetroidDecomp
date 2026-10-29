using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The cartridge's packed forms of a sprite's graphics, for the native oracles and native-memory comparisons that pack
/// or read words exactly as the cartridge does. Production carries <see cref="SpriteGraphics"/> and colourings; the
/// words are encoded here once, by the import pipeline's attribute-word codec.
/// </summary>
internal static class NativeSpriteGraphicsWords
{
    /// <summary>Palette row <paramref name="row"/>, 0 through 7, as an attribute word's palette field stores it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the eight sprite palette rows.</exception>
    public static ushort PaletteBits(int row) => SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: row, priority: 0).PaletteBits;

    /// <summary>The palette row an enemy's native palette word selects: its palette field (#1435).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The word holds bits outside the palette field.</exception>
    public static int PaletteRow(ushort paletteBits) => SnesObjAttributeWordConstants.FromPaletteBits(paletteBits).PaletteIndex;

    /// <summary>
    /// <paramref name="graphics"/> as the cartridge's graphics word holds it: the tile base in the character field and
    /// <paramref name="paletteRow"/>, the row holding its colours, in the palette field, the word the native writers add
    /// to and OR into each part's attributes.
    /// </summary>
    public static ushort GraphicsWord(SpriteGraphics graphics, int paletteRow) =>
        SnesObjAttributeWord.Create(tileNumber: graphics.TileBase, paletteIndex: paletteRow, priority: 0).Raw;
}
