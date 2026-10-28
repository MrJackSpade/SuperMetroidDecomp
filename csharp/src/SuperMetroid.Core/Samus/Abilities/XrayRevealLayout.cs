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
    internal const int ScreenBlocks = 16;
    /// <summary>The screens the reveal writes.</summary>
    internal const int Screens = 2;
}
