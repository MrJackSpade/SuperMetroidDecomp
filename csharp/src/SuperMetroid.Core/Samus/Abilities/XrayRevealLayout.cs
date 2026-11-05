namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The area the X-ray reveals (#1431, #1514): the room's 16x16 blocks covering the screen from the block its top-left
/// corner lies in, written in whole blocks by the reveal and its overlays.
/// </summary>
public static class XrayRevealLayout
{
    /// <summary>Rows of blocks revealed: every row the gameplay area shows, at any scroll within a block.</summary>
    public const int Rows = 16;
    /// <summary>Columns of blocks revealed: the screen's sixteen and the one a scroll within a block also shows.</summary>
    public const int Columns = 17;
    /// <summary>The blocks revealed, row by row, <see cref="Columns"/> to a row.</summary>
    public const int Cells = Columns * Rows;
}
