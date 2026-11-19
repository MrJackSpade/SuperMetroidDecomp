namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One pattern of Phantoon's flame rain (#2258): where his body reappears as the rain falls, and so where the rain leaves
/// its one gap.
/// </summary>
/// <param name="PathPlace">The place on his figure-eight path his body resumes from (<c>Places</c>).</param>
/// <param name="BodyPosition">His body's whole-pixel room position.</param>
/// <param name="FirstColumn">
/// The rain column of its first flame (#2471, <c>RainXs</c>): the column after the one his body
/// stands in, the first after the last. The rain's eight flames take the columns from there on, the first following the
/// last, so the one column left empty is his body's.
/// </param>
public sealed record PhantoonRainPattern(int PathPlace, Position16<RoomPixels> BodyPosition, ListStep<int> FirstColumn);
