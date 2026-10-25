namespace SuperMetroid.Core.Hardware;

/// <summary>A lossless view over one standard SNES background tilemap entry.</summary>
/// <param name="Raw">The complete 16-bit entry, including character, palette, priority, and flip fields.</param>
[EncodingOwner("background tilemap entry word")]
public readonly record struct SnesBgTilemapWord(ushort Raw)
{
    /// <summary>Bits 0 through 9 address one of 1024 background characters.</summary>
    private const ushort CharacterMask = 0x03ff;
    /// <summary>The background palette field begins immediately after the character number.</summary>
    private const int PaletteShift = 10;
    /// <summary>Each background tile selects one of eight palette groups.</summary>
    private const ushort PaletteMask = 0x0007;
    /// <summary>Bit 13 raises this tile above the background's lower priority tier.</summary>
    internal const ushort PriorityMask = 0x2000;
    /// <summary>The two upper bits carry horizontal and vertical reflection independently.</summary>
    private const ushort FlipMask = 0xc000;

    /// <summary>Bit 14, the horizontal-mirror flag.</summary>
    internal const ushort HorizontalFlipMask = (ushort)TileFlip.Horizontal;

    /// <summary>Bit 15, the vertical-mirror flag.</summary>
    internal const ushort VerticalFlipMask = (ushort)TileFlip.Vertical;

    /// <summary>Gets the ten-bit character number, from 0 through 1023, relative to the background's character base.</summary>
    public int CharacterIndex => Raw & CharacterMask;
    /// <summary>Gets the three-bit background palette selector, from 0 through 7.</summary>
    public int PaletteIndex => (Raw >> PaletteShift) & PaletteMask;
    /// <summary>Gets whether bit 13 selects this background tile's higher priority tier.</summary>
    public bool HasPriority => (Raw & PriorityMask) != 0;
    /// <summary>Gets the horizontal and vertical reflection bits in their original packed positions.</summary>
    public TileFlip FlipFlags => (TileFlip)(Raw & FlipMask);
    /// <summary>Gets whether pixels are reflected across the tile's horizontal axis of traversal.</summary>
    public bool FlipHorizontally => (FlipFlags & TileFlip.Horizontal) != 0;
    /// <summary>Gets whether rows are drawn in reverse vertical order.</summary>
    public bool FlipVertically => (FlipFlags & TileFlip.Vertical) != 0;

    /// <summary>
    /// Toggles only the requested PPU flip bits, exactly matching the XOR performed while
    /// expanding a flipped 16×16 room block into four 8×8 entries.
    /// </summary>
    public SnesBgTilemapWord ToggleFlips(TileFlip flags)
    {
        ValidateFlips(flags);
        return new(unchecked((ushort)(Raw ^ (ushort)flags)));
    }

    /// <summary>Encodes every documented SNES BG tilemap field into one lossless word.</summary>
    public static SnesBgTilemapWord Create(
        int characterIndex,
        int paletteIndex,
        bool priority,
        TileFlip flips = TileFlip.None)
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
        ushort fieldMask = unchecked(PaletteMask << PaletteShift);
        return new SnesBgTilemapWord(unchecked((ushort)((Raw & ~fieldMask) | paletteBits)));
    }

    /// <summary>Rejects flags outside the two reflection bits before packing a background word.</summary>
    private static void ValidateFlips(TileFlip flips)
    {
        if ((flips & ~(TileFlip.Horizontal | TileFlip.Vertical)) != 0)
            throw new ArgumentOutOfRangeException(nameof(flips));
    }

    /// <summary>Wraps a packed background entry without discarding or validating any bits.</summary>
    public static implicit operator SnesBgTilemapWord(ushort raw) => new(raw);
    /// <summary>Returns the complete background entry for storage in a native tilemap.</summary>
    public static implicit operator ushort(SnesBgTilemapWord word) => word.Raw;
}
