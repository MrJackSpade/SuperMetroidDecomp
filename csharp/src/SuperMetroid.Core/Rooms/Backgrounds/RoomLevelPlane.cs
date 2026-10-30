using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One room background plane as the room shows it: a grid of 16x16 blocks, each naming the block it shows and its
/// mirroring. Immutable; the room publishes a new plane when gameplay changes a block, so renderers draw each block's
/// picture from the room's grid with no stored tilemap (#1346, #1431, #1469).
/// </summary>
public sealed class RoomLevelPlane
{
    /// <summary>Pixels along each side of a level block.</summary>
    public const int BlockPixels = 16;

    /// <summary>What each block shows, row by row.</summary>
    private readonly RoomBlockAppearance[] blocks;

    /// <summary>Copies the blocks.</summary>
    /// <param name="size">The room's size in blocks.</param>
    /// <param name="blocks">Exactly <paramref name="size"/>'s blocks, row by row.</param>
    /// <exception cref="ArgumentException">The blocks do not cover the room.</exception>
    public RoomLevelPlane(Extent16<RoomBlocks> size, ReadOnlySpan<RoomBlockAppearance> blocks)
    {
        int count = size.X * size.Y;
        if (count == 0 || blocks.Length != count)
            throw new ArgumentException($"A {size.X}x{size.Y}-block plane needs {count} blocks; got {blocks.Length}.", nameof(blocks));
        Size = size;
        this.blocks = blocks.ToArray();
    }

    /// <summary>The room's size in blocks.</summary>
    public Extent16<RoomBlocks> Size { get; }

    /// <summary>What each block shows, row by row.</summary>
    public ReadOnlySpan<RoomBlockAppearance> Blocks => blocks;

    /// <summary>The plane's blocks as a frame shows them (#1403), read once.</summary>
    public RoomBlockCells Cells => field ??= new RoomBlockCells(Size,
        [.. blocks.Select(block => new RoomBlockCell(block.Block, block.Mirroring))]);
}
