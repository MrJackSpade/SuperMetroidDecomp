namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How the game's terrain lookups reach a block from another (#1608, #1628): the room's blocks run in reading order, so a
/// step along a row past the row's last block continues on the next row's first, one before a row's first block
/// continues on the last block of the row above, and a column past the room's width names a block that many columns into
/// the following rows. A step down or up the rows keeps its column. The block a lookup reaches this way is the one it
/// collides with, reacts to or changes. A lookup crosses a row's end only at the room's side edges, so the run-on is
/// observed only there.
/// </summary>
/// <remarks>
/// <para>
/// Consulted wherever a lookup steps from one block to another: a room object reaching its neighbour or moving its
/// origin, an enemy projectile's extension links and its block scans across a leading edge during the enemy projectile
/// pass, and, within <see cref="BlockScanCycle"/>, the enemies' terrain movers and the vertical Wave scan. A rule with
/// no state.
/// </para>
/// <para>
/// Evidence: the original addresses a room's blocks by their place in its level data, the row times the room's width
/// plus the column, and steps from block to block by adding to that place: the enemy projectiles' horizontal extension
/// reaction (<c>$86:8506</c>) adds the link's offset, the vertical one (<c>$86:853C</c>) the offset times the room's width,
/// and their block scans (<c>$86:88B6</c>, <c>$86:897B</c>) step by the room's width down a column and by one block
/// along a row, so a scan along a row that crosses its end reads the next row's first blocks. The room objects' neighbour
/// steps add their offsets to the same place.
/// </para>
/// </remarks>
[CompatibilityService("a step along a room's rows past a row's end continues on the next row, and one before a row's start on the row above")]
public static class RoomReadingOrder
{
    /// <summary>
    /// The block <paramref name="step"/> from <paramref name="block"/> in the room's reading order: the step's columns
    /// are counted along the rows and its rows down them, so a column step past a row's end continues on the following
    /// row. The block reached lies in the room's columns; a row past the room's height is a block beyond its grid.
    /// </summary>
    /// <param name="block">The block stepped from; a column past the room's width runs into the following rows.</param>
    /// <param name="step">The columns and rows stepped.</param>
    /// <param name="roomWidth">The room's width in blocks, one or more.</param>
    /// <returns>The block reached, or null when the step leads before the room's first block.</returns>
    public static Position16<RoomBlocks>? Step(Position16<RoomBlocks> block, Offset16<RoomBlocks> step, int roomWidth)
    {
        int reached = PlaceStepped(block, step, roomWidth);
        return reached < 0 ? null : BlockAt(reached, roomWidth);
    }

    /// <summary>
    /// How many blocks precede the block <paramref name="step"/> from <paramref name="block"/> in the room's reading order;
    /// below zero when the step leads before the room's first block.
    /// </summary>
    /// <param name="block">The block stepped from; a column past the room's width runs into the following rows.</param>
    /// <param name="step">The columns and rows stepped.</param>
    /// <param name="roomWidth">The room's width in blocks, one or more.</param>
    internal static int PlaceStepped(Position16<RoomBlocks> block, Offset16<RoomBlocks> step, int roomWidth) =>
        PlaceOf(block, roomWidth) + step.Y * roomWidth + step.X;

    /// <summary>
    /// How many blocks precede <paramref name="block"/> in the room's reading order: its row's blocks, then its column as
    /// it is.
    /// </summary>
    /// <param name="block">The block; a column past the room's width runs into the following rows.</param>
    /// <param name="roomWidth">The room's width in blocks, one or more.</param>
    internal static int PlaceOf(Position16<RoomBlocks> block, int roomWidth) =>
        block.Y * Width(roomWidth) + block.X;

    /// <summary>The block that <paramref name="place"/> blocks follow in the room's reading order.</summary>
    /// <param name="place">The blocks before it, zero or more.</param>
    /// <param name="roomWidth">The room's width in blocks, one or more.</param>
    internal static Position16<RoomBlocks> BlockAt(int place, int roomWidth)
    {
        Ensure.AtLeastZero(place);
        return Position16<RoomBlocks>.Of(place % Width(roomWidth), place / roomWidth);
    }

    /// <summary>A room's width, which must hold at least one block for its rows to be read in order.</summary>
    private static int Width(int roomWidth) => Ensure.BetweenInclusive(1, ushort.MaxValue, roomWidth);
}
