using System.Buffers.Binary;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Decompressed visual/collision assets selected by one cartridge room-state header.
/// </summary>
/// <remarks>
/// This is the reusable, data-only portion of <c>LoadCRETilesTilesetTilesAndPalette</c> and
/// <c>LoadLevelDataAndOtherThings</c>. It deliberately does not execute arbitrary room ASM;
/// those bank-$8F setup/main functions remain separate behavior to translate per room.
/// </remarks>
public sealed class CartridgeRoomAssets
{
    private const int TilesetPointerTable = 0x8fe7a7;
    private const int TilesetBank = 0x8f0000;
    private const int CreTilesAddress = 0xb98000;
    private const int CreBlockDefinitionsAddress = 0xb9a09d;

    private CartridgeRoomAssets(
        CartridgeRoomHeader header,
        RoomLevelData levelData,
        RoomScrollGrid scrolls,
        byte[] creCharacters,
        byte[] roomCharacters,
        byte[] paletteBytes,
        TilesetDefinition tileset)
    {
        Header = header;
        LevelData = levelData;
        Scrolls = scrolls;
        CreCharacters = creCharacters;
        RoomCharacters = roomCharacters;
        PaletteBytes = paletteBytes;
        Tileset = tileset;
    }

    public CartridgeRoomHeader Header { get; }
    public RoomLevelData LevelData { get; }
    public RoomScrollGrid Scrolls { get; }
    public byte[] CreCharacters { get; }
    public byte[] RoomCharacters { get; }
    public byte[] PaletteBytes { get; }
    public TilesetDefinition Tileset { get; }

    /// <summary>Reads every compressed input named by the selected room and graphics set.</summary>
    public static CartridgeRoomAssets Load(ISnesAddressSpace bus, CartridgeRoomHeader header)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(header);
        TilesetDefinition tileset = ReadTileset(bus, header.State.GraphicsSet);

        byte[] roomBlockDefinitions = RomDataReader.Decompress(bus, tileset.BlockDefinitionsAddress);
        byte[] blockDefinitions;
        if (header.AreaIndex == 6)
        {
            // $82:E7D3 installs a Ceres tileset directly at WRAM $A000. Unlike Zebes
            // rooms, area six does not reserve the first $800 bytes for common CRE blocks.
            blockDefinitions = roomBlockDefinitions;
        }
        else
        {
            byte[] creDefinitions = RomDataReader.Decompress(bus, CreBlockDefinitionsAddress);
            blockDefinitions = new byte[creDefinitions.Length + roomBlockDefinitions.Length];
            creDefinitions.CopyTo(blockDefinitions, 0);
            roomBlockDefinitions.CopyTo(blockDefinitions, creDefinitions.Length);
        }

        if (blockDefinitions.Length == 0 || (blockDefinitions.Length & 7) != 0)
        {
            throw new InvalidDataException(
                $"Graphics set ${header.State.GraphicsSet:X2} produced ${blockDefinitions.Length:X} " +
                "bytes of block definitions; native blocks are eight bytes each.");
        }

        byte[] levelStream = RomDataReader.Decompress(bus, header.State.CompressedLevelDataAddress);
        RoomLevelData levelData = ParseLevelData(header, levelStream, blockDefinitions);
        RoomScrollGrid scrolls = LoadScrolls(bus, header);
        return new CartridgeRoomAssets(
            header,
            levelData,
            scrolls,
            RomDataReader.Decompress(bus, CreTilesAddress),
            RomDataReader.Decompress(bus, tileset.CharacterAddress),
            RomDataReader.Decompress(bus, tileset.PaletteAddress),
            tileset);
    }

    /// <summary>Installs the room's two character regions and its first 128 CGRAM colors.</summary>
    public void LoadGraphics(SnesVram vram, SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);

        // Preserve $82:E783's transfer order, not merely its two destinations. Ordinary
        // area art ends at byte $5000, but Ceres tileset $11 expands to the full $8000-byte
        // BG character allocation. The second transfer from VRAM byte $0000 must therefore
        // overwrite the CRE upload at $5000..$7FFF; reversing these calls produces the
        // characteristic vertical-stripe corruption seen when CRE replaces Ceres tiles.
        vram.LoadBytes(0x5000, CreCharacters);
        vram.LoadBytes(0x0000, RoomCharacters);
        if (PaletteBytes.Length < 0x0100)
        {
            throw new InvalidDataException(
                $"Tileset palette expanded to ${PaletteBytes.Length:X} bytes; expected at least $100.");
        }
        cgram.LoadBytes(PaletteBytes.AsSpan(0, 0x0100), destinationIndex: 0);
    }

    private static TilesetDefinition ReadTileset(ISnesAddressSpace bus, byte graphicsSet)
    {
        ushort pointer = RomDataReader.ReadWordFixedBank(
            bus,
            TilesetPointerTable + graphicsSet * 2);
        int address = TilesetBank | pointer;
        return new TilesetDefinition(
            pointer,
            BlockDefinitionsAddress: RomDataReader.ReadLongFixedBank(bus, address),
            CharacterAddress: RomDataReader.ReadLongFixedBank(bus, address + 3),
            PaletteAddress: RomDataReader.ReadLongFixedBank(bus, address + 6));
    }

    private static RoomLevelData ParseLevelData(
        CartridgeRoomHeader header,
        byte[] levelStream,
        byte[] blockDefinitions)
    {
        int blockCount = checked(header.WidthInScreens * 16 * header.HeightInScreens * 16);
        int layerByteCount = checked(blockCount * 2);
        if (levelStream.Length < 2)
            throw new InvalidDataException("Compressed room level stream has no size word.");

        int declaredLayerBytes = BinaryPrimitives.ReadUInt16LittleEndian(levelStream);
        if (declaredLayerBytes != layerByteCount)
        {
            throw new InvalidDataException(
                $"Room $8F:{header.Pointer:X4} declares ${declaredLayerBytes:X} BG1 bytes, " +
                $"but its {header.WidthInScreens}x{header.HeightInScreens} header requires ${layerByteCount:X}.");
        }

        int foregroundOffset = 2;
        int behaviorOffset = foregroundOffset + layerByteCount;
        int backgroundOffset = behaviorOffset + blockCount;
        if (levelStream.Length < backgroundOffset + layerByteCount)
        {
            throw new InvalidDataException(
                $"Room $8F:{header.Pointer:X4} level stream is missing its complete BG1/BTS/BG2 allocation.");
        }

        ushort[] streamingAllocation = ReadWords(levelStream.AsSpan(foregroundOffset));
        ushort[] foreground = ReadWords(levelStream.AsSpan(foregroundOffset, layerByteCount));
        byte[] behavior = levelStream.AsSpan(behaviorOffset, blockCount).ToArray();
        ushort[] background = ReadWords(levelStream.AsSpan(backgroundOffset, layerByteCount));
        return new RoomLevelData(
            header.WidthInScreens * 16,
            header.HeightInScreens * 16,
            foreground,
            behavior,
            background,
            blockDefinitions,
            streamingAllocation);
    }

    private static RoomScrollGrid LoadScrolls(ISnesAddressSpace bus, CartridgeRoomHeader header)
    {
        short scrollPointer = unchecked((short)header.State.ScrollPointer);
        return scrollPointer < 0
            ? RoomScrollGrid.LoadExplicit(
                bus,
                0x8f0000 | header.State.ScrollPointer,
                header.WidthInScreens,
                header.HeightInScreens)
            : RoomScrollGrid.CreateImplicit(
                bus,
                header.WidthInScreens,
                header.HeightInScreens,
                unchecked((byte)(header.State.ScrollPointer + 1)));
    }

    private static ushort[] ReadWords(ReadOnlySpan<byte> bytes)
    {
        var words = new ushort[bytes.Length / 2];
        for (int index = 0; index < words.Length; index++)
            words[index] = BinaryPrimitives.ReadUInt16LittleEndian(bytes[(index * 2)..]);
        return words;
    }
}

/// <summary>One nine-byte bank-$8F tileset definition selected by graphics-set index.</summary>
public readonly record struct TilesetDefinition(
    ushort Pointer,
    int BlockDefinitionsAddress,
    int CharacterAddress,
    int PaletteAddress);
