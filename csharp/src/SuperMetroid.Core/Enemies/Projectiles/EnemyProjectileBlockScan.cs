namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The blocks an enemy projectile's block scan tests across its leading edge (#1628): it starts at the block where the
/// edge meets the first pixel of the span the projectile covers across its move, and steps down the edge's column for a
/// horizontal move or along the edge's row for a vertical one, testing the first block and the blocks
/// <see cref="BlockScanCycle.BlocksAfterFirstAcross"/> counts after it. The scan names its blocks through
/// <see cref="BlockScanCycle"/>.
/// </summary>
/// <remarks>
/// Built once for each axis an enemy projectile moves on during the enemy projectile pass, by the shared axis move
/// (<see cref="EnemyProjectileEngine"/>); each tested block is then judged by the move's
/// <see cref="EnemyProjectileBlockCollision"/>.
/// </remarks>
internal readonly partial record struct EnemyProjectileBlockScan
{
    /// <summary>A value of its parts; only its own operations make one.</summary>
    /// <param name="first">The first block tested, its row as the edge or span names it; <see cref="Block"/> takes it within the cycle.</param>
    /// <param name="edge">The edge scanned: horizontal for a horizontal move, stepping down the column; vertical for a vertical one, stepping along the row.</param>
    /// <param name="extraBlocks">How many blocks the scan tests after the first.</param>
    internal EnemyProjectileBlockScan(Position16<RoomBlocks> first, Axis edge, int extraBlocks)
    {
        First = first;
        Edge = edge;
        ExtraBlocks = extraBlocks;
    }

    /// <summary>The first block tested, its row as the edge or span names it; <see cref="Block"/> takes it within the cycle.</summary>
    public Position16<RoomBlocks> First { get; init; }

    /// <summary>The edge scanned: horizontal for a horizontal move, stepping down the column; vertical for a vertical one, stepping along the row.</summary>
    public Axis Edge { get; init; }

    /// <summary>How many blocks the scan tests after the first.</summary>
    public int ExtraBlocks { get; init; }

    /// <summary>The scan across a leading edge.</summary>
    /// <param name="edgePixel">The leading edge's pixel on the axis of the move, wrapping as
    /// <see cref="CoordinateAxis"/> states.</param>
    /// <param name="spanStartPixel">The first pixel of the span across the move, wrapping as
    /// <see cref="CoordinateAxis"/> states.</param>
    /// <param name="spanEndPixel">The last pixel of the span across the move, wrapping as <see cref="CoordinateAxis"/>
    /// states.</param>
    /// <param name="edge">The edge scanned: horizontal for a horizontal move, vertical for a vertical one.</param>
    internal static EnemyProjectileBlockScan Across(int edgePixel, int spanStartPixel, int spanEndPixel, Axis edge)
    {
        int edgeLine = RoomGridConversions.PixelToBlock(TerrainScanEdges.Edge(edgePixel));
        int spanStart = TerrainScanEdges.Edge(spanStartPixel);
        int extraBlocks = BlockScanCycle.BlocksAfterFirstAcross(spanStart, TerrainScanEdges.Edge(spanEndPixel));
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
