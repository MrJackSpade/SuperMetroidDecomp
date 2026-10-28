using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>A lossless view over the two-byte attribute word in one low-OAM record.</summary>
/// <param name="Raw">The packed tile number and OBJ attributes, preserving all 16 native bits.</param>
[EncodingOwner("sprite attribute word")]
public readonly record struct SnesObjAttributeWord(ushort Raw)
{
    /// <summary>Bits 0 through 8 select the OBJ character, including its tile page.</summary>
    private const ushort TileNumberMask = 0x01ff;
    /// <summary>The OBJ palette field follows the nine-bit tile number.</summary>
    internal const int PaletteShift = 9;
    /// <summary>The OBJ priority field begins at bit 12.</summary>
    private const int PriorityShift = 12;
    /// <summary>OBJ palette indices and attribute priority fragments use three bits.</summary>
    internal const ushort ThreeBitMask = 0x0007;
    /// <summary>The PPU exposes four OBJ priority tiers.</summary>
    private const ushort TwoBitMask = 0x0003;
    /// <summary>Bit 14 mirrors the object left to right.</summary>
    private const ushort HorizontalFlipBit = 0x4000;
    /// <summary>Bit 15 mirrors the object top to bottom.</summary>
    private const ushort VerticalFlipBit = 0x8000;

    /// <summary>Gets the nine-bit OBJ character number, from 0 through 511, including the tile-page select bit.</summary>
    public int TileNumber => Raw & TileNumberMask;
    /// <summary>Gets the OBJ palette selector, from 0 through 7, decoded from bits 9 through 11.</summary>
    public int PaletteIndex => (Raw >> PaletteShift) & ThreeBitMask;
    /// <summary>Gets the two-bit OBJ priority tier, from 0 through 3.</summary>
    public int Priority => (Raw >> PriorityShift) & TwoBitMask;
    /// <summary>Gets only the palette field in its packed bit positions, suitable for native palette composition.</summary>
    public ushort PaletteBits => unchecked((ushort)(Raw & PaletteFieldMask));
    /// <summary>Gets both reflection flags without including tile, palette, or priority bits.</summary>
    public TileFlip FlipFlags =>
        ((Raw & HorizontalFlipBit) != 0 ? TileFlip.Horizontal : TileFlip.None) |
        ((Raw & VerticalFlipBit) != 0 ? TileFlip.Vertical : TileFlip.None);
    /// <summary>Gets whether the object's pixels are drawn in reverse horizontal order.</summary>
    public bool FlipHorizontally => (FlipFlags & TileFlip.Horizontal) != 0;
    /// <summary>Gets whether the object's rows are drawn in reverse vertical order.</summary>
    public bool FlipVertically => (FlipFlags & TileFlip.Vertical) != 0;

    /// <summary>Bits 9 through 11 are the only bits accepted by palette-fragment operations.</summary>
    internal const ushort PaletteFieldMask = ThreeBitMask << PaletteShift;

    /// <summary>Encodes every field in one standard two-byte low-OAM attribute record.</summary>
    public static SnesObjAttributeWord Create(
        int tileNumber,
        int paletteIndex,
        int priority,
        TileFlip flips = TileFlip.None)
    {
        if ((uint)tileNumber > TileNumberMask)
            throw new ArgumentOutOfRangeException(nameof(tileNumber));
        if ((uint)paletteIndex > ThreeBitMask)
            throw new ArgumentOutOfRangeException(nameof(paletteIndex));
        if ((uint)priority > TwoBitMask)
            throw new ArgumentOutOfRangeException(nameof(priority));
        ValidateFlips(flips);

        return new(unchecked((ushort)(
            tileNumber |
            (paletteIndex << PaletteShift) |
            (priority << PriorityShift) |
            FlipBits(flips))));
    }

    /// <summary>
    /// Adds a native base-tile value to the complete packed word. Enemy spritemap writers
    /// deliberately allow tile-number overflow to carry into the adjacent attribute bits.
    /// </summary>
    public SnesObjAttributeWord AddPackedTileBase(ushort baseTileNumber) =>
        new(unchecked((ushort)(Raw + baseTileNumber)));

    /// <summary>ORs another already-validated packed attribute fragment.</summary>
    public SnesObjAttributeWord Or(SnesObjAttributeWord attributes) =>
        new(unchecked((ushort)(Raw | attributes.Raw)));

    /// <summary>The attribute bits that store <paramref name="flips"/>.</summary>
    private static int FlipBits(TileFlip flips) =>
        ((flips & TileFlip.Horizontal) != 0 ? HorizontalFlipBit : 0) |
        ((flips & TileFlip.Vertical) != 0 ? VerticalFlipBit : 0);

    /// <summary>Rejects unsupported reflection bits before composing an OBJ attribute word.</summary>
    private static void ValidateFlips(TileFlip flips)
    {
        if ((flips & ~(TileFlip.Horizontal | TileFlip.Vertical)) != 0)
            throw new ArgumentOutOfRangeException(nameof(flips));
    }

    /// <summary>The word that stores <paramref name="part"/>'s character, palette row, priority and mirroring.</summary>
    public static SnesObjAttributeWord From(SpritePart part) => Create(part.Character, part.Palette, part.Priority, part.Flip);

    /// <summary>The single small piece at the origin this word describes.</summary>
    public SpritePart ToSinglePart() => SpritePart.Single(TileNumber, PaletteIndex, Priority, FlipFlags);

    /// <summary>Wraps an existing low-OAM attribute word without changing its bit pattern.</summary>
    public static implicit operator SnesObjAttributeWord(ushort raw) => new(raw);
}
