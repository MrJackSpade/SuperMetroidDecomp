using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A room plane's blocks as a frame shows them (#1403): each block's definition and mirroring, row by row, which the
/// room's block pictures draw. Outside the room the plane shows nothing.
/// </summary>
public sealed class RoomBlockCells
{
    /// <summary>Holds a plane's blocks.</summary>
    /// <exception cref="ArgumentException">The cells do not fill the plane.</exception>
    public RoomBlockCells(Extent16<RoomBlocks> size, ImmutableArray<RoomBlockCell> cells)
    {
        Ensure.BetweenInclusive(1, ushort.MaxValue, size.X);
        Ensure.BetweenInclusive(1, ushort.MaxValue, size.Y);
        Ensure.LengthEqual(cells.AsSpan(), size.X * size.Y);
        Size = size;
        Cells = cells;
    }

    /// <summary>The plane's size in blocks.</summary>
    public Extent16<RoomBlocks> Size { get; }

    /// <summary>The blocks, row by row.</summary>
    public ImmutableArray<RoomBlockCell> Cells { get; }

    /// <summary>The block at a level pixel and the pixel within it; false outside the room.</summary>
    /// <param name="level">The level pixel; coordinates past the room's right or bottom edge are outside.</param>
    /// <param name="cell">The block there.</param>
    /// <param name="within">The pixel within the block, from its top-left corner, before its mirroring.</param>
    public bool TryCellAt(Position16<BgPlanePixels> level, out RoomBlockCell cell, out Position16<BgPlanePixels> within)
    {
        if (level.X >= Size.X * RoomLevelPlane.BlockPixels || level.Y >= Size.Y * RoomLevelPlane.BlockPixels)
        {
            cell = default;
            within = default;
            return false;
        }
        cell = Cells[level.Y / RoomLevelPlane.BlockPixels * Size.X + level.X / RoomLevelPlane.BlockPixels];
        within = new Position16<BgPlanePixels>((ushort)(level.X % RoomLevelPlane.BlockPixels), (ushort)(level.Y % RoomLevelPlane.BlockPixels));
        return true;
    }
}
