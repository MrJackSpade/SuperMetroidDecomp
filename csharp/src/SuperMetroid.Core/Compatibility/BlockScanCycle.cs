namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How Samus's block scans, the enemies' terrain movers and the enemy projectiles' block scans number the rows and columns
/// they look up (#1552, #1593, #1628): each
/// lookup holds the row it starts from within a cycle of 256, so a row past the 256th, or one above the room's top whose
/// number continued from the bottom of the cycle, names a row counted from the top of the room again. The scan then
/// counts the blocks it steps through within a cycle of 32768, so one that runs past it continues from the room's first
/// block. The blocks a lookup finds there are the ones the scan reacts to, swings against or collides with. The
/// rooms are authored inside both cycles, so they are observed only at a room's edges and by lookups that run past them.
/// An enemy projectile's block scan also counts how many blocks it tests across its span within a cycle of 65536 pixels
/// (<see cref="BlocksAfterFirstAcross"/>), so a span that ends before its first block starts runs the scan on through
/// 4095 further blocks.
/// </summary>
/// <remarks>
/// <para>
/// Consulted wherever one of those scans turns a row or column into a block: a Wave shot's horizontal and vertical
/// reaction scans during the projectile pass, the grapple swing's body probe on every swinging update, and every enemy's
/// horizontal and vertical terrain move during the enemy pass, with the extension links its blocks lead it through, and every enemy projectile's
/// horizontal and vertical block scan during the enemy projectile pass (<c>EnemyProjectileBlockScan</c>). A scan starts at
/// a block named by its
/// column and row; the column is added to the start of the row as it is, so a column past the room's width runs into the
/// rows below. The blocks a scan reaches are found in the room's reading order (<see cref="RoomReadingOrder"/>), within
/// the room's columns, and a row past the room's height is a block beyond its grid.
/// </para>
/// <para>
/// Evidence: each of the original routines multiplies the row by the room's width with eight-bit operands, keeping only
/// the row's and the width's lowest eight bits; the Wave scans (<c>$94:A352</c>, <c>$94:A3E4</c>) take the projectile's
/// row that way, and the enemy movers
/// (<c>$A0:C6AD</c>, <c>$A0:C788</c>) and the enemy projectile moves (<c>$86:88B6</c>, <c>$86:897B</c>) the row of the
/// top edge or of the leading edge. The scans then step their
/// block index, and the enemies' extension reactions (<c>$A0:C619</c>, <c>$A0:C64F</c>) add their offsets to it, as a
/// sixteen-bit offset two bytes a block. The enemy projectile moves count the blocks to test as the span's last pixel
/// less its first pixel's block start, a difference kept within sixteen bits, over 16, and test one block more than that.
/// </para>
/// </remarks>
[CompatibilityService("a block scan's rows continue from zero past the 256th and its blocks past the 32768th, so scans past a room's edge read blocks from its top; an enemy projectile's scan counts its span within 65536 pixels, so a span ending before its first block runs 4095 blocks on")]
public static partial class BlockScanCycle
{
    /// <summary>The pixels within which an enemy projectile's scan counts its span before the count continues from zero.</summary>
    private const int SpanPixels = 65536;

    /// <summary>
    /// How many blocks an enemy projectile's block scan tests after its first across a span: the whole blocks from the
    /// start of the block holding <paramref name="spanStartPixel"/> to <paramref name="spanEndPixel"/>, 0 through 4095.
    /// The distance is counted within 65536 pixels, so a span whose last pixel lies before its first block's start (a
    /// zero radius on a block boundary gives one) counts on to 4095 blocks, and the scan runs on through the room until a
    /// block stops it.
    /// </summary>
    /// <param name="spanStartPixel">The span's first pixel, a room coordinate 0 through 65535.</param>
    /// <param name="spanEndPixel">The span's last pixel, a room coordinate 0 through 65535.</param>
    public static int BlocksAfterFirstAcross(int spanStartPixel, int spanEndPixel)
    {
        int distance = Ensure.BetweenInclusive(0, SpanPixels - 1, spanEndPixel) - RoomGridConversions.BlockStart(spanStartPixel);
        return RoomGridConversions.WholeBlocksIn((distance % SpanPixels + SpanPixels) % SpanPixels);
    }

    /// <summary>The row and column numbers a scan's lookup holds before it continues from zero.</summary>
    private const int Places = 256;

    /// <summary>The blocks a scan along the room's rows counts before it continues from the room's first block.</summary>
    private const int BlocksAlongRows = 32768;

    /// <summary>The place of <paramref name="blocks"/> within the cycle, 0 through 255.</summary>
    /// <param name="blocks">A row or column number, which may lie outside the cycle or below zero.</param>
    public static int Within(int blocks) => (blocks % Places + Places) % Places;

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
