namespace SuperMetroid.Core.Samus;

/// <summary>
/// How Samus's block scans number the rows and columns they look up: the Wave shot's span scans, the grapple swing's
/// body probe and the X-ray scope's reveal. Each lookup holds a row or column number within a cycle of 256, so a row
/// past the 256th, or one above the room's top whose number continued from the bottom of the cycle, addresses a row
/// counted from the top of the room again. The rooms themselves are authored inside the cycle, so the cycle is
/// observed only at the room's edges and by lookups that run past them.
/// </summary>
internal static class BlockScanCycle
{
    /// <summary>The row and column numbers a scan's lookup holds before it continues from zero.</summary>
    private const int Places = 256;

    /// <summary>The place of <paramref name="blocks"/> within the cycle, 0 through 255.</summary>
    /// <param name="blocks">A row or column number, which may lie outside the cycle or below zero.</param>
    internal static int Within(int blocks) => (blocks % Places + Places) % Places;

    /// <summary>The index of the first block of <paramref name="row"/>: the row's place in the cycle times the room's width.</summary>
    /// <param name="row">The row number, which may lie outside the cycle.</param>
    /// <param name="roomWidth">The room's width in blocks.</param>
    internal static int RowStart(int row, int roomWidth) => Within(row) * roomWidth;
}
