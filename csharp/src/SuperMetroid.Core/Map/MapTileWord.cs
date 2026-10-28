using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Map;

/// <summary>
/// The codec of one area-map cell word: the single type that reads and writes a map cell's character, palette,
/// priority and flip fields and applies the map's explored-palette and HUD attribute operations to them.
/// </summary>
/// <remarks>
/// Map cells use the ordinary SNES BG tile layout, but bank <c>$82</c> gives one palette
/// bit additional gameplay meaning: clearing bit <c>$0400</c> changes a discovered cell
/// from the downloaded-map palette to the explored palette. Keeping that operation here
/// prevents pause-map and HUD code from open-coding different masks while retaining every
/// character, priority, and flip bit supplied by the cartridge.
/// </remarks>
/// <param name="Raw">Unmodified 16-bit BG word: ten character bits, three palette bits, priority at bit 13, and horizontal/vertical flips at bits 14/15.</param>
[EncodingOwner("area map cell word")]
public readonly record struct MapTileWord(ushort Raw)
{
    /// <summary>Ten low bits selecting the BG map character independently of its attributes.</summary>
    internal const ushort CharacterMask = 0x03ff;

    /// <summary>Three BG palette bits, including the explored-cell bit.</summary>
    internal const ushort PaletteMask = 0x1c00;

    /// <summary>The BG priority bit preserved when presenting a tile.</summary>
    internal const ushort PriorityMask = 0x2000;

    /// <summary>The map cell's horizontal-mirror bit.</summary>
    internal const ushort HorizontalFlipMask = 0x4000;

    /// <summary>The map cell's vertical-mirror bit.</summary>
    internal const ushort VerticalFlipMask = 0x8000;

    /// <summary>Horizontal and vertical BG flip bits preserved by HUD projection.</summary>
    internal const ushort FlipMask = HorizontalFlipMask | VerticalFlipMask;

    /// <summary>Downloaded-map palette bit cleared when a cell has been explored.</summary>
    internal const ushort ExploredPaletteBit = 0x0400;

    /// <summary>HUD palette and priority for a visited minimap cell.</summary>
    internal const ushort HudExploredAttributes = 0x2800;

    /// <summary>HUD palette and priority for an unvisited minimap cell.</summary>
    internal const ushort HudUnexploredAttributes = 0x2c00;

    /// <summary>Alternating Samus marker palette bits applied during minimap blink.</summary>
    internal const ushort HudLocationBlinkAttributes = 0x1c00;

    /// <summary>The nine low character bits $90:AAFD compares, ignoring character bit nine and every attribute.</summary>
    private const ushort SlopedHallwayIdentityMask = 0x01ff;

    /// <summary>Low ten-bit map character selected by this cell.</summary>
    public ushort CharacterIndex => (ushort)(Raw & CharacterMask);

    /// <summary>Three-bit SNES BG palette index retained from the native word.</summary>
    public byte PaletteIndex => (byte)((Raw & PaletteMask) >> 10);

    /// <summary>Whether the native BG priority bit is set.</summary>
    public bool HasPriority => (Raw & PriorityMask) != 0;

    /// <summary>
    /// Whether this cell shows the sloped-hallway character, as $90:AAFD recognises it from the character's nine low
    /// bits regardless of flips and attributes.
    /// </summary>
    public bool IsSlopedHallway => (Raw & SlopedHallwayIdentityMask) == MapTileWords.SlopedHallwayCharacter;

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

    /// <summary>The background tile the cell places: its character, palette, mirroring and priority.</summary>
    public BackgroundTile ToTile() =>
        new(CharacterIndex, PaletteIndex, (Raw & HorizontalFlipMask) != 0, (Raw & VerticalFlipMask) != 0, HasPriority);

    /// <summary>Returns the complete native BG word without stripping map palette meaning or display attributes.</summary>
    /// <param name="word">Typed map cell whose stored bits are to be published or transferred.</param>
    /// <returns>The unchanged 16-bit value, including character, palette, priority, and flip fields.</returns>
    public static explicit operator ushort(MapTileWord word) => word.Raw;
}
