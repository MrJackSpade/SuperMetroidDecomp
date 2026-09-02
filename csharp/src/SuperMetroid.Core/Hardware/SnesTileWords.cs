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
    internal const ushort CharacterMask = 0x03ff;
    internal const int PaletteShift = 10;
    internal const ushort PaletteMask = 0x0007;
    internal const ushort PriorityMask = 0x2000;
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
    public ushort ToggleFlips(SnesTileFlipFlags flags) => (ushort)(Raw ^ (ushort)flags);

    public static implicit operator SnesBgTilemapWord(ushort raw) => new(raw);
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
    public SnesTileFlipFlags FlipFlags => (SnesTileFlipFlags)(Raw & FlipMask);
    public bool FlipHorizontally => (FlipFlags & SnesTileFlipFlags.Horizontal) != 0;
    public bool FlipVertically => (FlipFlags & SnesTileFlipFlags.Vertical) != 0;

    public static implicit operator SnesObjAttributeWord(ushort raw) => new(raw);
}
