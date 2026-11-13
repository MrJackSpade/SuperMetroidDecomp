using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A room plane's blocks as a frame shows them (#1403): each block's definition and mirroring, row by row, which the
/// room's block pictures draw. Outside the room the plane shows nothing.
/// </summary>
public sealed class RoomBlockCells
{
    /// <summary>Holds a plane's blocks.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The plane has no blocks along an axis.</exception>
    /// <exception cref="ArgumentException">The cells do not fill the plane.</exception>
    public RoomBlockCells(Extent16<RoomBlocks> size, ImmutableArray<RoomBlockCell> cells)
    {
        // A plane is at least one block along each axis; its size's own type holds its far bound (#1627).
        if (size.X == 0 || size.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(size), size, "A room plane is at least one block along each axis.");
        Ensure.LengthEqual(cells.AsSpan(), size.X * size.Y);
        Size = size;
        Cells = cells;
    }

    /// <summary>The plane's size in blocks.</summary>
    public Extent16<RoomBlocks> Size { get; }

    /// <summary>The blocks, row by row.</summary>
    public ImmutableArray<RoomBlockCell> Cells { get; }
}
