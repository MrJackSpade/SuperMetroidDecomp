using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Named values used when the cartridge map tilemap is projected into either the pause
/// screen or the five-by-three HUD minimap.
/// </summary>
public static class MapTileWords
{
    /// <summary>The cartridge's character-$01F empty map cell, without display attributes.</summary>
    public static readonly MapTileWord PauseBlank = new(0x001f);

    /// <summary>The same empty cell using the HUD minimap's unvisited priority/palette.</summary>
    public static readonly MapTileWord HudBlank = new(0x2c1f);
}

/// <summary>A lossless view over one cartridge map tilemap word.</summary>
/// <remarks>
/// Map cells use the ordinary SNES BG tile layout, but bank <c>$82</c> gives one palette
/// bit additional gameplay meaning: clearing bit <c>$0400</c> changes a discovered cell
/// from the downloaded-map palette to the explored palette. Keeping that operation here
/// prevents pause-map and HUD code from open-coding different masks while retaining every
/// character, priority, and flip bit supplied by the cartridge.
/// </remarks>
public readonly record struct MapTileWord(ushort Raw)
{
    private const ushort CharacterMask = 0x03ff;
    private const ushort PaletteMask = 0x1c00;
    private const ushort PriorityMask = 0x2000;
    private const ushort FlipMask = 0xc000;
    private const ushort ExploredPaletteBit = 0x0400;
    private const ushort HudExploredAttributes = 0x2800;
    private const ushort HudUnexploredAttributes = 0x2c00;
    private const ushort HudLocationBlinkAttributes = 0x1c00;

    /// <summary>Low ten-bit map character selected by this cell.</summary>
    public ushort CharacterIndex => (ushort)(Raw & CharacterMask);

    /// <summary>Three-bit SNES BG palette index retained from the native word.</summary>
    public byte PaletteIndex => (byte)((Raw & PaletteMask) >> 10);

    /// <summary>Whether the native BG priority bit is set.</summary>
    public bool HasPriority => (Raw & PriorityMask) != 0;

    /// <summary>Independent native horizontal and vertical flip attributes.</summary>
    public SnesTileFlipFlags FlipFlags => (SnesTileFlipFlags)(Raw & FlipMask);

    /// <summary>Whether this word selects the cartridge's empty map character.</summary>
    public bool IsBlank => CharacterIndex == MapTileWords.PauseBlank.CharacterIndex;

    /// <summary>
    /// Applies bank <c>$82:943D</c>'s explored-cell operation, clearing only palette bit
    /// <c>$0400</c> and preserving every other bit in the source map cell.
    /// </summary>
    public MapTileWord AsExplored() => new((ushort)(Raw & ~ExploredPaletteBit));

    /// <summary>
    /// Applies bank <c>$80:9D78</c>'s HUD presentation. The cartridge routine deliberately
    /// replaces palette and priority together while preserving the character and flips.
    /// </summary>
    public MapTileWord ForHud(bool explored) => new((ushort)(
        (Raw & (CharacterMask | FlipMask)) |
        (explored ? HudExploredAttributes : HudUnexploredAttributes)));

    /// <summary>
    /// Applies the alternating Samus-location palette bits used by the HUD minimap blink.
    /// </summary>
    public MapTileWord WithLocationBlink() => new((ushort)(Raw | HudLocationBlinkAttributes));

    public static implicit operator MapTileWord(ushort raw) => new(raw);

    public static explicit operator ushort(MapTileWord word) => word.Raw;
}
