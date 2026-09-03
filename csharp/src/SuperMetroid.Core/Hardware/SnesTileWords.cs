namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Independent horizontal/vertical flip bits shared by SNES BG tilemap and OBJ attribute
/// words. These are genuine flags: the PPU supports neither, either, or both simultaneously.
/// </summary>
[Flags]
public enum SnesTileFlipFlags : ushort
{
    None = 0,
    Horizontal = 0x4000,
    Vertical = 0x8000,
}

/// <summary>A lossless view over one standard SNES background tilemap entry.</summary>
public readonly record struct SnesBgTilemapWord(ushort Raw)
{
    private const ushort CharacterMask = 0x03ff;
    private const int PaletteShift = 10;
    private const ushort PaletteMask = 0x0007;
    private const ushort PriorityMask = 0x2000;
    private const ushort FlipMask = 0xc000;

    public int CharacterIndex => Raw & CharacterMask;
    public int PaletteIndex => (Raw >> PaletteShift) & PaletteMask;
    public bool HasPriority => (Raw & PriorityMask) != 0;
    public SnesTileFlipFlags FlipFlags => (SnesTileFlipFlags)(Raw & FlipMask);
    public bool FlipHorizontally => (FlipFlags & SnesTileFlipFlags.Horizontal) != 0;
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

    /// <summary>Replaces only the priority bit.</summary>
    public SnesBgTilemapWord WithPriority(bool priority) =>
        new(unchecked((ushort)(priority ? Raw | PriorityMask : Raw & ~PriorityMask)));

    /// <summary>Replaces both independent flip bits.</summary>
    public SnesBgTilemapWord WithFlips(SnesTileFlipFlags flips)
    {
        ValidateFlips(flips);
        return new(unchecked((ushort)((Raw & ~FlipMask) | (ushort)flips)));
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

    public static implicit operator SnesBgTilemapWord(ushort raw) => new(raw);
    public static implicit operator ushort(SnesBgTilemapWord word) => word.Raw;
}

/// <summary>A lossless view over the two-byte attribute word in one low-OAM record.</summary>
public readonly record struct SnesObjAttributeWord(ushort Raw)
{
    private const ushort TileNumberMask = 0x01ff;
    private const int PaletteShift = 9;
    private const int PriorityShift = 12;
    private const ushort ThreeBitMask = 0x0007;
    private const ushort TwoBitMask = 0x0003;
    private const ushort FlipMask = 0xc000;

    public int TileNumber => Raw & TileNumberMask;
    public int PaletteIndex => (Raw >> PaletteShift) & ThreeBitMask;
    public int Priority => (Raw >> PriorityShift) & TwoBitMask;
    public ushort PaletteBits => unchecked((ushort)(Raw & PaletteFieldMask));
    public SnesTileFlipFlags FlipFlags => (SnesTileFlipFlags)(Raw & FlipMask);
    public bool FlipHorizontally => (FlipFlags & SnesTileFlipFlags.Horizontal) != 0;
    public bool FlipVertically => (FlipFlags & SnesTileFlipFlags.Vertical) != 0;

    private const ushort PaletteFieldMask = ThreeBitMask << PaletteShift;
    private const ushort PriorityFieldMask = TwoBitMask << PriorityShift;

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

    /// <summary>Replaces only the nine-bit tile number.</summary>
    public SnesObjAttributeWord WithTileNumber(int tileNumber)
    {
        if ((uint)tileNumber > TileNumberMask)
            throw new ArgumentOutOfRangeException(nameof(tileNumber));
        return new(unchecked((ushort)((Raw & ~TileNumberMask) | tileNumber)));
    }

    /// <summary>Replaces only the two-bit OBJ priority.</summary>
    public SnesObjAttributeWord WithPriority(int priority)
    {
        if ((uint)priority > TwoBitMask)
            throw new ArgumentOutOfRangeException(nameof(priority));
        return new(unchecked((ushort)(
            (Raw & ~PriorityFieldMask) | (priority << PriorityShift))));
    }

    /// <summary>Replaces both independent flip bits.</summary>
    public SnesObjAttributeWord WithFlips(SnesTileFlipFlags flips)
    {
        ValidateFlips(flips);
        return new(unchecked((ushort)((Raw & ~FlipMask) | (ushort)flips)));
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

    public static implicit operator SnesObjAttributeWord(ushort raw) => new(raw);
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
    private const byte LargeObjectMask = 0x02;

    public byte Raw { get; }

    public bool XHigh => (Raw & XHighMask) != 0;
    public bool IsLarge => (Raw & LargeObjectMask) != 0;

    public static SnesOamHighTablePair Create(bool xHigh, bool isLarge) =>
        new(unchecked((byte)((xHigh ? XHighMask : 0) | (isLarge ? LargeObjectMask : 0))));

    /// <summary>Builds the pair from a nine-bit screen X and a spritemap size selector.</summary>
    public static SnesOamHighTablePair FromSprite(ushort screenX, bool isLarge) =>
        Create((screenX & 0x0100) != 0, isLarge);

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
public readonly record struct SnesSpritemapXWord(ushort Raw)
{
    private const ushort OffsetMask = 0x01ff;
    private const ushort LargeObjectMask = 0x8000;

    public int UnsignedOffset => Raw & OffsetMask;
    public bool IsLarge => (Raw & LargeObjectMask) != 0;

    public static implicit operator SnesSpritemapXWord(ushort raw) => new(raw);
}
