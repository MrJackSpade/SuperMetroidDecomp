using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Builds bank-$80 BG1/BG2 row and column staging buffers plus their exact NMI DMA plan.
/// </summary>
/// <remarks>
/// This ports <c>$80:A9D6-$80:AD17</c> and describes the later transfers at
/// <c>$80:8CD8-$80:8EA1</c>. Source arrays use decompressed level entries; block definitions
/// are the combined CRE-then-area table loaded at WRAM <c>$A000</c>. A produced update is
/// inert data until the runtime's NMI integration consumes its segments.
/// </remarks>
public sealed class BackgroundTilemapStreamer
{
    /// <summary>Retained BG1 level words, including the native streaming tail.</summary>
    private readonly ushort[] _levelEntries;
    /// <summary>Retained BG2 level words used when the requested layer is background.</summary>
    private readonly ushort[] _backgroundEntries;
    /// <summary>Four tile words per 16-pixel metatile, shared by camera and PLM expansion.</summary>
    private readonly byte[] _blockDefinitions;

    /// <summary>Copies BG1/BG2 source allocations and the combined metatile table used to produce camera-stream and PLM redraw descriptors; construction does not write VRAM.</summary>
    /// <param name="roomWidthInBlocks">Native row stride in 16-pixel blocks, 1..255, matching the eight-bit width used by bank-$80 multiplication.</param>
    /// <param name="levelEntries">BG1 level-word allocation, including any defined native streaming tail required by requests; later mutations must be mirrored through <see cref="SetLevelEntry"/>.</param>
    /// <param name="backgroundEntries">BG2 level-word allocation and its retained streaming tail, in the same row-major block geometry.</param>
    /// <param name="blockDefinitions">Combined CRE/area metatiles in little-endian order, eight bytes per four-character 16-by-16 block; later table mutations require <see cref="SetBlockDefinitionWord"/>.</param>
    /// <param name="sizeOfBg2">Native $098E VRAM-word allocation offset subtracted from BG1 ring bases to form BG2 destinations.</param>
    /// <exception cref="ArgumentOutOfRangeException">The room width is outside 1..255.</exception>
    /// <exception cref="ArgumentException">The metatile byte length is not divisible by eight.</exception>
    public BackgroundTilemapStreamer(
        int roomWidthInBlocks,
        ReadOnlySpan<ushort> levelEntries,
        ReadOnlySpan<ushort> backgroundEntries,
        ReadOnlySpan<byte> blockDefinitions,
        ushort sizeOfBg2 = 0)
    {
        if (roomWidthInBlocks is <= 0 or > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(roomWidthInBlocks));
        if ((blockDefinitions.Length & 7) != 0)
            throw new ArgumentException("Every block definition must contain four 16-bit tile words.", nameof(blockDefinitions));

        RoomWidthInBlocks = roomWidthInBlocks;
        Bg2TilemapOffsetWords = sizeOfBg2;
        _levelEntries = levelEntries.ToArray();
        _backgroundEntries = backgroundEntries.ToArray();
        _blockDefinitions = blockDefinitions.ToArray();
    }

    /// <summary>Row stride of retained BG1/BG2 level words, in 16-pixel blocks rather than 8-pixel characters or VRAM words.</summary>
    public int RoomWidthInBlocks { get; }

    /// <summary>
    /// How many VRAM words below each BG1 screen base the room's BG2 screen base lies: zero when the
    /// BG2 tilemap shares BG1's pages (the scrolling sky) and 2048 for every ordinary room. Every use
    /// subtracts it from a BG1 base and keeps the sixteen-bit wrap of that subtraction itself.
    /// </summary>
    /// <remarks>
    /// Stored as an ordinary count rather than the native word: the constructor accepts only a
    /// sixteen-bit value, nothing writes it afterwards, it is only ever subtracted inside an
    /// <c>unchecked((ushort)…)</c> destination calculation (identical for an <c>int</c> or a
    /// <c>ushort</c> operand), and no other name or projection reads it.
    /// </remarks>
    public int Bg2TilemapOffsetWords { get; }

    /// <summary>
    /// Mirrors a bank-$84 level-data mutation into this streamer's retained source copy.
    /// </summary>
    public void SetLevelEntry(int blockIndex, ushort levelWord)
    {
        if ((uint)blockIndex >= (uint)_levelEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        _levelEntries[blockIndex] = levelWord;
    }

    /// <summary>
    /// Mirrors a bank-$84 mutation of the shared 16x16 block-definition table into this
    /// streamer's retained copy.
    /// </summary>
    /// <remarks>
    /// <c>LoadItemPLMGfx</c> writes eight words into <c>TileTable</c> before its item draw
    /// list selects block <c>$08E-$095</c>. The room and camera streamer are deliberately
    /// backed by separate arrays, so updating only <see cref="RoomLevelData"/> leaves both
    /// immediate DrawPLM expansion and later camera-column streaming on the pre-load room
    /// definition. Keep the two WRAM views coherent at the mutation point instead of
    /// teaching individual items or rooms how to redraw themselves.
    /// </remarks>
    public void SetBlockDefinitionWord(int wordIndex, ushort tilemapWord)
    {
        int byteIndex = checked(wordIndex * 2);
        if ((uint)(byteIndex + 1) >= (uint)_blockDefinitions.Length)
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        _blockDefinitions[byteIndex] = unchecked((byte)tilemapWord);
        _blockDefinitions[byteIndex + 1] = SnesLittleEndian.HighByte(tilemapWord);
    }

    /// <summary>
    /// Expands one mutated BG1 block into the two two-word VRAM rows used by
    /// <c>DrawPLM</c> at <c>$84:8DBB</c>.
    /// </summary>
    /// <remarks>
    /// This is deliberately separate from the 16-block camera streamer. PLMs redraw only
    /// their authored blocks and address the same two-screen-wide BG1 ring. Bit eight of
    /// BG1's X offset swaps the logical left/right screen bases exactly as $84:8E7C does.
    /// Visibility clipping remains the PLM handler's responsibility.
    /// </remarks>
    public PlmTilemapUpdate BuildPlmLevelBlockUpdate(int blockIndex, ushort bg1XOffset)
    {
        if ((uint)blockIndex >= (uint)_levelEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));

        int blockX = blockIndex % RoomWidthInBlocks;
        int blockY = blockIndex / RoomWidthInBlocks;
        int ringX = blockX & Bg1TilemapLayout.RingColumnMask;
        int ringY = blockY & Bg1TilemapLayout.RingRowMask;
        ushort screenBase = ringX < Bg1TilemapLayout.ScreenBlocks ? Bg1TilemapLayout.FirstScreen : Bg1TilemapLayout.SecondScreenColumnBase;
        ushort destination = unchecked((ushort)(
            screenBase + ringY * Bg1TilemapLayout.BlockRowWords + ringX * 2));

        // The native routine swaps the two 32x32 tilemap screens whenever the BG1 offset
        // crosses $0100. This preserves the ring identity after horizontal wrap.
        if ((bg1XOffset & Bg1TilemapLayout.ScreenSwapScrollBit) != 0)
        {
            destination = ringX < Bg1TilemapLayout.ScreenBlocks
                ? unchecked((ushort)(destination + Bg1TilemapLayout.ScreenWords))
                : unchecked((ushort)(destination - Bg1TilemapLayout.ScreenWords));
        }

        ExpandedBlockTiles tiles = LevelBlockTilemapExpander.Expand(
            _levelEntries[blockIndex],
            _blockDefinitions);
        return new PlmTilemapUpdate(
            destination,
            new[] { tiles.TopLeft, tiles.TopRight },
            new[] { tiles.BottomLeft, tiles.BottomRight });
    }

    /// <summary>Produces one native staging-buffer update, or null for Mode 7 rooms.</summary>
    public TilemapStreamUpdate? Build(BackgroundUpdateRequest request, bool mode7Enabled = false)
    {
        if (mode7Enabled)
            return null;

        return request.Axis switch
        {
            BackgroundUpdateAxis.Column => BuildColumn(request),
            BackgroundUpdateAxis.Row => BuildRow(request),
            _ => throw new ArgumentOutOfRangeException(nameof(request)),
        };
    }

    /// <summary>Expands sixteen vertical blocks into left and right columns, splitting DMA at the VRAM ring wrap.</summary>
    private TilemapStreamUpdate BuildColumn(BackgroundUpdateRequest request)
    {
        ReadOnlySpan<ushort> source = SelectSource(request.Layer);
        var leftTiles = new ushort[32];
        var rightTiles = new ushort[32];
        int sourceIndex = SourceIndex(request.SourceBlock);

        // The viewport streamer always expands sixteen 16x16 blocks. Each contributes two
        // vertically adjacent tile words to each of the left/right column buffers.
        for (int block = 0; block < 16; block++)
        {
            ushort levelEntry = ReadSource(source, sourceIndex, request);
            ExpandedBlockTiles tiles = LevelBlockTilemapExpander.Expand(levelEntry, _blockDefinitions);
            int destination = block * 2;
            leftTiles[destination] = tiles.TopLeft;
            leftTiles[destination + 1] = tiles.BottomLeft;
            rightTiles[destination] = tiles.TopRight;
            rightTiles[destination + 1] = tiles.BottomRight;
            sourceIndex += RoomWidthInBlocks;
        }

        // VRAM is a two-screen-wide, one-screen-tall ring buffer. The current vertical
        // block modulo 16 determines where a 32-tile column wraps from bottom back to top.
        int wrappedBytes = (4 * request.VramBlock.Y) & (Bg1TilemapLayout.ColumnBytes - 4);
        int unwrappedBytes = Bg1TilemapLayout.ColumnBytes - wrappedBytes;
        int unwrappedWords = unwrappedBytes / 2;
        int wrappedWords = wrappedBytes / 2;

        int yWordOffset = (request.VramBlock.Y & Bg1TilemapLayout.RingRowMask) * Bg1TilemapLayout.BlockRowWords;
        int xBlock = request.VramBlock.X & Bg1TilemapLayout.RingColumnMask;
        int xWordOffset = xBlock * 2;
        ushort screenBase = xBlock >= Bg1TilemapLayout.ScreenBlocks ? Bg1TilemapLayout.SecondScreenColumnBase : Bg1TilemapLayout.FirstScreen;
        if (request.Layer == BackgroundLayer.Background)
            screenBase = unchecked((ushort)(screenBase - Bg2TilemapOffsetWords));

        ushort unwrappedDestination = unchecked((ushort)(screenBase + yWordOffset + xWordOffset));
        ushort wrappedDestination = unchecked((ushort)(screenBase + xWordOffset));
        var segments = new List<TilemapDmaSegment>(capacity: wrappedWords == 0 ? 2 : 4)
        {
            new(leftTiles, 0, unwrappedWords, unwrappedDestination, TilemapDmaDirection.Column),
            new(rightTiles, 0, unwrappedWords, unchecked((ushort)(unwrappedDestination + 1)), TilemapDmaDirection.Column),
        };
        if (wrappedWords != 0)
        {
            segments.Add(new(leftTiles, unwrappedWords, wrappedWords, wrappedDestination, TilemapDmaDirection.Column));
            segments.Add(new(rightTiles, unwrappedWords, wrappedWords, unchecked((ushort)(wrappedDestination + 1)), TilemapDmaDirection.Column));
        }

        return new TilemapStreamUpdate(segments);
    }

    /// <summary>Expands seventeen horizontal blocks into top and bottom rows, splitting DMA across BG screens.</summary>
    private TilemapStreamUpdate BuildRow(BackgroundUpdateRequest request)
    {
        ReadOnlySpan<ushort> source = SelectSource(request.Layer);
        var topTiles = new ushort[34];
        var bottomTiles = new ushort[34];
        int sourceIndex = SourceIndex(request.SourceBlock);

        // Seventeen blocks, rather than sixteen, cover both partial blocks at the screen's
        // horizontal edges. This is the literal loop count $0011 at $80:AC51.
        for (int block = 0; block < 17; block++)
        {
            ushort levelEntry = ReadSource(source, sourceIndex + block, request);
            ExpandedBlockTiles tiles = LevelBlockTilemapExpander.Expand(levelEntry, _blockDefinitions);
            int destination = block * 2;
            topTiles[destination] = tiles.TopLeft;
            topTiles[destination + 1] = tiles.TopRight;
            bottomTiles[destination] = tiles.BottomLeft;
            bottomTiles[destination + 1] = tiles.BottomRight;
        }

        int xWithinScreen = request.VramBlock.X & (Bg1TilemapLayout.ScreenBlocks - 1);
        int unwrappedBytes = 4 * (16 - xWithinScreen);
        int wrappedBytes = 4 * (xWithinScreen + 1);
        int unwrappedWords = unwrappedBytes / 2;
        int wrappedWords = wrappedBytes / 2;

        int yWordOffset = (request.VramBlock.Y & Bg1TilemapLayout.RingRowMask) * Bg1TilemapLayout.BlockRowWords;
        int xBlock = request.VramBlock.X & Bg1TilemapLayout.RingColumnMask;
        int xWordOffset = xBlock * 2;
        ushort wrappedScreenBase = Bg1TilemapLayout.SecondScreen;
        ushort unwrappedScreenBase = Bg1TilemapLayout.FirstScreen;
        if (xBlock >= Bg1TilemapLayout.ScreenBlocks)
        {
            wrappedScreenBase = Bg1TilemapLayout.FirstScreen;
            unwrappedScreenBase = Bg1TilemapLayout.SecondScreenColumnBase;
        }
        if (request.Layer == BackgroundLayer.Background)
        {
            wrappedScreenBase = unchecked((ushort)(wrappedScreenBase - Bg2TilemapOffsetWords));
            unwrappedScreenBase = unchecked((ushort)(unwrappedScreenBase - Bg2TilemapOffsetWords));
        }

        ushort unwrappedDestination = unchecked((ushort)(unwrappedScreenBase + yWordOffset + xWordOffset));
        ushort wrappedDestination = unchecked((ushort)(wrappedScreenBase + yWordOffset));
        var segments = new List<TilemapDmaSegment>(capacity: 4)
        {
            new(topTiles, 0, unwrappedWords, unwrappedDestination, TilemapDmaDirection.Row),
            new(bottomTiles, 0, unwrappedWords, (ushort)(unwrappedDestination | Bg1TilemapLayout.TileRowWords), TilemapDmaDirection.Row),
            new(topTiles, unwrappedWords, wrappedWords, wrappedDestination, TilemapDmaDirection.Row),
            new(bottomTiles, unwrappedWords, wrappedWords, (ushort)(wrappedDestination | Bg1TilemapLayout.TileRowWords), TilemapDmaDirection.Row),
        };
        return new TilemapStreamUpdate(segments);
    }

    /// <summary>Selects the retained BG1 or BG2 room words requested by the camera streamer.</summary>
    private ReadOnlySpan<ushort> SelectSource(BackgroundLayer layer) => layer switch
    {
        BackgroundLayer.Level => _levelEntries,
        BackgroundLayer.Background => _backgroundEntries,
        _ => throw new ArgumentOutOfRangeException(nameof(layer)),
    };

    /// <summary>Converts the source block to the native row-major index with eight-bit Y multiplication.</summary>
    private int SourceIndex(Position16<RoomBlocks> block)
    {
        // $80:A9E5/$80:AB7F use the SNES 8x8 multiplier for Y*roomWidth, then add the
        // complete 16-bit X word. Valid gameplay requests stay inside the loaded room.
        return unchecked((byte)block.Y * RoomWidthInBlocks + block.X);
    }

    /// <summary>Reads a retained layer word and reports an invalid request beyond the loaded allocation.</summary>
    private static ushort ReadSource(
        ReadOnlySpan<ushort> source,
        int index,
        BackgroundUpdateRequest request)
    {
        if ((uint)index >= (uint)source.Length)
        {
            throw new InvalidDataException(
                $"{request.Layer} stream request ({request.SourceBlock.X:X4},{request.SourceBlock.Y:X4}) " +
                $"indexed entry {index}, outside the {source.Length}-entry room layer.");
        }
        return source[index];
    }
}
