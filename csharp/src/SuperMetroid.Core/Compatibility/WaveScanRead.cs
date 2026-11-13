namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where one step of a Wave shot's horizontal reaction scan reads (#1469, #1552): the block the read starts in, and
/// whether it lands halfway through that block and so reads the terrain straddling it and the next one
/// (<see cref="StraddledTerrain{TTerrain}"/>). The scan walks down the rows the shot's leading edge spans, but it counts its place
/// in halves of a block within a cycle of 65536 halves, so a walk that runs past the cycle above the room's ceiling lands
/// one half further on, halfway through a block, for the rest of its steps.
/// </summary>
/// <remarks>
/// <para>
/// Consulted by the horizontal Wave-shot reaction scan during the projectile pass, once for each step of its scan
/// (<see cref="At"/>). The scan's place starts at the target column of the shot's top row, the row and the room's width
/// taken within the <see cref="BlockScanCycle"/>; each step adds the room's width to the place twice, and the second
/// addition takes one more half when the first ran past the cycle. A place on a block's second half straddles that block
/// and the next. A rule with no state.
/// </para>
/// <para>
/// Evidence: the original horizontal Wave scan counts its place in halves of a block and adds the room's width to it
/// twice for each row it steps, discarding an overflow only before the first addition, so a first addition that runs
/// past the cycle makes the second take one more half; a place on a block's second half then reads the end of that
/// block and the start of the next.
/// </para>
/// </remarks>
/// <param name="Block">The block the read starts in, counted along the room's rows from its first block, 0 through 32767.</param>
/// <param name="StraddlesNext">
/// Whether the read starts halfway through <paramref name="Block"/>, reading the terrain straddling it and the next
/// (<see cref="StraddledTerrain{TTerrain}.Straddling"/>); otherwise it reads the block itself.
/// </param>
[CompatibilityService("where a Wave shot's scan past the room's ceiling reads: a block, or the terrain straddling it and the next")]
public readonly record struct WaveScanRead(int Block, bool StraddlesNext)
{
    /// <summary>The halves of a block the scan counts its place in.</summary>
    private const int HalvesPerBlock = 2;

    /// <summary>The halves the scan's place counts before it continues from zero.</summary>
    private const int HalfPlaces = 65536;

    /// <summary>
    /// Where a Wave shot's horizontal reaction scan reads at <paramref name="step"/>: from the shot's top row, one row
    /// further for each step until the walk runs past the cycle, after which each read lands halfway through a block.
    /// </summary>
    /// <param name="start">
    /// The column of the shot's leading edge and the row of its top edge; the row may lie outside the scan cycle.
    /// </param>
    /// <param name="roomWidth">The room's width in blocks.</param>
    /// <param name="step">The scan's step, zero for the top row.</param>
    public static WaveScanRead At(Position16<RoomBlocks> start, int roomWidth, int step)
    {
        Ensure.AtLeastZero(roomWidth);
        Ensure.AtLeastZero(step);
        int place = WithinPlaces(
            (BlockScanCycle.Within(start.Y) * BlockScanCycle.Within(roomWidth) + start.X) * HalvesPerBlock);
        for (int taken = 0; taken < step; taken++)
        {
            int first = place + roomWidth;
            place = WithinPlaces(first + roomWidth + (first >= HalfPlaces ? 1 : 0));
        }
        return new WaveScanRead(place / HalvesPerBlock, place % HalvesPerBlock != 0);
    }

    /// <summary>
    /// The block the read starts in, as a position: <see cref="Block"/>'s place along the room's rows in the room's
    /// reading order (<see cref="RoomReadingOrder"/>), in the room's columns; a row past the room's height is a block
    /// beyond its grid.
    /// </summary>
    /// <param name="roomWidth">The room's width in blocks, one or more.</param>
    public Position16<RoomBlocks> BlockIn(int roomWidth) => RoomReadingOrder.BlockAt(Block, roomWidth);

    /// <summary>A place of the scan within its cycle of halves.</summary>
    private static int WithinPlaces(int halves) => halves % HalfPlaces;
}
