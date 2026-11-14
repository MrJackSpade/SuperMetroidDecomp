namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's idle, attacking and rolling animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="MaridiaLargeSnailAnimations"/>.
/// </summary>
public enum MaridiaLargeSnailSegment
{
    /// <summary>The facing left idle animation.</summary>
    FacingLeftIdle,
    /// <summary>The facing left attacking animation.</summary>
    FacingLeftAttacking,
    /// <summary>The facing left rolling forwards animation.</summary>
    FacingLeftRollingForwards,
    /// <summary>The facing left rolling backwards animation.</summary>
    FacingLeftRollingBackwards,
    /// <summary>The facing right idle animation.</summary>
    FacingRightIdle,
    /// <summary>The facing right attacking animation.</summary>
    FacingRightAttacking,
    /// <summary>The facing right rolling forwards animation.</summary>
    FacingRightRollingForwards,
    /// <summary>The facing right rolling backwards animation.</summary>
    FacingRightRollingBackwards,
}
