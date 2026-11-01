namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's vertical direction (<c>SamusYDirection</c>). The game
/// stores vertical speed as a magnitude and keeps its sign here. Every writer
/// stores only these three values: zero, one
/// and two.
/// </summary>
public enum SamusVerticalDirection
{
    /// <summary>Zero: no vertical movement (grounded, or a pose change that cleared the motion).</summary>
    None = 0,

    /// <summary>One: moving up (jumping, bouncing, rising).</summary>
    Up = 1,

    /// <summary>Two: moving down (falling).</summary>
    Down = 2,
}
