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
    /// <summary>One fixed WRAM fill followed by a DMA to a VRAM word destination.</summary>
    public readonly record struct TilemapTransfer(
        int WorkRamSourceAddress,
        ushort WorkRamDestination,
        ushort ByteCount,
        ushort FillValue,
        ushort VramDestinationWord);

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
        /// <summary>Bytes occupied by one SNES four-bit-per-pixel 8x8 character.</summary>
        public const int BytesPer4BppCharacter = 32;

        /// <summary>VRAM byte destination used for area-specific BG characters.</summary>
        public const int AreaCharactersVramByteOffset = 0x0000;

        /// <summary>VRAM byte destination used for common-room-element BG characters.</summary>
        public const int CreCharactersVramByteOffset = 0x5000;

        /// <summary>Bytes modeled by the static room renderer's BG character allocation.</summary>
        public const int BackgroundCharacterVramByteCount = 0x8000;

        /// <summary>Bytes in the decompressed CRE 16x16 block-definition table.</summary>
        public const int CreBlockDefinitionsByteCount = 0x0800;

        /// <summary>Bytes in one 16x16 block definition (four packed tilemap words).</summary>
        public const int BytesPerBlockDefinition = 8;

        /// <summary>Number of CRE block definitions preceding area-specific blocks.</summary>
        public const int CreBlockDefinitionCount =
            CreBlockDefinitionsByteCount / BytesPerBlockDefinition;

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

            /// <summary>48 complete 4-bpp characters uploaded by the native library-background command.</summary>
            public const ushort TransferByteCount = 0x0600;
        }

        /// <summary>Lowest full 24-bit SNES ROM address for command-source classification.</summary>
        public const int RomSourceAddressFloor = 0x800000;

        /// <summary>Bank containing the shared decompression and tilemap staging buffers.</summary>
        public const int WorkRamBank = 0x7e0000;

        /// <summary>Exclusive byte offset at the end of one SNES address bank.</summary>
        public const int BankByteCount = 0x10000;

        /// <summary>Maximum commands accepted before a malformed unterminated list fails.</summary>
        public const int MaximumCommandsPerList = 128;

        /// <summary>Unused native clear-FX command's fill and transfer definition.</summary>
        public static readonly TilemapTransfer ClearFx =
            new(0x7e4000, 0x4000, 0x0f00, 0x184e, 0x5880);

        /// <summary>Ordinary clear-BG2 command's fill and primary transfer definition.</summary>
        public static readonly TilemapTransfer ClearBg2 =
            new(0x7e4000, 0x4000, 0x1000, 0x0338, 0x4800);

        /// <summary>Kraid's additional destination for the already-filled BG2 buffer.</summary>
        public const ushort KraidBg2VramDestinationWord = 0x4000;

        /// <summary>
        /// HUD BG3 character base selected by command eight's BG34NBA=$02 write while
        /// Kraid occupies the ordinary $4000 character region with his private BG2 map.
        /// </summary>
        public const ushort KraidHudCharacterBaseWord = 0x2000;
    }
}
