using SuperMetroid.Core.Assets;
using System.Buffers.Binary;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// The level, scroll grid and installed graphics a selected room state loads.
/// </summary>
/// <remarks>
/// This is the reusable, data-only portion of <c>LoadCRETilesTilesetTilesAndPalette</c> and
/// <c>LoadLevelDataAndOtherThings</c>. It deliberately does not execute arbitrary room ASM;
/// those bank-$8F setup/main functions remain separate behavior to translate per room.
/// </remarks>
[RoomLifetime]
public sealed class LoadedRoomAssets
{
    /// <summary>Retains the selected header, physical level allocation, scroll grid, and installed graphics.</summary>
    private LoadedRoomAssets(
        SelectedRoom header,
        RoomLevelData levelData,
        RoomScrollGrid scrolls,
        RoomTilesetArtwork tileset)
    {
        Header = header;
        LevelData = levelData;
        Scrolls = scrolls;
        CreCharacterCount = tileset.CreCharacterCount;
        TilesetCharacterCount = tileset.TilesetCharacterCount;
        TilesetColours = tileset.Colours;
        Tileset = tileset.Tileset;
        Pictures = tileset.Pictures;
    }

    /// <summary>Selected room header and room-state metadata used to choose level source, graphics set, dimensions, scrolls, and door table; loading these assets does not execute its room ASM.</summary>
    public SelectedRoom Header { get; }
    /// <summary>Runtime level allocation with compiled BG1/BG2 physical words, BTS, and separately installed visual references; retained allocation rows and streaming tails may exceed the visible header dimensions.</summary>
    public RoomLevelData LevelData { get; }
    /// <summary>Room's screen-indexed scroll-control grid used to initialize the boundary camera, separate from current camera position or editable map-marker artwork.</summary>
    public RoomScrollGrid Scrolls { get; }
    /// <summary>The installed common room elements' characters the room binds first; a Ceres tileset binds over them all.</summary>
    public int CreCharacterCount { get; }
    /// <summary>The installed tileset's characters the room binds over the common room elements' (#1422).</summary>
    public int TilesetCharacterCount { get; }
    /// <summary>The tileset's colours for each background set, which <see cref="LoadGraphics"/> gives the sets, leaving the sprite sets alone.</summary>
    public ShownBackgroundColourSets TilesetColours { get; }
    /// <summary>Compiled graphics-set source identities selected by the room-state index and used to resolve installed block pictures, characters, and palette artwork.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed pre-coloured room artwork: block sheets and library background images (#1392).</summary>
    public InstalledRoomArtwork Pictures { get; }

    /// <summary>The pre-coloured pictures of the tileset's blocks the room's planes draw from (#1392).</summary>
    public RoomBlockSheet BlockSheet => Pictures.Get(Tileset);

    /// <summary>Reads every compressed input named by the selected room and graphics set.</summary>
    public static LoadedRoomAssets Load(RoomScrollStorage scrollStorage, SelectedRoom header, RoomAssetCatalogs catalogs)
    {
        Ensure.NotNull(scrollStorage);
        Ensure.NotNull(header);
        Ensure.NotNull(catalogs);
        RoomCharacterAtlases characterArt = catalogs.Characters;
        RoomStaticPalettes paletteArt = catalogs.Palettes;
        RoomLevelArtworks visualLayouts = catalogs.VisualLayouts;
        TilesetDefinition tileset = RoomTilesets.Get(header.State.GraphicsSet);

        // Installed visual layouts imply a retail room source. Its collision/BTS and
        // native overread allocation come from the stock-only installed level streams;
        // only the low visual tile bits come from the separately replaceable layout catalog.
        byte[] levelStream = catalogs.LevelStreams.Get(header.State.Level).ToArray();
        RoomLevelData levelData = ParseLevelData(header, levelStream, visualLayouts.Get(header.State.Level));
        RoomScrollGrid scrolls = LoadScrolls(scrollStorage, header);
        return new LoadedRoomAssets(
            header,
            levelData,
            scrolls,
            new RoomTilesetArtwork(tileset, BgCharacterSet.Tiles.CharactersIn(characterArt.Cre.Characters.Length),
                BgCharacterSet.Tiles.CharactersIn(characterArt.Get(tileset.Characters).Characters.Length),
                paletteArt.Get(tileset.Palette).Sets, catalogs.BlockSheets));
    }

    /// <summary>
    /// The perspective floor a Ceres room shows (#1349): the 128 by 128 tile numbers its tileset's installed floor map
    /// starts from, which extraction imports with the tileset's floor tiles (#1422).
    /// </summary>
    /// <exception cref="InvalidDataException">No floor tiles are installed for the room's tileset.</exception>
    public FloorMap CreateFloorMap()
    {
        FloorTileSheet tiles = Pictures.FloorTilesOf(Tileset);
        return new FloorMap(tiles.Map, Tileset, tiles);
    }

    /// <summary>
    /// The two character sets a room of <paramref name="tileset"/> binds as it loads, in order: the common room elements,
    /// then the tileset's characters over them (#1350).
    /// </summary>
    /// <param name="tileset">The tileset the room loads.</param>
    /// <param name="commonRoomElementCharacters">The common room elements' characters.</param>
    /// <param name="tilesetCharacters">The tileset's characters.</param>
    public static IReadOnlyList<BgCharacterPlacement> CharacterPlacements(TilesetDefinition tileset, int commonRoomElementCharacters,
        int tilesetCharacters) =>
    [
        // Preserve $82:E783's transfer order, not merely its two destinations. Ordinary
        // area art ends before the common room elements' characters, but Ceres tileset $11 holds all
        // 1024 tile characters. The tileset must therefore bind over the common room elements;
        // reversing these produces the vertical-stripe corruption seen when CRE replaces Ceres tiles.
        new(RoomBgArtwork.CommonRoomElements, BgCharacterSet.Tiles, RoomTileCharacters.CommonRoomElementsFirst, commonRoomElementCharacters),
        new(RoomBgArtwork.Tileset(tileset.Characters), BgCharacterSet.Tiles, RoomTileCharacters.TilesetFirst, tilesetCharacters),
    ];

    /// <summary>Binds the room's two character regions (#1350) and gives every background set the tileset's colours for it.</summary>
    public void LoadGraphics(BgArtworkBindings backgrounds, BackgroundColourSets colours)
    {
        Ensure.NotNull(backgrounds);
        Ensure.NotNull(colours);

        foreach (BgCharacterPlacement placement in CharacterPlacements(Tileset, CreCharacterCount, TilesetCharacterCount))
            placement.BindTo(backgrounds);
        TilesetColours.CopyTo(colours);
    }

    /// <summary>Parses BG1, BTS, and BG2 while retaining native allocation tails beyond visible room screens.</summary>
    private static RoomLevelData ParseLevelData(
        SelectedRoom header,
        byte[] levelStream,
        RoomLevelArtwork? visualLayout)
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
        return new RoomLevelData(new Extent16<RoomBlocks>(unchecked((ushort)widthInBlocks), unchecked((ushort)allocationHeightInBlocks)), new RoomLevelPlanes(foreground, behavior, background), new RoomLevelStreaming(streamingAllocation, streamingBackground), doors: header.Doors, visualLayout: visualLayout);
    }

    /// <summary>Creates the room's screen-indexed scroll grid from its selected state.</summary>
    private static RoomScrollGrid LoadScrolls(RoomScrollStorage scrollStorage, SelectedRoom header)
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
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index), RoomLevelMemoryLayout.PrefilledLevelWord);
        payload.CopyTo(bytes);
        return ReadWords(bytes);
    }
}
