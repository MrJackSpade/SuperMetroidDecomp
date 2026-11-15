namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's figure-eight (#1933): where on its path he is, which way he runs it, and how fast. Each pattern that starts
/// a figure-eight places him on the path and sets its direction and speed before the first step reads them.
/// </summary>
public sealed class PhantoonFigureEightState
{
    /// <summary>His place on the figure-eight path, zero through the path's last place.</summary>
    public int PathCursor { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether he runs the path backwards.</summary>
    public bool Reversed { get; internal set; }

    /// <summary>
    /// His speed in path places per update: zero or more forward, zero or less while he runs the path backwards, and
    /// never more than seven places from rest.
    /// </summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>The stage of the speed change he is in.</summary>
    public PhantoonFigureEightStage Stage { get; internal set; }
}
