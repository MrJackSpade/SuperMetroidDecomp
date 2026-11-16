namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One pattern of Phantoon's flame rain (#2258): where his body reappears as the rain falls, and so where the rain leaves
/// its one gap.
/// </summary>
/// <param name="PathCursor">The place on his figure-eight path his body resumes from.</param>
/// <param name="BodyPosition">His body's whole-pixel room position.</param>
public sealed record PhantoonRainPattern(int PathCursor, Position16<RoomPixels> BodyPosition)
{
    /// <summary>
    /// The rain column of its first flame: the column after the one his body stands in. The rain's eight flames take the
    /// columns from there on, wrapping from the last to the first, so the one column left empty is his body's.
    /// </summary>
    public int FirstColumn
    {
        get
        {
            int origin = PhantoonFlameSpawns.RainX(0);
            int spacing = PhantoonFlameSpawns.RainX(1) - origin;
            int bodyColumn = (BodyPosition.X - origin) / spacing;
            return (bodyColumn + 1) % PhantoonPatternDefinitions.RainColumnCount;
        }
    }
}
