namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How Samus's block scans number the rows and columns they look up (#1552): each lookup holds a row or column number
/// within a cycle of 256, so a row past the 256th, or one above the room's top whose number continued from the bottom of
/// the cycle, addresses a row counted from the top of the room again, and the blocks the lookup finds there are the ones
/// the scan reacts to, reveals or swings against. The rooms are authored inside the cycle, so it is observed only at a
/// room's edges and by lookups that run past them.
/// </summary>
/// <remarks>
/// Consulted wherever one of those scans turns a row or column into a block: a Wave shot's horizontal and vertical
/// reaction scans during the projectile pass, the grapple swing's body probe on every swinging update, and the X-ray
/// scope's reveal and its extension chains on every update the scope is held. Evidence: each of the original routines
/// multiplies the row by the room's width with eight-bit operands, keeping only the row's and the width's lowest eight bits;
/// the Wave scans (<c>$94:A352</c>, <c>$94:A3E4</c>) take the projectile's row that way and the X-ray reveal
/// (<c>$91:CC70</c>, <c>$91:CE57</c>) its rows.
/// </remarks>
[CompatibilityService("a block scan's rows and columns continue from zero past the 256th, so scans past a room's edge read rows from its top")]
public static class BlockScanCycle
{
    /// <summary>The row and column numbers a scan's lookup holds before it continues from zero.</summary>
    private const int Places = 256;

    /// <summary>The place of <paramref name="blocks"/> within the cycle, 0 through 255.</summary>
    /// <param name="blocks">A row or column number, which may lie outside the cycle or below zero.</param>
    public static int Within(int blocks) => (blocks % Places + Places) % Places;

    /// <summary>The index of the first block of <paramref name="row"/>: the row's place in the cycle times the room's width.</summary>
    /// <param name="row">The row number, which may lie outside the cycle.</param>
    /// <param name="roomWidth">The room's width in blocks.</param>
    public static int RowStart(int row, int roomWidth) => Within(row) * roomWidth;

    /// <summary>
    /// The block a scan along the room's rows reaches <paramref name="step"/> blocks after <paramref name="first"/>: the
    /// scan counts the room's blocks within a cycle of 32768, so one that runs past it continues from the room's first
    /// block. Evidence: the vertical Wave scan (<c>$94:A3E4</c>) steps its block index as a sixteen-bit offset two bytes
    /// a block.
    /// </summary>
    /// <param name="first">The index of the scan's first block, counted along the room's rows.</param>
    /// <param name="step">The blocks stepped since the first.</param>
    public static int BlockAlongRows(int first, int step) =>
        (Ensure.AtLeastZero(first) + Ensure.AtLeastZero(step)) % BlocksAlongRows;

    /// <summary>The blocks a scan along the room's rows counts before it continues from the room's first block.</summary>
    private const int BlocksAlongRows = 32768;
}
