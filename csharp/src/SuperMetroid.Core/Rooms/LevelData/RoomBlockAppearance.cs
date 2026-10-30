namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What one room block shows: a block of the room's tileset block set and how it is mirrored.</summary>
/// <param name="Block">The block shown, by its number in the room tileset's block set: the room block sheet's frame identity.</param>
/// <param name="Mirroring">How the block's picture is mirrored.</param>
public readonly record struct RoomBlockAppearance(int Block, BlockMirroring Mirroring)
{
    /// <summary>The highest block number a tileset's block set holds.</summary>
    public const int LastBlock = 1023;

    /// <summary>The block shown, 0 through <see cref="LastBlock"/>.</summary>
    public int Block
    {
        get;
        init => field = Ensure.BetweenInclusive(0, LastBlock, value);
    } = Ensure.BetweenInclusive(0, LastBlock, Block);

    /// <summary>How the block's picture is mirrored; only the two reflections are allowed.</summary>
    public BlockMirroring Mirroring
    {
        get;
        init => field = Validate(value);
    } = Validate(Mirroring);

    /// <summary>The appearance showing <paramref name="block"/> with <paramref name="mirroring"/>.</summary>
    /// <param name="block">The block's number in the tileset's block set, 0 through <see cref="LastBlock"/>.</param>
    /// <param name="mirroring">How the block is mirrored.</param>
    public static RoomBlockAppearance Of(int block, BlockMirroring mirroring = BlockMirroring.None) => new(block, mirroring);

    /// <summary>This appearance showing <paramref name="block"/> with the same mirroring.</summary>
    public RoomBlockAppearance WithBlock(int block) => this with { Block = block };

    /// <summary>This block mirrored by <paramref name="mirroring"/> instead.</summary>
    public RoomBlockAppearance WithMirroring(BlockMirroring mirroring) => this with { Mirroring = mirroring };

    /// <summary>Rejects mirroring outside the two reflections.</summary>
    private static BlockMirroring Validate(BlockMirroring mirroring) =>
        mirroring is BlockMirroring.None or BlockMirroring.Horizontal or BlockMirroring.Vertical or BlockMirroring.Both
            ? mirroring
            : throw new ArgumentOutOfRangeException(nameof(mirroring), mirroring, "A block mirrors only horizontally and vertically.");
}
