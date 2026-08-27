namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Decompressed native room allocation: BG1 level words, one-byte BTS metadata, BG2 level
/// words, and the combined CRE/area visual block definitions.
/// </summary>
/// <remarks>
/// Bank $82 installs these planes together, but their consumers are different: bank $80
/// expands the low visual bits for VRAM streaming while bank $94 reads BG1's high nibble
/// and the parallel BTS byte for collision. Keeping one object prevents rendering and
/// physics from quietly loading different room revisions or indexing conventions.
/// </remarks>
public sealed class RoomLevelData
{
    private readonly ushort[] _foregroundEntries;
    private readonly byte[] _behaviorBytes;
    private readonly ushort[] _backgroundEntries;
    private readonly byte[] _blockDefinitions;
    private readonly ushort[] _streamingForegroundAllocation;

    public RoomLevelData(
        int widthInBlocks,
        int heightInBlocks,
        ReadOnlySpan<ushort> foregroundEntries,
        ReadOnlySpan<byte> behaviorBytes,
        ReadOnlySpan<ushort> backgroundEntries,
        ReadOnlySpan<byte> blockDefinitions,
        ReadOnlySpan<ushort> streamingForegroundAllocation = default)
    {
        if (widthInBlocks is <= 0 or > 0xff)
            throw new ArgumentOutOfRangeException(nameof(widthInBlocks));
        if (heightInBlocks <= 0)
            throw new ArgumentOutOfRangeException(nameof(heightInBlocks));

        int expectedBlocks = checked(widthInBlocks * heightInBlocks);
        if (foregroundEntries.Length != expectedBlocks)
            throw new ArgumentException("BG1 must contain exactly one word per room block.", nameof(foregroundEntries));
        if (behaviorBytes.Length != expectedBlocks)
            throw new ArgumentException("BTS must contain exactly one byte per room block.", nameof(behaviorBytes));
        if (backgroundEntries.Length != expectedBlocks)
            throw new ArgumentException("BG2 must contain exactly one word per room block.", nameof(backgroundEntries));
        if ((blockDefinitions.Length & 7) != 0)
            throw new ArgumentException("Each visual block definition is exactly eight bytes.", nameof(blockDefinitions));

        WidthInBlocks = widthInBlocks;
        HeightInBlocks = heightInBlocks;
        _foregroundEntries = foregroundEntries.ToArray();
        _behaviorBytes = behaviorBytes.ToArray();
        _backgroundEntries = backgroundEntries.ToArray();
        _blockDefinitions = blockDefinitions.ToArray();

        // The cartridge's initial 17-column fill can read past logical BG1 into adjacent
        // decompressed planes at the room's bottom edge. LandingSiteStreamingData supplies
        // that complete native allocation. Synthetic/general callers default to logical
        // BG1 only, which remains sufficient for in-bounds streaming and all collision.
        _streamingForegroundAllocation = streamingForegroundAllocation.IsEmpty
            ? _foregroundEntries.ToArray()
            : streamingForegroundAllocation.ToArray();
        if (_streamingForegroundAllocation.Length < expectedBlocks)
        {
            throw new ArgumentException(
                "The streaming BG1 allocation cannot be shorter than logical BG1.",
                nameof(streamingForegroundAllocation));
        }
    }

    /// <summary>Native <c>room_width_in_blocks</c>, used as every row's index stride.</summary>
    public int WidthInBlocks { get; }

    /// <summary>Logical room height from the room header, measured in 16-pixel blocks.</summary>
    public int HeightInBlocks { get; }

    /// <summary>Read-only logical BG1 words, including collision type in bits 12–15.</summary>
    public ReadOnlyMemory<ushort> ForegroundEntries => _foregroundEntries;

    /// <summary>Read-only parallel BTS (“block type special”) byte plane.</summary>
    public ReadOnlyMemory<byte> BehaviorBytes => _behaviorBytes;

    /// <summary>Read-only logical BG2 words.</summary>
    public ReadOnlyMemory<ushort> BackgroundEntries => _backgroundEntries;

    /// <summary>CRE definitions followed by area definitions, eight bytes per visual block.</summary>
    public ReadOnlyMemory<byte> BlockDefinitions => _blockDefinitions;

    /// <summary>
    /// Converts a 16-pixel block coordinate using the same row-major multiplication as
    /// bank $94. Invalid coordinates fail loudly instead of wrapping into unrelated WRAM.
    /// </summary>
    public int GetBlockIndex(int blockX, int blockY)
    {
        if ((uint)blockX >= (uint)WidthInBlocks)
            throw new ArgumentOutOfRangeException(nameof(blockX));
        if ((uint)blockY >= (uint)HeightInBlocks)
            throw new ArgumentOutOfRangeException(nameof(blockY));
        return blockY * WidthInBlocks + blockX;
    }

    /// <summary>Returns both collision inputs consumed for one block by bank $94.</summary>
    public RoomCollisionBlock GetCollisionBlock(int blockX, int blockY)
    {
        int index = GetBlockIndex(blockX, blockY);
        return GetCollisionBlockByIndex(index);
    }

    /// <summary>
    /// Reads a native row-major index after bank-$94 type-$5/$D extension arithmetic.
    /// </summary>
    public RoomCollisionBlock GetCollisionBlockByIndex(int blockIndex)
    {
        if ((uint)blockIndex >= (uint)_foregroundEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return new RoomCollisionBlock(
            blockIndex,
            _foregroundEntries[blockIndex],
            _behaviorBytes[blockIndex]);
    }

    /// <summary>
    /// Converts unsigned room-world pixels to a block coordinate exactly as the native
    /// <c>LSR ×4</c>/<c>&gt;&gt; 4</c> collision paths do.
    /// </summary>
    public RoomCollisionBlock GetCollisionBlockAtPixel(ushort xPosition, ushort yPosition) =>
        GetCollisionBlock(xPosition >> 4, yPosition >> 4);

    /// <summary>
    /// Applies bank-$84 bomb-block setup's immediate <c>level_data &amp;= $0FFF</c> write.
    /// </summary>
    /// <remarks>
    /// Collision-triggered bomb blocks retain their low twelve visual/flip bits while the
    /// high dispatcher nibble becomes air. The later PLM animation is a separate producer;
    /// this method models only the synchronous mutation that bank $94 observes during the
    /// same movement scan. The native streaming allocation aliases logical BG1, so keep our
    /// retained streaming copy coherent as well.
    /// </remarks>
    public void ClearCollisionType(int blockIndex)
    {
        if ((uint)blockIndex >= (uint)_foregroundEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));

        ushort airWord = unchecked((ushort)(_foregroundEntries[blockIndex] & 0x0fff));
        _foregroundEntries[blockIndex] = airWord;
        if (blockIndex < _streamingForegroundAllocation.Length)
            _streamingForegroundAllocation[blockIndex] = airWord;
    }

    /// <summary>
    /// Replaces one complete native <c>level_data</c> word and keeps the bank-$80
    /// streaming allocation coherent with the collision plane.
    /// </summary>
    /// <remarks>
    /// Bank-$84 PLM draw instructions write the full word, including the collision nibble,
    /// visual block number, and parent flip bits. A narrower "change the graphic" helper
    /// would be incorrect here: breakable grapple frames intentionally alternate between
    /// type-$E grapple terrain and type-$0 air.
    /// </remarks>
    public void SetForegroundEntry(int blockIndex, ushort levelWord)
    {
        if ((uint)blockIndex >= (uint)_foregroundEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));

        _foregroundEntries[blockIndex] = levelWord;
        if (blockIndex < _streamingForegroundAllocation.Length)
            _streamingForegroundAllocation[blockIndex] = levelWord;
    }

    /// <summary>
    /// Replaces the low BTS byte paired with one level-data block.
    /// </summary>
    /// <remarks>
    /// The cartridge stores BTS bytes in a word-addressable allocation and preserves the
    /// neighboring high byte with <c>AND #$FF00</c>. This model already exposes each logical
    /// low byte separately, so assigning this element is the equivalent operation.
    /// </remarks>
    public void SetBehavior(int blockIndex, byte behavior)
    {
        if ((uint)blockIndex >= (uint)_behaviorBytes.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        _behaviorBytes[blockIndex] = behavior;
    }

    /// <summary>
    /// Constructs bank $80's visual row/column producer over the exact same decompressed
    /// room allocation later consumed by collision.
    /// </summary>
    public BackgroundTilemapStreamer CreateBackgroundStreamer(ushort sizeOfBg2 = 0) =>
        new(
            WidthInBlocks,
            _streamingForegroundAllocation,
            _backgroundEntries,
            _blockDefinitions,
            sizeOfBg2);
}

/// <summary>One indexed pair from native <c>level_data</c> and parallel <c>BTS</c>.</summary>
public readonly record struct RoomCollisionBlock(int Index, ushort LevelWord, byte Behavior)
{
    /// <summary>
    /// Lossless typed interpretation of <see cref="LevelWord"/>. The original property is
    /// retained because ROM fixtures and diagnostic output intentionally expose raw words.
    /// </summary>
    public RoomLevelWord PackedWord => new(LevelWord);

    /// <summary>
    /// High-nibble dispatcher index used by <c>$94:9515</c>. Values are native categories,
    /// not a simplified solid/air Boolean: 1 is slope, 8/C/E are solid-family, etc.
    /// </summary>
    public byte CollisionType => PackedWord.CollisionTypeValue;

    /// <summary>
    /// Typed collision-dispatch view for code paths whose native handler has a verified
    /// name. Unnamed enum values remain representable and retain their original nibble.
    /// </summary>
    public RoomCollisionType CollisionKind => PackedWord.CollisionType;

    /// <summary>Low ten bits selecting the visual 16×16 block definition.</summary>
    public ushort VisualBlockIndex => PackedWord.VisualBlockIndex;

    /// <summary>Parent block horizontal/vertical flip flags in bits 10 and 11.</summary>
    public LevelBlockFlipFlags VisualFlipFlags => PackedWord.VisualFlipFlags;
}
