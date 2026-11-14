namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The blocks an enemy projectile's block scan tests across its leading edge (#1628): it starts at the block where the
/// edge meets the first pixel of the span the projectile covers across its move, and steps down the edge's column for a
/// horizontal move or along the edge's row for a vertical one, testing one block more than the span's distance in whole
/// blocks from that first block's start to its last pixel. The scan names its blocks through
/// <see cref="BlockScanCycle"/>: its first row is taken within the cycle of 256, a column past the room's width runs
/// into the following rows, a step along a row past its end continues on the next row, and the blocks are counted within
/// the cycle of 32768.
/// </summary>
/// <remarks>
/// <para>
/// Built once for each axis an enemy projectile moves on during the enemy projectile pass, by the shared axis move and
/// by the eye door's projectiles; each tested block is then resolved through its extension links and classified by the
/// caller. A span whose last pixel lies before its first block's start, as a zero radius on a block boundary gives, counts
/// on past 65535 to 4095 more blocks.
/// </para>
/// <para>
/// Evidence: the original's horizontal and vertical enemy projectile moves count the blocks to test as the span's last
/// pixel less its first pixel's block start, a difference counted within 65536, over 16, and test one block more than that; they
/// name the blocks as <see cref="BlockScanCycle"/> steps, stepping a row down a column or a block along a row.
/// </para>
/// </remarks>
/// <param name="First">The first block tested, its row as the edge or span names it; <see cref="Block"/> takes it within the cycle.</param>
/// <param name="Edge">The edge scanned: horizontal for a horizontal move, stepping down the column; vertical for a vertical one, stepping along the row.</param>
/// <param name="ExtraBlocks">How many blocks the scan tests after the first, 0 through 4095.</param>
internal readonly record struct EnemyProjectileBlockScan(Position16<RoomBlocks> First, Axis Edge, int ExtraBlocks)
{
    /// <summary>The scan across a leading edge.</summary>
    /// <param name="edgePixel">The leading edge's pixel on the axis of the move; it wraps past the room coordinates' 65535.</param>
    /// <param name="spanStartPixel">The first pixel of the span across the move; it wraps past 65535.</param>
    /// <param name="spanEndPixel">The last pixel of the span across the move; it wraps past 65535.</param>
    /// <param name="edge">The edge scanned: horizontal for a horizontal move, vertical for a vertical one.</param>
    internal static EnemyProjectileBlockScan Across(int edgePixel, int spanStartPixel, int spanEndPixel, Axis edge)
    {
        int edgeLine = RoomGridConversions.PixelToBlock(CoordinateAxis.Wrap(edgePixel));
        int spanStart = CoordinateAxis.Wrap(spanStartPixel);
        int extraBlocks = RoomGridConversions.WholeBlocksIn(CoordinateAxis.Wrap(CoordinateAxis.Wrap(spanEndPixel) - RoomGridConversions.BlockStart(spanStart)));
        Position16<RoomBlocks> first = edge == Axis.Vertical
            ? Position16<RoomBlocks>.Of(RoomGridConversions.PixelToBlock(spanStart), edgeLine)
            : Position16<RoomBlocks>.Of(edgeLine, RoomGridConversions.PixelToBlock(spanStart));
        return new EnemyProjectileBlockScan(first, edge, extraBlocks);
    }

    /// <summary>The block the scan tests <paramref name="tested"/> blocks after its first.</summary>
    /// <param name="tested">The blocks tested before it, 0 through <see cref="ExtraBlocks"/>.</param>
    /// <param name="roomWidth">The room's width in blocks, one or more.</param>
    /// <returns>The block, in the room's columns; a row past the room's height is a block beyond its grid.</returns>
    internal Position16<RoomBlocks> Block(int tested, int roomWidth) =>
        Edge == Axis.Vertical
            ? BlockScanCycle.BlockAlongRows(First, roomWidth, tested)
            : BlockScanCycle.BlockDownRows(First, roomWidth, tested);
}
