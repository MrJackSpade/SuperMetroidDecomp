using static SuperMetroid.Core.Map.MapTileWordMasks;
namespace SuperMetroid.Core.Map;

/// <summary>A lossless view over one cartridge map tilemap word.</summary>
/// <remarks>
/// Map cells use the ordinary SNES BG tile layout, but bank <c>$82</c> gives one palette
/// bit additional gameplay meaning: clearing bit <c>$0400</c> changes a discovered cell
/// from the downloaded-map palette to the explored palette. Keeping that operation here
/// prevents pause-map and HUD code from open-coding different masks while retaining every
/// character, priority, and flip bit supplied by the cartridge.
/// </remarks>
/// <param name="Raw">Unmodified 16-bit BG word: ten character bits, three palette bits, priority at bit 13, and horizontal/vertical flips at bits 14/15.</param>
public readonly record struct MapTileWord(ushort Raw)
{

    /// <summary>Low ten-bit map character selected by this cell.</summary>
    public ushort CharacterIndex => (ushort)(Raw & CharacterMask);

    /// <summary>Three-bit SNES BG palette index retained from the native word.</summary>
    public byte PaletteIndex => (byte)((Raw & PaletteMask) >> 10);

    /// <summary>Whether the native BG priority bit is set.</summary>
    public bool HasPriority => (Raw & PriorityMask) != 0;

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

    /// <summary>Returns the complete native BG word without stripping map palette meaning or display attributes.</summary>
    /// <param name="word">Typed map cell whose stored bits are to be published or transferred.</param>
    /// <returns>The unchanged 16-bit value, including character, palette, priority, and flip fields.</returns>
    public static explicit operator ushort(MapTileWord word) => word.Raw;
}
