namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge addresses and fixed memory layouts used while loading room graphics.
/// </summary>
/// <remarks>
/// These values describe immutable retail-ROM data and native DMA layouts; they are not
/// runtime room state. Keeping them outside the loaders gives each numeric field a domain
/// name and prevents a SNES address, byte count, and VRAM word destination from being
/// passed interchangeably merely because all three happen to fit in an integer.
/// </remarks>
public static class RoomAssetRomData
{
    /// <summary>Native nine-byte graphics-set records and their shared CRE inputs.</summary>
    public static class Tilesets
    {

        /// <summary><c>$B9:8000 Tiles_CRE</c>, the common-room-element character stream.</summary>
        public const int CreCharactersAddress = 0xb98000;

        /// <summary><c>$B9:A09D TileTable_CRE</c>, the common 16x16 block stream.</summary>
        public const int CreBlockDefinitionsAddress = 0xb9a09d;
    }

    /// <summary>Fixed BG-character, block-table, palette, and VRAM allocation geometry.</summary>
    public static class GraphicsLayout
    {

        /// <summary>Bytes in one 16x16 block definition (four packed tilemap words).</summary>
        public const int BytesPerBlockDefinition = 8;

        /// <summary>Palette bytes copied by <c>LoadCRETilesTilesetTilesAndPalette</c>.</summary>
        public const int BackgroundPaletteByteCount = 0x0100;
    }

    /// <summary>Bank-$82 library-background command memory and transfer definitions.</summary>
    public static class LibraryBackground
    {
        /// <summary>Direct-ROM character upload for the Tourian entrance statue's ghostly BG effect.</summary>
        public static class TourianStatueGhost
        {
            /// <summary><c>$87:AD64</c>, the 48 raw 4-bpp ghost characters named <c>kTileData_TourianEntranceStatueGhost</c>.</summary>
            public const int SourceAddress = 0x87ad64;
        }

        /// <summary>
        /// HUD BG3 character base selected by command eight's BG34NBA=$02 write while
        /// Kraid occupies the ordinary $4000 character region with his private BG2 map.
        /// </summary>
        public const ushort KraidHudCharacterBaseWord = 0x2000;
    }
    /// <summary>$8A:B180: first of the seven contiguous 32x32 scrolling-sky tilemap pages.</summary>
    public const int ScrollingSkyTilemapPages = 0x8ab180;
}
