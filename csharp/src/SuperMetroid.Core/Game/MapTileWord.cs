using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Named values used when the cartridge map tilemap is projected into either the pause
/// screen or the five-by-three HUD minimap.
/// </summary>
public static class MapTileWords
{
    /// <summary>$90:AAFD masks the center tile to nine character bits, ignoring flips and bit nine.</summary>
    public const ushort SlopedHallwayIdentityMask = 0x01ff;

    /// <summary>$90:AB00 recognizes character $028 and marks the map cell above Samus explored.</summary>
    public const ushort SlopedHallwayCharacter = 0x0028;
    /// <summary>$82:9517 uses character $00F for hidden cells without a downloaded map.</summary>
    public static readonly MapTileWord FileSelectUndownloadedBlank = new(0x000f);

    /// <summary>The cartridge's character-$01F empty map cell, without display attributes.</summary>
    public static readonly MapTileWord PauseBlank = new(0x001f);

    /// <summary>The same empty cell using the HUD minimap's unvisited priority/palette.</summary>
    public static readonly MapTileWord HudBlank = new(0x2c1f);
}

/// <summary>
/// Native coordinate addressing for one 64-by-32 area map. Both the tilemap and one-bit
/// presence/exploration planes are stored as two adjacent 32-by-32 pages rather than as
/// conventional 64-column rows.
/// </summary>
public static class AreaMapLayout
{
    /// <summary>Logical area-map width of 64 cells, with the left and right halves stored in separate native pages.</summary>
    public const int WidthInTiles = 64;
    /// <summary>Logical area-map height of 32 cells, shared by both horizontal pages.</summary>
    public const int HeightInTiles = 32;
    /// <summary>Width of each 32-by-32 native page; X coordinates 32..63 select the second page rather than extending a 64-word physical row.</summary>
    public const int PageWidthInTiles = 32;
    /// <summary>$80 bytes per presence or exploration page: 32 rows of four MSB-first bit bytes, one bit per map cell.</summary>
    public const int BitPlaneBytesPerPage = 0x80;
    /// <summary>$0400 BG tile words per page, arranged as 32 rows of 32 cells; the second page begins after these 1024 words.</summary>
    public const int TilemapWordsPerPage = 0x400;

    /// <summary>Returns the native byte offset containing one map bit.</summary>
    public static int GetBitByteIndex(int mapX, int mapY)
    {
        ValidateCoordinate(mapX, mapY);
        int pageOffset = mapX >= PageWidthInTiles ? BitPlaneBytesPerPage : 0;
        int byteColumnWithinPage = (mapX & (PageWidthInTiles - 1)) >> 3;
        return pageOffset + mapY * 4 + byteColumnWithinPage;
    }

    /// <summary>Returns the native word offset containing one map tile.</summary>
    public static int GetTilemapWordIndex(int mapX, int mapY)
    {
        ValidateCoordinate(mapX, mapY);
        int pageOffset = mapX >= PageWidthInTiles ? TilemapWordsPerPage : 0;
        return pageOffset + mapY * PageWidthInTiles + (mapX & (PageWidthInTiles - 1));
    }

    /// <summary>Returns the MSB-first mask used by both cartridge one-bit planes.</summary>
    public static byte GetBitMask(int mapX)
    {
        if ((uint)mapX >= WidthInTiles)
            throw new ArgumentOutOfRangeException(nameof(mapX));
        return unchecked((byte)(0x80 >> (mapX & 7)));
    }

    /// <summary>Rejects coordinates outside the logical 64-by-32 area map before calculating native offsets.</summary>
    /// <param name="mapX">Zero-based horizontal map coordinate.</param>
    /// <param name="mapY">Zero-based vertical map coordinate.</param>
    private static void ValidateCoordinate(int mapX, int mapY)
    {
        if ((uint)mapX >= WidthInTiles)
            throw new ArgumentOutOfRangeException(nameof(mapX));
        if ((uint)mapY >= HeightInTiles)
            throw new ArgumentOutOfRangeException(nameof(mapY));
    }
}

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
    /// <summary>Selects the ten low bits containing the SNES BG character number.</summary>
    private const ushort CharacterMask = 0x03ff;
    /// <summary>Selects the three BG palette bits at positions 10 through 12.</summary>
    private const ushort PaletteMask = 0x1c00;
    /// <summary>Selects the BG priority bit at position 13.</summary>
    private const ushort PriorityMask = 0x2000;
    /// <summary>Selects the horizontal and vertical flip bits at positions 14 and 15.</summary>
    private const ushort FlipMask = 0xc000;
    /// <summary>Palette bit cleared by the native pause-map operation to mark a discovered cell explored.</summary>
    private const ushort ExploredPaletteBit = 0x0400;
    /// <summary>Palette and priority attributes installed by the HUD for explored cells.</summary>
    private const ushort HudExploredAttributes = 0x2800;
    /// <summary>Palette and priority attributes installed by the HUD for unexplored cells.</summary>
    private const ushort HudUnexploredAttributes = 0x2c00;
    /// <summary>Palette attributes ORed into the HUD tile while the Samus-location marker blinks.</summary>
    private const ushort HudLocationBlinkAttributes = 0x1c00;

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
