namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How Samus's block scans, the enemies' terrain movers and the enemy projectiles' block scans number the rows and columns
/// they look up (#1552, #1593, #1628): each
/// lookup holds the row it starts from within a cycle of 256, so a row past the 256th, or one above the room's top whose
/// number continued from the bottom of the cycle, names a row counted from the top of the room again. The scan then
/// counts the blocks it steps through within a cycle of 32768, so one that runs past it continues from the room's first
/// block. The blocks a lookup finds there are the ones the scan reacts to, reveals, swings against or collides with. The
/// rooms are authored inside both cycles, so they are observed only at a room's edges and by lookups that run past them.
/// </summary>
/// <remarks>
/// <para>
/// Consulted wherever one of those scans turns a row or column into a block: a Wave shot's horizontal and vertical
/// reaction scans during the projectile pass, the grapple swing's body probe on every swinging update, the X-ray scope's
/// reveal and its extension chains on every update the scope is held, and every enemy's horizontal and vertical terrain
/// move during the enemy pass, with the extension links its blocks lead it through, and every enemy projectile's
/// horizontal and vertical block scan during the enemy projectile pass (<c>EnemyProjectileBlockScan</c>). A scan starts at
/// a block named by its
/// column and row; the column is added to the start of the row as it is, so a column past the room's width runs into the
/// rows below. The blocks a scan reaches are found in the room's reading order (<see cref="RoomReadingOrder"/>), within
/// the room's columns, and a row past the room's height is a block beyond its grid.
/// </para>
/// <para>
/// Evidence: each of the original routines multiplies the row by the room's width with eight-bit operands, keeping only
/// the row's and the width's lowest eight bits; the Wave scans (<c>$94:A352</c>, <c>$94:A3E4</c>) take the projectile's
/// row that way, the X-ray reveal (<c>$91:CC70</c>, <c>$91:CE57</c>) its rows, and the enemy movers
/// (<c>$A0:C6AD</c>, <c>$A0:C788</c>) and the enemy projectile moves (<c>$86:88B6</c>, <c>$86:897B</c>) the row of the
/// top edge or of the leading edge. The scans then step their
/// block index, and the enemies' extension reactions (<c>$A0:C619</c>, <c>$A0:C64F</c>) add their offsets to it, as a
/// sixteen-bit offset two bytes a block.
/// </para>
/// </remarks>
[CompatibilityService("a block scan's rows continue from zero past the 256th and its blocks past the 32768th, so scans past a room's edge read blocks from its top")]
public static class BlockScanCycle
{
    /// <summary>The row and column numbers a scan's lookup holds before it continues from zero.</summary>
    private const int Places = 256;

    /// <summary>The blocks a scan along the room's rows counts before it continues from the room's first block.</summary>
    private const int BlocksAlongRows = 32768;

    /// <summary>The blocks an X-ray reveal's extension lookup counts before it continues from the room's first block.</summary>
    private const int RevealLookupBlocks = 65536;

    /// <summary>The place of <paramref name="blocks"/> within the cycle, 0 through 255.</summary>
    /// <param name="blocks">A row or column number, which may lie outside the cycle or below zero.</param>
    public static int Within(int blocks) => (blocks % Places + Places) % Places;

    /// <summary>
    /// The block an X-ray reveal's extension lookup reads at <paramref name="block"/>: the row taken within the cycle of
    /// 256 and the column added to that row's start in the room's reading order (<see cref="RoomReadingOrder"/>), the
    /// blocks before it counted within a cycle of 65536, as the lookup's behaviour read indexes them with a sixteen-bit
    /// word (<c>$91:CE5F</c>).
    /// </summary>
    /// <param name="block">The column and row the lookup names; the row may lie outside the cycle and the column past the room's width.</param>
    /// <param name="roomWidth">The room's width in blocks, one or more.</param>
    /// <returns>The block read, in the room's columns; a row past the room's height is a block beyond its grid.</returns>
    public static Position16<RoomBlocks> RevealLookupBlock(Position16<RoomBlocks> block, int roomWidth) =>
        RoomReadingOrder.BlockAt(RoomReadingOrder.PlaceOf(CycleStart(block), roomWidth) % RevealLookupBlocks, roomWidth);

    /// <summary>
    /// The block a scan reaches <paramref name="step"/> blocks along the room's rows after <paramref name="start"/>: the
    /// vertical Wave scan's and an enemy's vertical move's walk across the columns of the row they reach, which continues
    /// on the following rows past a row's end (<see cref="RoomReadingOrder"/>).
    /// </summary>
    /// <param name="start">The column and row of the scan's first block; the row is taken within the cycle of 256.</param>
    /// <param name="roomWidth">The room's width in blocks, one or more.</param>
    /// <param name="step">The blocks stepped since the first.</param>
    /// <returns>The block reached, in the room's columns; a row past the room's height is a block beyond its grid.</returns>
    public static Position16<RoomBlocks> BlockAlongRows(Position16<RoomBlocks> start, int roomWidth, int step) =>
        Reached(RoomReadingOrder.PlaceStepped(CycleStart(start), Offset16<RoomBlocks>.Of(Ensure.AtLeastZero(step), 0), roomWidth), roomWidth);

    /// <summary>
    /// The block a scan reaches <paramref name="step"/> rows below <paramref name="start"/>: an enemy's horizontal move's
    /// walk down the rows of the column it reaches. Only the starting row is taken within the cycle of 256; each step adds
    /// a row to the block the scan has reached.
    /// </summary>
    /// <param name="start">The column and row of the scan's first block; the row is taken within the cycle of 256.</param>
    /// <param name="roomWidth">The room's width in blocks, one or more.</param>
    /// <param name="step">The rows stepped since the first.</param>
    /// <returns>The block reached, in the room's columns; a row past the room's height is a block beyond its grid.</returns>
    public static Position16<RoomBlocks> BlockDownRows(Position16<RoomBlocks> start, int roomWidth, int step) =>
        Reached(RoomReadingOrder.PlaceStepped(CycleStart(start), Offset16<RoomBlocks>.Of(0, Ensure.AtLeastZero(step)), roomWidth), roomWidth);

    /// <summary>
    /// The block an extension link leads a scan to from <paramref name="block"/>, a block the scan reached: the link's
    /// columns and rows are stepped in the room's reading order (<see cref="RoomReadingOrder"/>), and the block reached is
    /// counted within the same cycle of 32768.
    /// </summary>
    /// <param name="block">The block the scan reached.</param>
    /// <param name="link">The link's columns (a horizontal link) or rows (a vertical link); it may lead back, but never before the room's first block.</param>
    /// <param name="roomWidth">The room's width in blocks, one or more.</param>
    /// <returns>The block reached, in the room's columns; a row past the room's height is a block beyond its grid.</returns>
    public static Position16<RoomBlocks> LinkedBlock(Position16<RoomBlocks> block, Offset16<RoomBlocks> link, int roomWidth) =>
        Reached(RoomReadingOrder.PlaceStepped(block, link, roomWidth), roomWidth);

    /// <summary>A scan's first block: its row taken within the cycle of 256, and its column as it is.</summary>
    private static Position16<RoomBlocks> CycleStart(Position16<RoomBlocks> start) => start.WithY(Within(start.Y));

    /// <summary>The block that <paramref name="blocks"/> blocks, zero or more, follow along the room's rows within the cycle of 32768.</summary>
    private static Position16<RoomBlocks> Reached(int blocks, int roomWidth) =>
        RoomReadingOrder.BlockAt(Ensure.AtLeastZero(blocks) % BlocksAlongRows, roomWidth);
}
