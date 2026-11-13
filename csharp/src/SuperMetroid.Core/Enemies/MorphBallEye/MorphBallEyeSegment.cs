namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The Morph Ball eye's tracking, eyelid and mount animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="MorphBallEyeAnimations"/>.
/// </summary>
internal enum MorphBallEyeSegment
{
    /// <summary>The active animation.</summary>
    Active,
    /// <summary>The facing right deactivating animation.</summary>
    FacingRightDeactivating,
    /// <summary>The facing right closed animation.</summary>
    FacingRightClosed,
    /// <summary>The facing left deactivating animation.</summary>
    FacingLeftDeactivating,
    /// <summary>The facing left closed animation.</summary>
    FacingLeftClosed,
    /// <summary>The facing right activating animation.</summary>
    FacingRightActivating,
    /// <summary>The facing left activating animation.</summary>
    FacingLeftActivating,
    /// <summary>The mount facing right animation.</summary>
    MountFacingRight,
    /// <summary>The mount facing down animation.</summary>
    MountFacingDown,
    /// <summary>The mount facing left animation.</summary>
    MountFacingLeft,
    /// <summary>The mount facing up animation.</summary>
    MountFacingUp,
}
