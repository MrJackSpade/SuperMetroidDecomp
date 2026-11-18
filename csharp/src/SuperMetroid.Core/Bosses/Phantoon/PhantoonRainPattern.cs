namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One pattern of Phantoon's flame rain (#2258): where his body reappears as the rain falls, and so where the rain leaves
/// its one gap.
/// </summary>
/// <param name="PathPlace">The place on his figure-eight path his body resumes from (<see cref="PhantoonPath.Places"/>).</param>
/// <param name="BodyPosition">His body's whole-pixel room position.</param>
public sealed record PhantoonRainPattern(int PathPlace, Position16<RoomPixels> BodyPosition)
{
    /// <summary>
    /// The rain column of its first flame: the column after the one his body stands in. The rain's eight flames take the
    /// columns from there on, the first following the last, so the one column left empty is his body's.
    /// </summary>
    public ListStep<int> FirstColumn
    {
        get
        {
            IReadOnlyList<int> columns = PhantoonFlameSpawns.RainXs;
            int spacing = columns[1] - columns[0];
            return ListStep<int>.At(columns, (BodyPosition.X - columns[0]) / spacing).NextInCycle;
        }
    }
}
