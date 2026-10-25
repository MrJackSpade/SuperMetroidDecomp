using SuperMetroid.Core.Assets;
using System.Buffers.Binary;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// Decompressed visual/collision assets selected by one cartridge room-state header.
/// </summary>
/// <remarks>
/// This is the reusable, data-only portion of <c>LoadCRETilesTilesetTilesAndPalette</c> and
/// <c>LoadLevelDataAndOtherThings</c>. It deliberately does not execute arbitrary room ASM;
/// those bank-$8F setup/main functions remain separate behavior to translate per room.
/// </remarks>
[RoomLifetime]
public sealed class CartridgeRoomAssets
{
    /// <summary>Retains the selected header, physical level allocation, scroll grid, and installed graphics.</summary>
    private CartridgeRoomAssets(
        CartridgeRoomHeader header,
        RoomLevelData levelData,
        RoomScrollGrid scrolls,
        RoomTilesetArtwork tileset)
    {
        Header = header;
        LevelData = levelData;
        Scrolls = scrolls;
        CreCharacters = tileset.CreCharacters;
        RoomCharacters = tileset.RoomCharacters;
        PaletteBytes = tileset.PaletteBytes;
        Tileset = tileset.Tileset;
        Pictures = tileset.Pictures;
    }

    /// <summary>Selected room header and room-state metadata used to choose level source, graphics set, dimensions, scrolls, and door table; loading these assets does not execute its room ASM.</summary>
    public CartridgeRoomHeader Header { get; }
    /// <summary>Runtime level allocation with compiled BG1/BG2 physical words, BTS, metatiles, and separately installed visual references; retained allocation rows and streaming tails may exceed the visible header dimensions.</summary>
    public RoomLevelData LevelData { get; }
    /// <summary>Room's screen-indexed scroll-control grid used to initialize the boundary camera, separate from current camera position or editable map-marker artwork.</summary>
    public RoomScrollGrid Scrolls { get; }
    /// <summary>Installed common-room-element planar BG character bytes, uploaded first at VRAM byte $5000; Ceres's later full-size area upload intentionally overwrites this region.</summary>
    public byte[] CreCharacters { get; }
    /// <summary>Installed graphics-set planar BG character bytes uploaded after CRE at VRAM byte $0000; the Ceres tileset spans the full $8000-byte BG character allocation.</summary>
    public byte[] RoomCharacters { get; }
    /// <summary>Installed graphics-set little-endian BGR555 palette bytes; <see cref="LoadGraphics"/> copies the first $0100 bytes into CGRAM colors 0..127, preserving OBJ palettes.</summary>
    public byte[] PaletteBytes { get; }
    /// <summary>Compiled graphics-set source identities selected by the room-state index and used to resolve installed metatiles, characters, and palette artwork.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed pre-coloured room artwork: block sheets and library background images (#1392).</summary>
    public RoomBlockSheetCatalog Pictures { get; }

    /// <summary>The pre-coloured pictures of the tileset's blocks the room's planes draw from (#1392).</summary>
    public RoomBlockSheet BlockSheet => Pictures.Get(Tileset);

    /// <summary>
    /// The block definitions a room of <paramref name="tileset"/> loads: the common room elements' and then the tileset's,
    /// or a Ceres room's tileset alone.
    /// </summary>
    /// <exception cref="InvalidDataException">The definitions are not whole blocks.</exception>
    public static byte[] BlockDefinitions(RoomMetatileCatalog metatiles, TilesetDefinition tileset, bool ceres)
    {
        Ensure.NotNull(metatiles);
        byte[] roomBlockDefinitions = metatiles.Get(tileset.Blocks).BlockDefinitions.ToArray();
        byte[] blockDefinitions;
        if (ceres)
        {
            // $82:E7D3 installs a Ceres tileset directly at WRAM $A000. Unlike Zebes
            // rooms, area six does not reserve the first $800 bytes for common CRE blocks.
            blockDefinitions = roomBlockDefinitions;
        }
        else
        {
            byte[] creDefinitions = metatiles.Cre.BlockDefinitions.ToArray();
            blockDefinitions = new byte[creDefinitions.Length + roomBlockDefinitions.Length];
            creDefinitions.CopyTo(blockDefinitions, 0);
            roomBlockDefinitions.CopyTo(blockDefinitions, creDefinitions.Length);
        }

        if (blockDefinitions.Length == 0 || (blockDefinitions.Length & 7) != 0)
        {
            throw new InvalidDataException(
                $"Tileset {tileset} produced ${blockDefinitions.Length:X} bytes of block definitions; native blocks are eight bytes each.");
        }
        return blockDefinitions;
    }

    /// <summary>Reads every compressed input named by the selected room and graphics set.</summary>
    public static CartridgeRoomAssets Load(RoomScrollStorage scrollStorage, CartridgeRoomHeader header, RoomAssetCatalogs catalogs)
    {
        Ensure.NotNull(scrollStorage);
        Ensure.NotNull(header);
        Ensure.NotNull(catalogs);
        RoomCharacterAtlasCatalog characterArt = catalogs.Characters;
        RoomStaticPaletteCatalog paletteArt = catalogs.Palettes;
        RoomMetatileCatalog metatileArt = catalogs.Metatiles;
        RoomVisualLayoutCatalog visualLayouts = catalogs.VisualLayouts;
        TilesetDefinition tileset = RoomTilesetDefinitions.Get(header.State.GraphicsSet);

        byte[] blockDefinitions = BlockDefinitions(metatileArt, tileset, header.AreaIndex == AreaId.Ceres);

        // Installed visual layouts imply a retail room source. Its collision/BTS and
        // native overread allocation come from the stock-only installed level streams;
        // only the low visual tile bits come from the separately replaceable layout catalog.
        byte[] levelStream = catalogs.LevelStreams.Get(
            header.State.CompressedLevelDataAddress).ToArray();
        RoomLevelData levelData = ParseLevelData(header, levelStream, blockDefinitions,
            visualLayouts.Get(header.State.CompressedLevelDataAddress));
        RoomScrollGrid scrolls = LoadScrolls(scrollStorage, header);
        return new CartridgeRoomAssets(
            header,
            levelData,
            scrolls,
            new RoomTilesetArtwork(tileset, characterArt.Cre.Characters.ToArray(), characterArt.Get(tileset.Characters).Characters.ToArray(),
                paletteArt.Get(tileset.Palette).PackedColours.ToArray(), catalogs.BlockSheets));
    }

    /// <summary>
    /// The Mode 7 map a Ceres room shows (#1349): Mode 7 reads its 128 by 128 tile numbers from the low byte of each of
    /// the first $4000 VRAM words, which the room's two character uploads fill, the area tileset over the CRE.
    /// </summary>
    [PpuLayoutBoundary("codec: the Mode 7 map shares VRAM words with the room tileset's character upload")]
    public FloorMap CreateMode7Map()
    {
        var words = new byte[FloorMap.Cells * 2];
        CopyAt(CreCharacters, RoomTileCharacters.CommonRoomElementsFirst * BgCharacterSet.Tiles.CharacterBytes());
        CopyAt(RoomCharacters, RoomTileCharacters.TilesetFirst * BgCharacterSet.Tiles.CharacterBytes());
        var cells = new byte[FloorMap.Cells];
        for (int cell = 0; cell < cells.Length; cell++)
            cells[cell] = words[cell * 2];
        return new FloorMap(cells, Tileset, Pictures.Mode7(Tileset));

        void CopyAt(byte[] source, int byteOffset)
        {
            int length = Math.Min(source.Length, words.Length - byteOffset);
            source.AsSpan(0, length).CopyTo(words.AsSpan(byteOffset));
        }
    }

    /// <summary>
    /// The two character sets a room of <paramref name="tileset"/> binds as it loads, in order: the common room elements,
    /// then the tileset's characters over them (#1350).
    /// </summary>
    public static IReadOnlyList<BgCharacterPlacement> CharacterPlacements(TilesetDefinition tileset, ReadOnlyMemory<byte> creCharacters,
        ReadOnlyMemory<byte> roomCharacters) =>
    [
        // Preserve $82:E783's transfer order, not merely its two destinations. Ordinary
        // area art ends before the common room elements' characters, but Ceres tileset $11 holds all
        // 1024 tile characters. The tileset must therefore bind over the common room elements;
        // reversing these produces the vertical-stripe corruption seen when CRE replaces Ceres tiles.
        new(RoomBgArtwork.CommonRoomElements, BgCharacterSet.Tiles, RoomTileCharacters.CommonRoomElementsFirst, creCharacters),
        new(RoomBgArtwork.Tileset(tileset.Characters), BgCharacterSet.Tiles, RoomTileCharacters.TilesetFirst, roomCharacters),
    ];

    /// <summary>Binds the room's two character regions (#1350) and installs its first 128 CGRAM colors.</summary>
    public void LoadGraphics(BgArtworkBindings backgrounds, BackgroundColourSets colours)
    {
        Ensure.NotNull(backgrounds);
        Ensure.NotNull(colours);

        foreach (BgCharacterPlacement placement in CharacterPlacements(Tileset, CreCharacters, RoomCharacters))
            placement.BindTo(backgrounds);
        if (PaletteBytes.Length < RoomStaticPaletteFormat.ByteCount)
        {
            throw new InvalidDataException(
                $"Tileset palette expanded to ${PaletteBytes.Length:X} bytes; expected at least $100.");
        }
        ColourRun room = colours.All;
        for (int colour = 0; colour < room.Count; colour++)
            room.Write(colour, Bgr555.FromCgramPortWord(BinaryPrimitives.ReadUInt16LittleEndian(PaletteBytes.AsSpan(colour * sizeof(ushort)))));
    }

    /// <summary>Parses BG1, BTS, and BG2 while retaining native allocation tails beyond visible room screens.</summary>
    private static RoomLevelData ParseLevelData(
        CartridgeRoomHeader header,
        byte[] levelStream,
        byte[] blockDefinitions,
        RoomVisualLayout? visualLayout)
    {
        Extent16<RoomBlocks> visibleSize = header.Size.ToRoomBlocks();
        int widthInBlocks = visibleSize.X;
        int visibleBlockCount = visibleSize.Area;
        if (levelStream.Length < 2)
            throw new InvalidDataException("Compressed room level stream has no size word.");

        int declaredLayerBytes = BinaryPrimitives.ReadUInt16LittleEndian(levelStream);
        if ((declaredLayerBytes & 1) != 0)
        {
            throw new InvalidDataException(
                $"Room {header.Definition.Name} declares an odd ${declaredLayerBytes:X} BG1 byte count.");
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
                $"Room {header.Definition.Name} declares ${declaredLayerBytes:X} BG1 bytes " +
                $"({blockCount} blocks), which cannot contain its {header.Size.X}x" +
                $"{header.Size.Y} visible screens in complete {widthInBlocks}-block rows.");
        }
        int allocationHeightInBlocks = blockCount / widthInBlocks;
        int layerByteCount = declaredLayerBytes;

        int foregroundOffset = 2;
        int behaviorOffset = foregroundOffset + layerByteCount;
        int backgroundOffset = behaviorOffset + blockCount;
        if (levelStream.Length < backgroundOffset)
        {
            throw new InvalidDataException(
                $"Room {header.Definition.Name} level stream is missing its complete BG1/BTS allocation.");
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
        Array.Fill(background, RoomLevelMemoryLayout.PrefilledLevelWord);
        int availableBackgroundBytes = Math.Min(
            layerByteCount,
            levelStream.Length - backgroundOffset);
        if ((availableBackgroundBytes & 1) != 0)
        {
            throw new InvalidDataException(
                $"Room {header.Definition.Name} ends midway through a BG2 level word.");
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
        return new RoomLevelData(new Extent16<RoomBlocks>(unchecked((ushort)widthInBlocks), unchecked((ushort)allocationHeightInBlocks)), new RoomLevelPlanes(foreground, behavior, background), blockDefinitions, new RoomLevelStreaming(streamingAllocation, streamingBackground), doors: header.Doors, visualLayout: visualLayout);
    }

    /// <summary>Creates the room's screen-indexed scroll grid from its selected state.</summary>
    private static RoomScrollGrid LoadScrolls(RoomScrollStorage scrollStorage, CartridgeRoomHeader header)
        => RoomScrollGrid.FromDefinition(
            scrollStorage,
            header.State.Scroll,
            header.Size);

    /// <summary>Decodes little-endian level words from a byte-aligned room stream span.</summary>
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
            bytes[index + 1] = SnesLittleEndian.HighByte(RoomLevelMemoryLayout.PrefilledLevelWord);
        }
        payload.CopyTo(bytes);
        return ReadWords(bytes);
    }
}
