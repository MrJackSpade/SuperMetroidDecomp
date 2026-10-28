using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One room background plane as its level data shows it: a grid of 16x16 blocks, each a visual level word naming a
/// block and its mirroring. Immutable; the room publishes a new plane when gameplay changes a block, so renderers draw
/// each block's picture from level data with no stored tilemap (#1346, #1431).
/// </summary>
public sealed class RoomLevelPlane
{
    /// <summary>Pixels along each side of a level block.</summary>
    public const int BlockPixels = 16;

    /// <summary>The visual level words, row by row.</summary>
    private readonly ushort[] blocks;

    /// <summary>Copies the logical blocks.</summary>
    /// <param name="size">The room's size in blocks.</param>
    /// <param name="blocks">At least <paramref name="size"/>'s blocks of visual level words, row by row; any allocation tail is ignored.</param>
    /// <exception cref="ArgumentException">The blocks do not cover the room.</exception>
    public RoomLevelPlane(Extent16<RoomBlocks> size, ReadOnlySpan<ushort> blocks)
    {
        int count = size.X * size.Y;
        if (count == 0 || blocks.Length < count)
            throw new ArgumentException($"A {size.X}x{size.Y}-block plane needs {count} level words; got {blocks.Length}.", nameof(blocks));
        Size = size;
        this.blocks = blocks[..count].ToArray();
    }

    /// <summary>The room's size in blocks.</summary>
    public Extent16<RoomBlocks> Size { get; }

    /// <summary>The visual level words, row by row.</summary>
    public ReadOnlySpan<ushort> Blocks => blocks;

    /// <summary>The plane's blocks as a frame shows them (#1403), read once.</summary>
    public RoomBlockCells Cells => field ??= new RoomBlockCells(Size, [.. blocks.Select(word =>
    {
        var level = new RoomLevelWord(word);
        return new RoomBlockCell(level.VisualBlockIndex, level.VisualFlipFlags);
    })]);
}
