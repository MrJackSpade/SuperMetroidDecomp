using static SuperMetroid.Core.Samus.Abilities.XrayRevealLayout;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The cell of the reveal that each block occupies.</summary>
public static class XrayRevealCells
{
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
