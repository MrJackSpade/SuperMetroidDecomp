namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Etecoon's look, run, wall-jump, hop and flex animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="EtecoonAnimations"/>.
/// </summary>
internal enum EtecoonSegment
{
    /// <summary>The look right at Samus and run left animation.</summary>
    LookRightAtSamusAndRunLeft,
    /// <summary>The begin running left animation.</summary>
    BeginRunningLeft,
    /// <summary>The running left animation.</summary>
    RunningLeft,
    /// <summary>The wall jump left animation.</summary>
    WallJumpLeft,
    /// <summary>The wall jump left loop animation.</summary>
    WallJumpLeftLoop,
    /// <summary>The hopping facing left animation.</summary>
    HoppingFacingLeft,
    /// <summary>The continue hopping facing left animation.</summary>
    ContinueHoppingFacingLeft,
    /// <summary>The hit ceiling animation.</summary>
    HitCeiling,
    /// <summary>The wall jump left eligible animation.</summary>
    WallJumpLeftEligible,
    /// <summary>The look left at Samus and run right animation.</summary>
    LookLeftAtSamusAndRunRight,
    /// <summary>The begin running right animation.</summary>
    BeginRunningRight,
    /// <summary>The running right animation.</summary>
    RunningRight,
    /// <summary>The wall jump right animation.</summary>
    WallJumpRight,
    /// <summary>The jumping right animation.</summary>
    JumpingRight,
    /// <summary>The hopping facing right animation.</summary>
    HoppingFacingRight,
    /// <summary>The continue hopping facing right animation.</summary>
    ContinueHoppingFacingRight,
    /// <summary>The wall jump right eligible animation.</summary>
    WallJumpRightEligible,
    /// <summary>The initial animation.</summary>
    Initial,
    /// <summary>The flexing animation.</summary>
    Flexing,
    /// <summary>The flexing loop animation.</summary>
    FlexingLoop,
}
