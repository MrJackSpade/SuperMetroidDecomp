using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One room background plane as the room shows it: a grid of 16x16 blocks, each naming the block it shows and its
/// mirroring, or the collectible picture shown where a collectible showing its own picture lies (#1538). Immutable; the
/// room publishes a new plane when gameplay changes a block, so renderers draw each block's picture from the room's grid
/// with no stored picture grid (#1346, #1431, #1469).
/// </summary>
public sealed class RoomLevelPlane
{
    /// <summary>Pixels along each side of a level block.</summary>
    public const int BlockPixels = 16;

    /// <summary>What each block shows, row by row.</summary>
    private readonly RoomBlockAppearance[] blocks;

    /// <summary>The collectible picture shown over each block a collectible showing its own picture lies on, by block.</summary>
    private readonly Dictionary<int, CollectiblePicture> collectibles;

    /// <summary>Copies the blocks and the collectible pictures shown over them.</summary>
    /// <param name="size">The room's size in blocks.</param>
    /// <param name="blocks">Exactly <paramref name="size"/>'s blocks, row by row.</param>
    /// <param name="collectibles">The collectible picture shown over each block a collectible lies on, by row-major block index; none when null.</param>
    /// <exception cref="ArgumentException">The blocks do not cover the room, or a collectible lies outside it.</exception>
    public RoomLevelPlane(Extent16<RoomBlocks> size, ReadOnlySpan<RoomBlockAppearance> blocks,
        IReadOnlyDictionary<int, CollectiblePicture>? collectibles = null)
    {
        int count = size.X * size.Y;
        if (count == 0 || blocks.Length != count)
            throw new ArgumentException($"A {size.X}x{size.Y}-block plane needs {count} blocks; got {blocks.Length}.", nameof(blocks));
        Size = size;
        this.blocks = blocks.ToArray();
        this.collectibles = collectibles is null ? [] : new Dictionary<int, CollectiblePicture>(collectibles);
        if (this.collectibles.Keys.Any(block => (uint)block >= (uint)count))
            throw new ArgumentException($"A collectible lies outside the {size.X}x{size.Y}-block plane.", nameof(collectibles));
    }

    /// <summary>The room's size in blocks.</summary>
    public Extent16<RoomBlocks> Size { get; }

    /// <summary>
    /// The plane's blocks as a frame shows them (#1403), read once: a collectible's picture where one lies, as
    /// <see cref="CollectibleCellPictures"/> names it, and each other block's own.
    /// </summary>
    public RoomBlockCells Cells => field ??= new RoomBlockCells(Size,
        [.. blocks.Select((block, index) => collectibles.TryGetValue(index, out CollectiblePicture picture)
            ? new RoomBlockCell(CollectibleCellPictures.DefinitionOf(picture), BlockMirroring.None)
            : new RoomBlockCell(block.Block, block.Mirroring))]);
}
