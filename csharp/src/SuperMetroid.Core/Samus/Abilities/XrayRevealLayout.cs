namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The X-ray reveal's blocks (#1431): two screens of 16x16 blocks, written in whole blocks by <c>$91:CB8E</c> and its
/// overlays, of which BG2 shows the first screen and the second screen's first column.
/// </summary>
public static class XrayRevealLayout
{
    /// <summary>Sixteen rows of blocks are constructed, including offscreen rows.</summary>
    public const int MetatileRows = 16;
    /// <summary>Sixteen complete columns plus the right-hand fine-scroll column.</summary>
    public const int MetatileColumns = 17;
    /// <summary>The blocks BG2 shows, row by row, <see cref="MetatileColumns"/> to a row.</summary>
    public const int Cells = MetatileColumns * MetatileRows;
    /// <summary>Blocks across and down one of the reveal's two screens.</summary>
    private const int ScreenBlocks = 16;
    /// <summary>The screens the reveal writes.</summary>
    private const int Screens = 2;

    /// <summary>
    /// The shown cell a whole-block write at <paramref name="column"/>, <paramref name="row"/> lands in, or -1 where it
    /// lands where BG2 shows nothing. Columns past the first screen's sixteen continue in the second screen; a row past a
    /// screen's last continues at the top of the screen that follows it, and past the second screen the write is lost,
    /// as the native buffer lays its two screens one after the other. Of the second screen only its first column is
    /// shown, as the fine-scroll column.
    /// </summary>
    /// <param name="column">The block column, the first screen's sixteen followed by the second's.</param>
    /// <param name="row">The block row, zero or more.</param>
    internal static int CellOf(int column, int row)
    {
        Ensure.BetweenInclusive(0, Screens * ScreenBlocks - 1, column);
        Ensure.BetweenInclusive(0, int.MaxValue / ScreenBlocks / Screens, row);
        int screen = column / ScreenBlocks;
        int written = (screen * ScreenBlocks + row) * ScreenBlocks + column % ScreenBlocks;
        if (written >= Screens * ScreenBlocks * ScreenBlocks)
            return -1;
        int landedScreen = written / (ScreenBlocks * ScreenBlocks);
        int landedRow = written / ScreenBlocks % ScreenBlocks;
        int landedColumn = written % ScreenBlocks;
        if (landedScreen == 0)
            return landedRow * MetatileColumns + landedColumn;
        return landedColumn == 0 ? landedRow * MetatileColumns + ScreenBlocks : -1;
    }
}
