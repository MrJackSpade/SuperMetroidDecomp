namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Independent horizontal/vertical flip bits shared by SNES BG tilemap and OBJ attribute
/// words. These are genuine flags: the PPU supports neither, either, or both simultaneously.
/// </summary>
[Flags]
public enum SnesTileFlipFlags : ushort
{
    /// <summary>Leaves both axes in their stored pixel order.</summary>
    None = 0,
    /// <summary>Sets bit 14 to reverse the tile's horizontal pixel order.</summary>
    Horizontal = 0x4000,
    /// <summary>Sets bit 15 to reverse the tile's vertical pixel order.</summary>
    Vertical = 0x8000,
}

/// <summary>A lossless view over one standard SNES background tilemap entry.</summary>
/// <param name="Raw">The complete 16-bit entry, including character, palette, priority, and flip fields.</param>
public readonly record struct SnesBgTilemapWord(ushort Raw)
{
    private const ushort CharacterMask = 0x03ff;
    private const int PaletteShift = 10;
    private const ushort PaletteMask = 0x0007;
    private const ushort PriorityMask = 0x2000;
    private const ushort FlipMask = 0xc000;

    /// <summary>Gets the ten-bit character number, from 0 through 1023, relative to the background's character base.</summary>
    public int CharacterIndex => Raw & CharacterMask;
    /// <summary>Gets the three-bit background palette selector, from 0 through 7.</summary>
    public int PaletteIndex => (Raw >> PaletteShift) & PaletteMask;
    /// <summary>Gets whether bit 13 selects this background tile's higher priority tier.</summary>
    public bool HasPriority => (Raw & PriorityMask) != 0;
    /// <summary>Gets the horizontal and vertical reflection bits in their original packed positions.</summary>
    public SnesTileFlipFlags FlipFlags => (SnesTileFlipFlags)(Raw & FlipMask);
    /// <summary>Gets whether pixels are reflected across the tile's horizontal axis of traversal.</summary>
    public bool FlipHorizontally => (FlipFlags & SnesTileFlipFlags.Horizontal) != 0;
    /// <summary>Gets whether rows are drawn in reverse vertical order.</summary>
    public bool FlipVertically => (FlipFlags & SnesTileFlipFlags.Vertical) != 0;

    /// <summary>
    /// Toggles only the requested PPU flip bits, exactly matching the XOR performed while
    /// expanding a flipped 16×16 room block into four 8×8 entries.
    /// </summary>
    public SnesBgTilemapWord ToggleFlips(SnesTileFlipFlags flags)
    {
        ValidateFlips(flags);
        return new(unchecked((ushort)(Raw ^ (ushort)flags)));
    }

    /// <summary>Encodes every documented SNES BG tilemap field into one lossless word.</summary>
    public static SnesBgTilemapWord Create(
        int characterIndex,
        int paletteIndex,
        bool priority,
        SnesTileFlipFlags flips = SnesTileFlipFlags.None)
    {
        if ((uint)characterIndex > CharacterMask)
            throw new ArgumentOutOfRangeException(nameof(characterIndex));
        if ((uint)paletteIndex > PaletteMask)
            throw new ArgumentOutOfRangeException(nameof(paletteIndex));
        ValidateFlips(flips);

        return new(unchecked((ushort)(
            characterIndex |
            (paletteIndex << PaletteShift) |
            (priority ? PriorityMask : 0) |
            (ushort)flips)));
    }

    /// <summary>Replaces only the ten-bit BG character index.</summary>
    public SnesBgTilemapWord WithCharacterIndex(int characterIndex)
    {
        if ((uint)characterIndex > CharacterMask)
            throw new ArgumentOutOfRangeException(nameof(characterIndex));
        return new(unchecked((ushort)((Raw & ~CharacterMask) | characterIndex)));
    }

    /// <summary>
    /// Replaces only the PPU's three-bit palette field while preserving character,
    /// priority, and flip fields in the packed word.
    /// </summary>
    public SnesBgTilemapWord WithPaletteIndex(int paletteIndex)
    {
        if ((uint)paletteIndex > PaletteMask)
            throw new ArgumentOutOfRangeException(nameof(paletteIndex));

        ushort paletteBits = unchecked((ushort)(paletteIndex << PaletteShift));
        ushort fieldMask = unchecked((ushort)(PaletteMask << PaletteShift));
        return new SnesBgTilemapWord(unchecked((ushort)((Raw & ~fieldMask) | paletteBits)));
    }

    private static void ValidateFlips(SnesTileFlipFlags flips)
    {
        if ((flips & ~(SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical)) != 0)
            throw new ArgumentOutOfRangeException(nameof(flips));
    }

    /// <summary>Wraps a packed background entry without discarding or validating any bits.</summary>
    public static implicit operator SnesBgTilemapWord(ushort raw) => new(raw);
    /// <summary>Returns the complete background entry for storage in a native tilemap.</summary>
    public static implicit operator ushort(SnesBgTilemapWord word) => word.Raw;
}

/// <summary>A lossless view over the two-byte attribute word in one low-OAM record.</summary>
/// <param name="Raw">The packed tile number and OBJ attributes, preserving all 16 native bits.</param>
public readonly record struct SnesObjAttributeWord(ushort Raw)
{
    /// <summary>Low nine attribute-word bits containing the OBJ character number.</summary>
    private const ushort TileNumberMask = 0x01ff;
    /// <summary>Bit position where the three-bit OBJ palette selector begins.</summary>
    private const int PaletteShift = 9;
    private const int PriorityShift = 12;
    private const ushort ThreeBitMask = 0x0007;
    private const ushort TwoBitMask = 0x0003;
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
    public static SnesObjAttributeWord FromPaletteBits(ushort paletteBits)
    {
        if ((paletteBits & ~PaletteFieldMask) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(paletteBits), paletteBits, "OBJ palette bits must fit the three-bit field.");
        }

        return new(paletteBits);
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

/// <summary>
/// The two-bit high-OAM pair belonging to one sprite: X coordinate bit eight followed by
/// the size-select bit. Four independently encoded pairs share each physical table byte.
/// </summary>
public readonly record struct SnesOamHighTablePair
{
    private const byte PairMask = 0x03;
    private const byte XHighMask = 0x01;
    /// <summary>Second bit in each packed high-table pair selects the large OBJ size.</summary>
    private const byte LargeObjectMask = 0x02;

    /// <summary>Gets the unshifted two-bit pair, from 0 through 3, before placement in a shared high-OAM byte.</summary>
    public byte Raw { get; }

    /// <summary>Gets bit eight of the sprite's nine-bit screen X coordinate.</summary>
    public bool XHigh => (Raw & XHighMask) != 0;
    /// <summary>Gets whether the sprite selects the larger of the PPU's configured OBJ sizes.</summary>
    public bool IsLarge => (Raw & LargeObjectMask) != 0;

    /// <summary>Encodes the sprite's X high bit and size selector as an unshifted high-OAM pair.</summary>
    public static SnesOamHighTablePair Create(bool xHigh, bool isLarge) =>
        new(unchecked((byte)((xHigh ? XHighMask : 0) | (isLarge ? LargeObjectMask : 0))));

    /// <summary>Builds the pair from a nine-bit screen X and a spritemap size selector.</summary>
    public static SnesOamHighTablePair FromSprite(ushort screenX, bool isLarge) =>
        Create((screenX & 0x0100) != 0, isLarge);

    /// <summary>Wraps an unshifted high-OAM pair, rejecting bits belonging to other sprites.</summary>
    /// <param name="raw">The two-bit value, from 0 through 3.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value has any bit set outside the low two bits.</exception>
    public SnesOamHighTablePair(byte raw) : this()
    {
        if ((raw & ~PairMask) != 0)
            throw new ArgumentOutOfRangeException(nameof(raw));
        Raw = raw;
    }
}

/// <summary>
/// Lossless view of the five-byte spritemap record's encoded X word. Its low nine bits are
/// the modular X offset and bit fifteen independently selects the large OBJ size.
/// </summary>
/// <param name="Raw">The complete native X word, including any unused upper bits.</param>
public readonly record struct SnesSpritemapXWord(ushort Raw)
{
    /// <summary>Low nine bits carrying the modular spritemap X displacement.</summary>
    private const ushort OffsetMask = 0x01ff;
    /// <summary>Independent high bit selecting the large OBJ size.</summary>
    private const ushort LargeObjectMask = 0x8000;
    /// <summary>Gets whether bit 15 selects the larger of the PPU's configured OBJ sizes.</summary>
    public bool IsLarge => (Raw & LargeObjectMask) != 0;
    /// <summary>Signed nine-bit visual displacement; unrelated upper bits never reach OAM X.</summary>
    public int SignedOffset => unchecked((short)((Raw & OffsetMask) << 7)) >> 7;
    /// <summary>Encodes a signed nine-bit displacement and size selector, clearing unused upper bits.</summary>
    /// <param name="offset">The horizontal displacement from the sprite origin, from -256 through 255 pixels.</param>
    /// <param name="large">Whether the sprite selects the larger configured OBJ size.</param>
    /// <exception cref="ArgumentOutOfRangeException">The displacement does not fit a signed nine-bit value.</exception>
    public static SnesSpritemapXWord Create(int offset, bool large)
    {
        if (offset is < -256 or > 255) throw new ArgumentOutOfRangeException(nameof(offset));
        return new((ushort)((offset & OffsetMask) | (large ? LargeObjectMask : 0)));
    }

    /// <summary>Wraps the cartridge's encoded X word without normalizing unused bits.</summary>
    public static implicit operator SnesSpritemapXWord(ushort raw) => new(raw);
}
