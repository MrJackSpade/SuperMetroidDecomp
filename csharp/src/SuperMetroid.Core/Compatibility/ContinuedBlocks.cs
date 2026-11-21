namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A block of terrain spanning several of a room's cells (#2572): a door cap four blocks tall, a block that breaks two by
/// two, a scroll trigger's reach, Mother Brain's glass. Its first cell holds the block; each other cell continues it, its
/// link leading a number of blocks along its row or column toward the block it stands for, through other continuing
/// cells if it must. Whatever reads a continuing cell (Samus's body, her shots, missiles and bombs, the grapple beam)
/// collides with and reacts to the block it stands for, as that block stands then, and the reaction takes place at that
/// block: a shot striking a door cap's lowest cell opens the cap from its first. A link counts its blocks in the room's
/// reading order, so one along a row past the row's end goes on into the next row (<see cref="RoomReadingOrder"/>). A
/// continuing cell whose link is zero stands for nothing and behaves as air.
/// </summary>
/// <remarks>
/// Consulted wherever one of those reads finds a continuing cell, before it collides or reacts. The service is written
/// in its own types: the room module supplies how a cell continues and where its link leads. A chain of as many links as
/// the room has blocks must revisit one, a cycle the original never leaves, and is reported as broken room data.
/// Evidence: the original's horizontal extension reaction (<c>$94:9411</c>) adds the cell's signed parameter to its
/// block index and the vertical one (<c>$94:9447</c>) that many rows, then runs the reached block's reaction in the
/// reading cell's stead, every block-shot, bombed, grappled, collision and inside reaction table sending both extension
/// types there; a zero parameter returns no collision.
/// </remarks>
[CompatibilityService("a cell that continues a several-cell block collides and reacts as the block its link leads to, at that block, as that block stands then; a zero link behaves as air")]
public static partial class ContinuedBlocks
{
    /// <summary>
    /// The block <paramref name="cell"/> stands for: the cell itself when it continues nothing, the block its chain of links
    /// leads to when it continues one, or null when a link in the chain is zero and the cell behaves as air.
    /// </summary>
    /// <typeparam name="TCell">How the room module reads a cell.</typeparam>
    /// <param name="cell">The cell a read found.</param>
    /// <param name="roomBlocks">How many blocks the room has, one or more: a longer chain revisits a cell.</param>
    /// <param name="continues">Whether a cell continues another block.</param>
    /// <param name="standsForNothing">Whether a continuing cell's link is zero.</param>
    /// <param name="linked">The cell a continuing cell's link leads to.</param>
    /// <exception cref="InvalidDataException">The chain never leaves its continuing cells.</exception>
    public static TCell? StoodFor<TCell>(TCell cell, int roomBlocks, Func<TCell, bool> continues, Func<TCell, bool> standsForNothing,
        Func<TCell, TCell> linked) where TCell : struct
    {
        Ensure.NotNull(continues);
        Ensure.NotNull(standsForNothing);
        Ensure.NotNull(linked);
        for (int hops = Ensure.BetweenInclusive(1, int.MaxValue, roomBlocks); hops > 0; hops--)
        {
            if (!continues(cell))
                return cell;
            if (standsForNothing(cell))
                return null;
            cell = linked(cell);
        }
        throw new InvalidDataException($"A chain of continuing cells starting at {cell} never reaches the block it stands for.");
    }
}
