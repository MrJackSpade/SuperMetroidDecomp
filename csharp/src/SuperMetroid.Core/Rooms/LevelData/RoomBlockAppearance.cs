namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What one room block shows (#2100): a block of the room's tileset block set and how it is mirrored; nothing, the open
/// space level data leaves and a block Samus breaks becomes; or, for the solid terrain beyond the room's grid, no picture
/// at all, since the room never shows it. A block is bounded by its tileset's installed block set, which the room's block
/// sheet checks as it pictures the block. The default shows the block set's first block, unmirrored.
/// </summary>
public readonly record struct RoomBlockAppearance
{
    /// <summary>The block shown, by its number in the room tileset's block set; zero where no block is shown.</summary>
    private readonly int block;

    /// <summary>What kind of thing the appearance shows.</summary>
    private readonly Shown shown;

    /// <summary>Keeps what is shown and how it is mirrored.</summary>
    private RoomBlockAppearance(int block, Shown shown, BlockMirroring mirroring)
    {
        this.block = block;
        this.shown = shown;
        Mirroring = mirroring;
    }

    /// <summary>The kinds of thing a room block shows.</summary>
    private enum Shown
    {
        /// <summary>A block of the tileset's block set.</summary>
        Block,
        /// <summary>Nothing: open space.</summary>
        Nothing,
        /// <summary>No picture: terrain beyond the room's grid, which the room never shows.</summary>
        BeyondRoom,
    }

    /// <summary>A block that shows nothing, unmirrored: open space, as level data leaves it and a block Samus breaks becomes.</summary>
    public static RoomBlockAppearance Nothing { get; } = new(0, Shown.Nothing, BlockMirroring.None);

    /// <summary>
    /// What the solid terrain beyond the room's grid shows (<see cref="RoomTerrainBlocks.EdgeSolid"/>): no picture, as the
    /// room never shows it. It is not open space: it does not show nothing.
    /// </summary>
    public static RoomBlockAppearance BeyondRoom { get; } = new(0, Shown.BeyondRoom, BlockMirroring.None);

    /// <summary>Whether the block shows nothing: open space, however it is mirrored (#1610).</summary>
    public bool ShowsNothing => shown == Shown.Nothing;

    /// <summary>Whether the block shows a block of the tileset's block set, whose number <see cref="Block"/> gives.</summary>
    public bool ShowsBlock => shown == Shown.Block;

    /// <summary>Whether the block is terrain beyond the room's grid, which has no picture (<see cref="BeyondRoom"/>).</summary>
    public bool IsBeyondRoom => shown == Shown.BeyondRoom;

    /// <summary>The block shown, by its number in the room tileset's block set: the room block sheet's frame identity.</summary>
    /// <exception cref="InvalidOperationException">The block shows nothing, or is terrain beyond the room.</exception>
    public int Block => shown == Shown.Block ? block
        : throw new InvalidOperationException($"The appearance shows {this}, not a block of the tileset.");

    /// <summary>How the block's picture is mirrored; only the two reflections are allowed.</summary>
    public BlockMirroring Mirroring
    {
        get;
        init => field = Validate(value);
    }

    /// <summary>The appearance showing <paramref name="block"/> with <paramref name="mirroring"/>.</summary>
    /// <param name="block">The block's number in the tileset's block set, zero or more; the tileset's sheet bounds it.</param>
    /// <param name="mirroring">How the block is mirrored.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block is negative, or the mirroring is not the two reflections.</exception>
    public static RoomBlockAppearance Of(int block, BlockMirroring mirroring = BlockMirroring.None) =>
        new(Ensure.AtLeastZero(block), Shown.Block, Validate(mirroring));

    /// <summary>This block mirrored by <paramref name="mirroring"/> instead.</summary>
    public RoomBlockAppearance WithMirroring(BlockMirroring mirroring) => this with { Mirroring = mirroring };

    /// <summary>What the block shows, for diagnostics.</summary>
    public override string ToString()
    {
        string what = shown switch
        {
            Shown.Block => $"block {block}",
            Shown.Nothing => "nothing",
            Shown.BeyondRoom => "terrain beyond the room",
            _ => throw new InvalidOperationException($"Undefined shown kind {shown}."),
        };
        return Mirroring == BlockMirroring.None ? what : $"{what} mirrored {Mirroring}";
    }

    /// <summary>Rejects mirroring outside the two reflections.</summary>
    private static BlockMirroring Validate(BlockMirroring mirroring) =>
        mirroring is BlockMirroring.None or BlockMirroring.Horizontal or BlockMirroring.Vertical or BlockMirroring.Both
            ? mirroring
            : throw new ArgumentOutOfRangeException(nameof(mirroring), mirroring, "A block mirrors only horizontally and vertically.");
}
