namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>
/// Owtch's left and right shell-cycling animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="OwtchAnimations"/>.
/// </summary>
internal enum OwtchSegment
{
    /// <summary>The moving left animation.</summary>
    MovingLeft,
    /// <summary>The moving left loop animation.</summary>
    MovingLeftLoop,
    /// <summary>The moving right animation.</summary>
    MovingRight,
    /// <summary>The moving right loop animation.</summary>
    MovingRightLoop,
}
