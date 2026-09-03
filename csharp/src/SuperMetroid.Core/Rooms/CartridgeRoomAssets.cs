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
        if (header.AreaIndex == AreaId.Ceres)
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
        int widthInBlocks = checked(header.WidthInScreens * 16);
        int visibleHeightInBlocks = checked(header.HeightInScreens * 16);
        int visibleBlockCount = checked(widthInBlocks * visibleHeightInBlocks);
        if (levelStream.Length < 2)
            throw new InvalidDataException("Compressed room level stream has no size word.");

        int declaredLayerBytes = BinaryPrimitives.ReadUInt16LittleEndian(levelStream);
        if ((declaredLayerBytes & 1) != 0)
        {
            throw new InvalidDataException(
                $"Room $8F:{header.Pointer:X4} declares an odd ${declaredLayerBytes:X} BG1 byte count.");
        }

        // The room header constrains camera screens; the stream's leading word constrains
        // the physical level_data/BTS allocations. Most rooms make them equal, but Wrecked
        // Ship room $C98E is a shipped counterexample: its 6x3 camera owns a 6x4-screen
        // allocation. Bank $84 scripts and enemy collision may address those extra rows, so
        // retain them instead of truncating valid cartridge data or adding a room exception.
        int blockCount = declaredLayerBytes / 2;
        if (blockCount < visibleBlockCount || blockCount % widthInBlocks != 0)
        {
            throw new InvalidDataException(
                $"Room $8F:{header.Pointer:X4} declares ${declaredLayerBytes:X} BG1 bytes " +
                $"({blockCount} blocks), which cannot contain its {header.WidthInScreens}x" +
                $"{header.HeightInScreens} visible screens in complete {widthInBlocks}-block rows.");
        }
        int allocationHeightInBlocks = blockCount / widthInBlocks;
        int layerByteCount = declaredLayerBytes;

        int foregroundOffset = 2;
        int behaviorOffset = foregroundOffset + layerByteCount;
        int backgroundOffset = behaviorOffset + blockCount;
        if (levelStream.Length < backgroundOffset)
        {
            throw new InvalidDataException(
                $"Room $8F:{header.Pointer:X4} level stream is missing its complete BG1/BTS allocation.");
        }

        // `$82:E7D3` clears the complete $6400-byte level_data allocation before the
        // decompressor writes this room's shorter BG1/BTS/BG2 payload over its beginning.
        // Door scrolling legally reads a complete 16-block column beyond a short room's
        // authored bottom edge. Preserve the allocation, including the $8000 tail, rather
        // than sizing this source to however many compressed bytes happened to follow BG1.
        ushort[] streamingAllocation = BuildPrefilledStreamingAllocation(
            levelStream.AsSpan(foregroundOffset));
        ushort[] foreground = ReadWords(levelStream.AsSpan(foregroundOffset, layerByteCount));
        byte[] behavior = levelStream.AsSpan(behaviorOffset, blockCount).ToArray();

        // `LoadLevelDataAndOtherThings` first fills the entire WRAM level allocation with
        // word `$8000`, then decompresses the room stream over its beginning. Several Ceres
        // rooms stop after BG1+BTS because they have no authored BG2 plane; the subsequent
        // native memcpy still copies a full layer from the untouched `$8000` tail. Requiring
        // compressed bytes for that tail rejected valid retail room $DF8D. Preserve any
        // partial authored words, but materialize every omitted word from the real fill.
        ushort[] background = new ushort[blockCount];
        Array.Fill(background, (ushort)0x8000);
        int availableBackgroundBytes = Math.Min(
            layerByteCount,
            levelStream.Length - backgroundOffset);
        if ((availableBackgroundBytes & 1) != 0)
        {
            throw new InvalidDataException(
                $"Room $8F:{header.Pointer:X4} ends midway through a BG2 level word.");
        }
        ushort[] authoredBackground = ReadWords(
            levelStream.AsSpan(backgroundOffset, availableBackgroundBytes));
        authoredBackground.CopyTo(background, 0);

        // The bank-$80 column producer performs the same full-height native overread from
        // custom_background. A 17-word suffix covers a row request but not a bottom-aligned
        // column: Ceres door $83:AB7C, for example, reaches index 556 in a 512-word plane.
        // Give the copied BG2 plane the same maximum room allocation so those defined WRAM
        // reads remain visible while genuinely impossible indices still fail loudly.
        ushort[] streamingBackground = new ushort[RoomLevelMemoryLayout.PrefilledStreamingWordCount];
        Array.Fill(streamingBackground, RoomLevelMemoryLayout.PrefilledLevelWord);
        background.CopyTo(streamingBackground, 0);
        return new RoomLevelData(
            widthInBlocks,
            allocationHeightInBlocks,
            foreground,
            behavior,
            background,
            blockDefinitions,
            streamingAllocation,
            header.DoorListPointer,
            streamingBackground);
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

    /// <summary>
    /// Reconstructs the word-addressable $7F level_data allocation after its native $8000
    /// clear and the room decompressor's byte-exact overwrite.
    /// </summary>
    private static ushort[] BuildPrefilledStreamingAllocation(ReadOnlySpan<byte> payload)
    {
        // Large rooms legitimately decompress BG1+BTS+BG2 beyond the region cleared by the
        // $6400-byte loop: `level_data` is merely the first view into the remainder of bank
        // $7F. Retain every decompressed byte and add a filled tail only when the payload is
        // shorter than the cleared region.
        int prefilledBytes = checked(RoomLevelMemoryLayout.PrefilledStreamingWordCount * 2);
        int allocationBytes = Math.Max(prefilledBytes, checked((payload.Length + 1) & ~1));

        var bytes = new byte[allocationBytes];
        for (int index = 0; index < bytes.Length; index += 2)
        {
            bytes[index] = unchecked((byte)RoomLevelMemoryLayout.PrefilledLevelWord);
            bytes[index + 1] = unchecked((byte)(RoomLevelMemoryLayout.PrefilledLevelWord >> 8));
        }
        payload.CopyTo(bytes);
        return ReadWords(bytes);
    }
}

/// <summary>One nine-byte bank-$8F tileset definition selected by graphics-set index.</summary>
public readonly record struct TilesetDefinition(
    ushort Pointer,
    int BlockDefinitionsAddress,
    int CharacterAddress,
    int PaletteAddress);
