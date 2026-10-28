namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room-effect cartridge addresses, words and layouts only verification reads (#1415): the ROM oracles' view of the
/// tables Core no longer holds.
/// </summary>
internal static class RoomFxNativeSources
{
    /// <summary>SNES bank base containing the animated-tile objects, programs and artwork.</summary>
    public const int AnimatedTilesBank = 0x870000;

    /// <summary>Bank-$83 pointer table at $AC46 for each area's palette-FX object list.</summary>
    public const int AreaPaletteFxObjectListPointers = 0x83ac46;

    /// <summary>$89:AA02, the colour words blended into the temporary effect palette entries.</summary>
    public const int PaletteBlendColors = 0x89aa02;

    /// <summary>Bank-$A0 table at $872D of BG displacement records for earthquake types.</summary>
    public const int EarthquakeDisplacementTable = 0xa0872d;

    /// <summary>Byte stride between consecutive earthquake displacement records.</summary>
    public const int EarthquakeDisplacementRecordBytes = 8;

    /// <summary>First acid character frame at $87:A6A4.</summary>
    public const ushort AcidFirstFrame = 0xa6a4;

    /// <summary>First lava character frame at $87:A564.</summary>
    public const ushort LavaFirstFrame = 0xa564;

    /// <summary>First rain character frame at $87:A874.</summary>
    public const ushort RainFirstFrame = 0xa874;

    /// <summary>First lava instruction-list entry at $87:8293.</summary>
    public const ushort LavaFirstInstruction = 0x8293;

    /// <summary>First acid instruction-list entry at $87:82B1.</summary>
    public const ushort AcidFirstInstruction = 0x82b1;

    /// <summary>First rain instruction-list entry at $87:82CF.</summary>
    public const ushort RainFirstInstruction = 0x82cf;

    /// <summary>Liquid-options bit selecting the vertical BG2 wave used by heat rooms.</summary>
    public const ushort VerticalBg2WaveOption = 0x0002;

    /// <summary>Liquid-options bit seven selects the ±8-pixel tide.</summary>
    public const ushort SmallTideOption = 0x0080;

    /// <summary>Liquid-options bit six selects the ±32-pixel tide.</summary>
    public const ushort LargeTideOption = 0x0040;

    /// <summary>
    /// Blank tilemap word copied through the gameplay portion of the first BG3 page by
    /// <c>Load_StandardBG3Tiles_SpriteTiles_ClearTilemaps</c> at $82:82E2.
    /// </summary>
    public const ushort PaddingTilemapWord = 0x006f;

    /// <summary>Words from BG3 row four through the end of its first 32-row page.</summary>
    public const int PaddingWordCount = 0x0380;

    /// <summary>
    /// BG3SC base used by liquid effects. Its vertical-size bit exposes the HUD/padding
    /// page followed by the room-effect page as one 32-by-64 tilemap.
    /// </summary>
    public const ushort LiquidTilemapBaseWord = 0x5800;

    /// <summary>
    /// BG3SC base selected directly by the rain and fog initializers at $88:D950 and
    /// $88:DB08. These effects use only the second 32-by-32 page.
    /// </summary>
    public const ushort FullScreenAtmosphereTilemapBaseWord = 0x5c00;

    /// <summary>The liquid BG3 tilemap: one screen wide, two screens tall (32 by 64 characters).</summary>
    public static Extent16<TilemapCells> LiquidTilemapSize =>
        new(ScreenGeometry.TilemapScreenWidthInTiles, 2 * ScreenGeometry.TilemapScreenWidthInTiles);

    /// <summary>Rain and fog's BG3 tilemap: one 32-by-32 screen.</summary>
    public static Extent16<TilemapCells> FullScreenAtmosphereTilemapSize =>
        new(ScreenGeometry.TilemapScreenWidthInTiles, ScreenGeometry.TilemapScreenWidthInTiles);

    /// <summary>
    /// First word cleared by <c>Clear_FX_Tilemap</c> at $82:E566 and
    /// <c>ClearFXTilemap</c> at $80:A29C before a room's effect tilemap is uploaded.
    /// </summary>
    public const ushort ClearDestinationWord = 0x5880;

    /// <summary>VRAM word destination of a room effect's layer-three tilemap.</summary>
    public const ushort TilemapDestinationWord = 0x5be0;

    /// <summary>First CGRAM index receiving the shared three-color FX blend.</summary>
    public const int PaletteBlendDestinationIndex = 25;
}
