using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

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
    private readonly ushort[] _levelEntries;
    private readonly ushort[] _backgroundEntries;
    private readonly byte[] _blockDefinitions;

    public BackgroundTilemapStreamer(
        int roomWidthInBlocks,
        ReadOnlySpan<ushort> levelEntries,
        ReadOnlySpan<ushort> backgroundEntries,
        ReadOnlySpan<byte> blockDefinitions,
        ushort sizeOfBg2 = 0)
    {
        if (roomWidthInBlocks is <= 0 or > 0xff)
            throw new ArgumentOutOfRangeException(nameof(roomWidthInBlocks));
        if ((blockDefinitions.Length & 7) != 0)
            throw new ArgumentException("Every block definition must contain four 16-bit tile words.", nameof(blockDefinitions));

        RoomWidthInBlocks = roomWidthInBlocks;
        SizeOfBg2 = sizeOfBg2;
        _levelEntries = levelEntries.ToArray();
        _backgroundEntries = backgroundEntries.ToArray();
        _blockDefinitions = blockDefinitions.ToArray();
    }

    public int RoomWidthInBlocks { get; }

    /// <summary>
    /// Native <c>$098E</c>. BG2 screen bases are formed by subtracting this room-specific
    /// allocation offset from the corresponding BG1 bases.
    /// </summary>
    public ushort SizeOfBg2 { get; }

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
        int ringX = blockX & 0x1f;
        int ringY = blockY & 0x0f;
        ushort screenBase = ringX < 0x10 ? (ushort)0x5000 : (ushort)0x53e0;
        ushort destination = unchecked((ushort)(
            screenBase + ringY * 0x40 + ringX * 2));

        // The native routine swaps the two 32x32 tilemap screens whenever the BG1 offset
        // crosses $0100. This preserves the ring identity after horizontal wrap.
        if ((bg1XOffset & 0x0100) != 0)
        {
            destination = ringX < 0x10
                ? unchecked((ushort)(destination + 0x0400))
                : unchecked((ushort)(destination - 0x0400));
        }

        ExpandedBlockTiles tiles = LevelBlockTilemapExpander.Expand(
            _levelEntries[blockIndex],
            _blockDefinitions);
        return new PlmTilemapUpdate(
            blockIndex,
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

    private TilemapStreamUpdate BuildColumn(BackgroundUpdateRequest request)
    {
        ReadOnlySpan<ushort> source = SelectSource(request.Layer);
        var leftTiles = new ushort[32];
        var rightTiles = new ushort[32];
        int sourceIndex = SourceIndex(request.SourceXBlock, request.SourceYBlock);

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
        int wrappedBytes = (4 * request.VramYBlock) & 0x003c;
        int unwrappedBytes = (wrappedBytes ^ 0x003f) + 1;
        int unwrappedWords = unwrappedBytes / 2;
        int wrappedWords = wrappedBytes / 2;

        int yWordOffset = (request.VramYBlock & 0x000f) * 0x0040;
        int xBlock = request.VramXBlock & 0x001f;
        int xWordOffset = xBlock * 2;
        ushort screenBase = xBlock >= 0x10 ? (ushort)0x53e0 : (ushort)0x5000;
        if (request.Layer == BackgroundLayer.Background)
            screenBase = unchecked((ushort)(screenBase - SizeOfBg2));

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

        return new TilemapStreamUpdate(request, leftTiles, rightTiles, segments);
    }

    private TilemapStreamUpdate BuildRow(BackgroundUpdateRequest request)
    {
        ReadOnlySpan<ushort> source = SelectSource(request.Layer);
        var topTiles = new ushort[34];
        var bottomTiles = new ushort[34];
        int sourceIndex = SourceIndex(request.SourceXBlock, request.SourceYBlock);

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

        int xWithinScreen = request.VramXBlock & 0x000f;
        int unwrappedBytes = 4 * (16 - xWithinScreen);
        int wrappedBytes = 4 * (xWithinScreen + 1);
        int unwrappedWords = unwrappedBytes / 2;
        int wrappedWords = wrappedBytes / 2;

        int yWordOffset = (request.VramYBlock & 0x000f) * 0x0040;
        int xBlock = request.VramXBlock & 0x001f;
        int xWordOffset = xBlock * 2;
        ushort wrappedScreenBase = 0x5400;
        ushort unwrappedScreenBase = 0x5000;
        if (xBlock >= 0x10)
        {
            wrappedScreenBase = 0x5000;
            unwrappedScreenBase = 0x53e0;
        }
        if (request.Layer == BackgroundLayer.Background)
        {
            wrappedScreenBase = unchecked((ushort)(wrappedScreenBase - SizeOfBg2));
            unwrappedScreenBase = unchecked((ushort)(unwrappedScreenBase - SizeOfBg2));
        }

        ushort unwrappedDestination = unchecked((ushort)(unwrappedScreenBase + yWordOffset + xWordOffset));
        ushort wrappedDestination = unchecked((ushort)(wrappedScreenBase + yWordOffset));
        var segments = new List<TilemapDmaSegment>(capacity: 4)
        {
            new(topTiles, 0, unwrappedWords, unwrappedDestination, TilemapDmaDirection.Row),
            new(bottomTiles, 0, unwrappedWords, (ushort)(unwrappedDestination | 0x0020), TilemapDmaDirection.Row),
            new(topTiles, unwrappedWords, wrappedWords, wrappedDestination, TilemapDmaDirection.Row),
            new(bottomTiles, unwrappedWords, wrappedWords, (ushort)(wrappedDestination | 0x0020), TilemapDmaDirection.Row),
        };
        return new TilemapStreamUpdate(request, topTiles, bottomTiles, segments);
    }

    private ReadOnlySpan<ushort> SelectSource(BackgroundLayer layer) => layer switch
    {
        BackgroundLayer.Level => _levelEntries,
        BackgroundLayer.Background => _backgroundEntries,
        _ => throw new ArgumentOutOfRangeException(nameof(layer)),
    };

    private int SourceIndex(ushort x, ushort y)
    {
        // $80:A9E5/$80:AB7F use the SNES 8x8 multiplier for Y*roomWidth, then add the
        // complete 16-bit X word. Valid gameplay requests stay inside the loaded room.
        return unchecked((byte)y * RoomWidthInBlocks + x);
    }

    private static ushort ReadSource(
        ReadOnlySpan<ushort> source,
        int index,
        BackgroundUpdateRequest request)
    {
        if ((uint)index >= (uint)source.Length)
        {
            throw new InvalidDataException(
                $"{request.Layer} stream request ({request.SourceXBlock:X4},{request.SourceYBlock:X4}) " +
                $"indexed entry {index}, outside the {source.Length}-entry room layer.");
        }
        return source[index];
    }
}

/// <summary>One visible 16x16 PLM block redraw in BG1's two-screen ring.</summary>
public sealed record PlmTilemapUpdate(
    int BlockIndex,
    ushort TopRowDestination,
    ushort[] TopRow,
    ushort[] BottomRow)
{
    /// <summary>Applies the two horizontal two-word transfers produced by bank $84.</summary>
    public void ExecuteTo(SnesVram vram)
    {
        ArgumentNullException.ThrowIfNull(vram);
        vram.ExecuteWordTransfer(TopRow, TopRowDestination, 1);
        vram.ExecuteWordTransfer(
            BottomRow,
            unchecked((ushort)(TopRowDestination + 0x20)),
            1);
    }
}

/// <summary>One completed WRAM staging buffer and its ordered NMI DMA operations.</summary>
public sealed record TilemapStreamUpdate(
    BackgroundUpdateRequest Request,
    ushort[] FirstHalves,
    ushort[] SecondHalves,
    IReadOnlyList<TilemapDmaSegment> Segments)
{
    /// <summary>
    /// Executes the same ordered transfers that <c>$80:8CD8/$80:8DAC</c> perform after
    /// observing the corresponding update flag during NMI.
    /// </summary>
    public void ExecuteTo(SnesVram vram)
    {
        ArgumentNullException.ThrowIfNull(vram);
        foreach (TilemapDmaSegment segment in Segments)
            segment.ExecuteTo(vram);
    }
}

/// <summary>
/// One DMA sourced from a slice of a staging array. Column segments use VMAIN=$81
/// (destination +32 words); row segments use VMAIN=$80 (destination +1 word).
/// </summary>
public sealed record TilemapDmaSegment(
    ushort[] SourceWords,
    int SourceWordOffset,
    int WordCount,
    ushort VramWordDestination,
    TilemapDmaDirection Direction)
{
    public void ExecuteTo(SnesVram vram)
    {
        ArgumentNullException.ThrowIfNull(vram);
        if (SourceWordOffset < 0 || WordCount < 0 || SourceWordOffset + WordCount > SourceWords.Length)
            throw new InvalidDataException("Tilemap DMA segment exceeds its staging array.");

        int increment = Direction == TilemapDmaDirection.Column ? 32 : 1;
        vram.ExecuteWordTransfer(
            SourceWords.AsSpan(SourceWordOffset, WordCount),
            VramWordDestination,
            increment);
    }
}

public enum TilemapDmaDirection
{
    Row,
    Column,
}
