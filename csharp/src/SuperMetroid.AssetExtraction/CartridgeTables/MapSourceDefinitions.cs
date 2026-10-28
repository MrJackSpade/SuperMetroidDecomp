
namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the pause and file-select maps' screens, landmarks, sprites, colours and animations, and
/// the shared menu, map and HUD characters (#1416): the import pipeline's private knowledge of the cartridge, decoded
/// once into the installed map presentation.
/// </summary>
internal static class MapSourceDefinitions
{
    /// <summary>The shared menu screens' sources, installed by the menu tile loader at $81:8DDB.</summary>
    internal static class Menu
    {
        /// <summary>$8E:DC00, the shared background page the menus copy; the saved-map views replace or do not draw it.</summary>
        internal const int Background = 0x8edc00;

        /// <summary>The bytes of one 32-by-32 menu tilemap page.</summary>
        internal const int TilemapBytes = 0x800;

        /// <summary>$8E:8000, the menu foreground's 4-bpp characters.</summary>
        internal const int ForegroundCharacters = 0x8e8000;

        /// <summary>$8E:D600, the menu background's 2-bpp characters.</summary>
        internal const int BackgroundCharacters = 0x8ed600;

        /// <summary>$8E:E400, the full menu colour image the file-select palette loader reloads.</summary>
        internal const int Colors = 0x8ee400;

        /// <summary>Bank $81, holding the file-select tilemap text streams.</summary>
        internal const int TilemapTextBank = 0x810000;

        /// <summary>Bank $82, holding the menu spritemaps, map-object coordinate lists and arrow programs.</summary>
        internal const int ObjectBank = 0x820000;

        /// <summary>$82:C569, the menu spritemap pointers, one word per spritemap ordinal.</summary>
        internal const int SpritemapPointers = 0x82c569;

        /// <summary>$B6:C000, the shared menu sprite characters.</summary>
        internal const int ObjectCharacters = 0xb6c000;
    }

    /// <summary>The world (area-select) and room-select map screens' sources.</summary>
    internal static class FileSelectMap
    {
        /// <summary>$81:B71A, the world map's foreground page.</summary>
        internal const int AreaForeground = 0x81b71a;

        /// <summary>$81:BF1A, the six area-background pages, one per Zebes area.</summary>
        internal const int AreaBackgrounds = 0x81bf1a;

        /// <summary>$81:A4CE, the selected area's colour-program offsets, one word per area.</summary>
        internal const int ActivePaletteOffsets = 0x81a4ce;

        /// <summary>$81:A4DA, an unselected area's colour-program offsets, one word per area.</summary>
        internal const int InactivePaletteOffsets = 0x81a4da;

        /// <summary>$81:A4E6, the colour programs: source and destination byte offsets, terminated by $FFFF.</summary>
        internal const int PalettePrograms = 0x81a4e6;

        /// <summary>$81:A40E, the colours the programs copy in five-colour groups.</summary>
        internal const int PaletteColors = 0x81a40e;

        /// <summary>$82:C749, the PLANET ZEBES spritemap ordinal followed by the area labels' ordinals.</summary>
        internal const int LabelSpritemaps = 0x82c749;

        /// <summary>$81:AA1C, the area labels' X and Y words per area.</summary>
        internal const int LabelPositions = 0x81aa1c;

        /// <summary>$B6:E000, the room-select frame's first 800 words.</summary>
        internal const int RoomFrame = 0xb6e000;

        /// <summary>$81:B14B, the control footer: its words 1 to 160 become frame words 800 to 959.</summary>
        internal const int RoomFrameFooter = 0x81b14b;

        /// <summary>The room-select frame's words copied from <see cref="RoomFrame"/> before the remainder is filled.</summary>
        internal const int RoomFrameHeaderWords = 800;

        /// <summary>The control footer's words copied into the room-select frame.</summary>
        internal const int RoomFrameFooterWords = 160;

        /// <summary>The blank word filling the room-select frame's unused words.</summary>
        internal const ushort RoomFrameBlank = 0x2801;

        /// <summary>$82:965F, the area labels' twelve-word tilemaps, one bank-$82 pointer per area.</summary>
        internal const int AreaLabelPointers = 0x82965f;

        /// <summary>The words of one room-select area label.</summary>
        internal const int RoomLabelWords = 12;

        /// <summary>The room-select area label's words keep every bit but palette bit $1000, which the copy clears.</summary>
        internal const ushort RoomLabelMask = 0xefff;

        /// <summary>$81:AF32, the four scroll arrows' ten-byte records: X, Y, animation, held-button mask, direction.</summary>
        internal const int ScrollArrows = 0x81af32;

        /// <summary>The bytes of one scroll-arrow record.</summary>
        internal const int ScrollArrowRecordBytes = 10;
    }

    /// <summary>The seven area maps' tilemaps and map-station reveal planes.</summary>
    internal static class AreaMaps
    {
        /// <summary>$82:964A, kPauseMenuMapTilemaps: one long pointer per area to its packed tilemap.</summary>
        internal const int TilemapPointers = 0x82964a;

        /// <summary>The bytes of one tilemap pointer.</summary>
        internal const int TilemapPointerBytes = 3;

        /// <summary>$82:9717, kPauseMenuMapData: one bank-$82 pointer per area to its map-station reveal plane.</summary>
        internal const int StationRevealMaskPointers = 0x829717;

        /// <summary>Bank $82, holding the map-station reveal planes.</summary>
        internal const int StationRevealMaskBank = 0x820000;

        /// <summary>The bytes of one packed 64-by-32 area tilemap: 2,048 words.</summary>
        internal const int TilemapBytes = 0x1000;

        /// <summary>The bytes of one 64-by-32 one-bit map-station reveal plane.</summary>
        internal const int StationRevealMaskBytes = 0x0100;
    }

    /// <summary>The map landmarks' per-area coordinate lists, bank-$82 pointers indexed by area.</summary>
    internal static class Landmarks
    {
        /// <summary>$82:C7CB, the boss coordinate lists.</summary>
        internal const int BossLists = 0x82c7cb;

        /// <summary>$82:C7DB, the missile-refill coordinate lists.</summary>
        internal const int MissileLists = 0x82c7db;

        /// <summary>$82:C7EB, the energy-refill coordinate lists.</summary>
        internal const int EnergyLists = 0x82c7eb;

        /// <summary>$82:C7FB, the map-station coordinate lists.</summary>
        internal const int MapStationLists = 0x82c7fb;

        /// <summary>$82:C74D, the elevator-destination records: X, Y and spritemap ordinal.</summary>
        internal const int ElevatorLists = 0x82c74d;
    }

    /// <summary>The map sprite and highlight animations' sources.</summary>
    internal static class Animation
    {
        /// <summary>$82:C0E8, pointers to the sprite animations' three-byte records.</summary>
        internal const int SpritePrograms = 0x82c0e8;

        /// <summary>$82:C1E4, pointers to the sprite animations' base spritemap variants.</summary>
        internal const int SpriteBases = 0x82c1e4;

        /// <summary>$82:C10C, the highlight's three-byte timing records, terminated by $FF.</summary>
        internal const int PaletteTiming = 0x82c10c;

        /// <summary>The bytes of one highlight timing record: duration, sprite-frame index and spritemap ordinal.</summary>
        internal const int PaletteTimingStride = 3;

        /// <summary>$82:A987, the highlight's sixteen-colour frames.</summary>
        internal const int PaletteColors = 0x82a987;
    }

    /// <summary>The map screens' static colours and characters.</summary>
    internal static class Artwork
    {
        /// <summary>$B6:F000, the pause screen's full base colour image.</summary>
        internal const int PauseColors = 0xb6f000;

        /// <summary>The colours one world-map colour-program record copies.</summary>
        internal const int CopyColorCount = 5;

        /// <summary>The bytes of one world-map colour-program record: source and destination words.</summary>
        internal const int CopyRecordBytes = 4;

        /// <summary>The most complete colour-program records one bank can hold.</summary>
        internal const int MaximumCopyRecords = 16384;

        /// <summary>$B6:8000, the first 256 4-bpp characters the pause and file-select maps share.</summary>
        internal const int MapCharacters = 0xb68000;

        /// <summary>$B6:A000, the pause interface's second 256 background characters.</summary>
        internal const int PauseInterfaceCharacters = 0xb6a000;

        /// <summary>$9A:B200, the standard 2-bpp HUD characters; Kraid's library background also reads them from here.</summary>
        internal const int HudCharacters = 0x9ab200;
    }
}
