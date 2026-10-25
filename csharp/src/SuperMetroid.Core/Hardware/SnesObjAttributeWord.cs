namespace SuperMetroid.Core.Hardware;

/// <summary>A lossless view over the two-byte attribute word in one low-OAM record.</summary>
/// <param name="Raw">The packed tile number and OBJ attributes, preserving all 16 native bits.</param>
[EncodingOwner("sprite attribute word")]
public readonly record struct SnesObjAttributeWord(ushort Raw)
{
    /// <summary>Bits 0 through 8 select the OBJ character, including its tile page.</summary>
    private const ushort TileNumberMask = 0x01ff;
    /// <summary>The OBJ palette field follows the nine-bit tile number.</summary>
    private const int PaletteShift = 9;
    /// <summary>The OBJ priority field begins at bit 12.</summary>
    private const int PriorityShift = 12;
    /// <summary>OBJ palette indices and attribute priority fragments use three bits.</summary>
    private const ushort ThreeBitMask = 0x0007;
    /// <summary>The PPU exposes four OBJ priority tiers.</summary>
    private const ushort TwoBitMask = 0x0003;
    /// <summary>The upper two attribute bits reverse the tile on either axis.</summary>
    private const ushort FlipMask = 0xc000;

    /// <summary>Gets the nine-bit OBJ character number, from 0 through 511, including the tile-page select bit.</summary>
    public int TileNumber => Raw & TileNumberMask;
    /// <summary>Gets the OBJ palette selector, from 0 through 7, decoded from bits 9 through 11.</summary>
    public int PaletteIndex => (Raw >> PaletteShift) & ThreeBitMask;
    /// <summary>Gets the two-bit OBJ priority tier, from 0 through 3.</summary>
    public int Priority => (Raw >> PriorityShift) & TwoBitMask;
    /// <summary>Gets only the palette field in its packed bit positions, suitable for native palette composition.</summary>
    public ushort PaletteBits => unchecked((ushort)(Raw & PaletteFieldMask));
    /// <summary>Gets both reflection flags without including tile, palette, or priority bits.</summary>
    public SnesTileFlipFlags FlipFlags => (SnesTileFlipFlags)(Raw & FlipMask);
    /// <summary>Gets whether the object's pixels are drawn in reverse horizontal order.</summary>
    public bool FlipHorizontally => (FlipFlags & SnesTileFlipFlags.Horizontal) != 0;
    /// <summary>Gets whether the object's rows are drawn in reverse vertical order.</summary>
    public bool FlipVertically => (FlipFlags & SnesTileFlipFlags.Vertical) != 0;

    /// <summary>Bits 9 through 11 are the only bits accepted by palette-fragment operations.</summary>
    private const ushort PaletteFieldMask = ThreeBitMask << PaletteShift;

    /// <summary>Encodes every field in one standard two-byte low-OAM attribute record.</summary>
    public static SnesObjAttributeWord Create(
        int tileNumber,
        int paletteIndex,
        int priority,
        SnesTileFlipFlags flips = SnesTileFlipFlags.None)
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
            (ushort)flips)));
    }

    /// <summary>Creates a word containing only a validated OBJ palette field.</summary>
    public static SnesObjAttributeWord FromPaletteBits(int paletteBits)
    {
        if ((paletteBits & ~PaletteFieldMask) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(paletteBits), paletteBits, "OBJ palette bits must fit the three-bit field.");
        }

        return new((ushort)paletteBits);
    }

    /// <summary>Replaces only the OBJ palette field, preserving tile, priority, and flips.</summary>
    public SnesObjAttributeWord WithPaletteBits(ushort paletteBits)
    {
        SnesObjAttributeWord palette = FromPaletteBits(paletteBits);
        return new(unchecked((ushort)((Raw & ~PaletteFieldMask) | palette.Raw)));
    }

    /// <summary>Replaces the three-bit palette index.</summary>
    public SnesObjAttributeWord WithPaletteIndex(int paletteIndex)
    {
        if ((uint)paletteIndex > ThreeBitMask)
            throw new ArgumentOutOfRangeException(nameof(paletteIndex));
        return WithPaletteBits(unchecked((ushort)(paletteIndex << PaletteShift)));
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

    /// <summary>Rejects unsupported reflection bits before composing an OBJ attribute word.</summary>
    private static void ValidateFlips(SnesTileFlipFlags flips)
    {
        if ((flips & ~(SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical)) != 0)
            throw new ArgumentOutOfRangeException(nameof(flips));
    }

    /// <summary>Wraps an existing low-OAM attribute word without changing its bit pattern.</summary>
    public static implicit operator SnesObjAttributeWord(ushort raw) => new(raw);
    /// <summary>Returns the packed tile and attribute bytes for native low-OAM storage.</summary>
    public static implicit operator ushort(SnesObjAttributeWord word) => word.Raw;
}
