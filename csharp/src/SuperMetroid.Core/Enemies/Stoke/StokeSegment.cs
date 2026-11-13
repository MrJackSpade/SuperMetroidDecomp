namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// Stoke's walking and fire-spitting animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="StokeAnimations"/>.
/// </summary>
internal enum StokeSegment
{
    /// <summary>The moving left animation.</summary>
    MovingLeft,
    /// <summary>The moving left loop animation.</summary>
    MovingLeftLoop,
    /// <summary>The attacking left animation.</summary>
    AttackingLeft,
    /// <summary>The moving right animation.</summary>
    MovingRight,
    /// <summary>The moving right loop animation.</summary>
    MovingRightLoop,
    /// <summary>The attacking right animation.</summary>
    AttackingRight,
}
