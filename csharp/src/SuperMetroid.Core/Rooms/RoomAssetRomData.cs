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
    /// <summary>One compressed stream together with its exact stored extent in the ROM.</summary>
    public readonly record struct BoundedCompressedAsset(int Address, int StoredByteCount);

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
        /// <summary><c>$8F:E7A7 tileset_table</c>, a table of bank-$8F word pointers.</summary>
        public const int PointerTableAddress = 0x8fe7a7;

        /// <summary>Bank containing both <see cref="PointerTableAddress"/> and its records.</summary>
        public const int DefinitionBank = 0x8f0000;

        /// <summary>Bytes in one tileset record: three consecutive 24-bit addresses.</summary>
        public const int DefinitionByteCount = 9;

        /// <summary>Offset of the compressed 16x16 block-definition address.</summary>
        public const int BlockDefinitionsAddressOffset = 0;

        /// <summary>Offset of the compressed 4bpp character address.</summary>
        public const int CharacterAddressOffset = 3;

        /// <summary>Offset of the compressed palette address.</summary>
        public const int PaletteAddressOffset = 6;

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

    /// <summary>Known bounded streams and room geometry for room <c>$8F:91F8</c>.</summary>
    public static class LandingSite
    {
        /// <summary><c>TileTable_CRE</c> and its exact compressed extent.</summary>
        public static readonly BoundedCompressedAsset CreBlockDefinitions =
            new(0xb9a09d, 0x0597);

        /// <summary>Upper-Crateria block definitions and their exact compressed extent.</summary>
        public static readonly BoundedCompressedAsset AreaBlockDefinitions =
            new(0xc1b6f6, 0x07f8);

        /// <summary>Landing Site BG1/BTS/BG2 stream and its exact compressed extent.</summary>
        public static readonly BoundedCompressedAsset LevelData =
            new(0xc2c2bb, 0x142d);

        /// <summary><c>Tiles_CRE</c> and its exact compressed extent.</summary>
        public static readonly BoundedCompressedAsset CreCharacters =
            new(0xb98000, 0x209d);

        /// <summary>Upper-Crateria characters and their exact compressed extent.</summary>
        public static readonly BoundedCompressedAsset AreaCharacters =
            new(0xbac629, 0x32e8);

        /// <summary>Horizontal screen count declared by room header <c>$8F:91F8</c>.</summary>
        public const int WidthInScreens = 9;

        /// <summary>Vertical screen count declared by room header <c>$8F:91F8</c>.</summary>
        public const int HeightInScreens = 5;

        /// <summary>Number of 16x16 blocks along either axis of one room screen.</summary>
        public const int BlocksPerScreenAxis = 16;
    }

    /// <summary>Bank-$82 library-background command memory and transfer definitions.</summary>
    public static class LibraryBackground
    {
        /// <summary>Bank containing room-authored library-background command lists.</summary>
        public const int CommandBank = 0x8f0000;

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
    }
}
