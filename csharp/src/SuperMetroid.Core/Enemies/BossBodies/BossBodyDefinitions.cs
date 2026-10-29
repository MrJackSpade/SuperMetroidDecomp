namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The shape of a boss body layer (#1466): its eight-pixel cells, thirty-two to a row, and its pages of thirty-two rows,
/// which the layer's arrangement lays side by side or one above another.
/// </summary>
internal static class BossBodyDefinitions
{
    /// <summary>Cells across a boss body layer, and across every part image.</summary>
    internal const int ColumnCount = 32;

    /// <summary>Pixels across and down one cell.</summary>
    internal const int CellPixels = 8;

    /// <summary>Rows of one page of a boss body layer.</summary>
    internal const int PageRows = 32;

    /// <summary>
    /// Rows of the body layer Phantoon, Draygon, Crocomire and Mother Brain draw into: two pages, which their room shows
    /// side by side, one above another, or only the first.
    /// </summary>
    internal const int SharedBodyRows = 64;
}
