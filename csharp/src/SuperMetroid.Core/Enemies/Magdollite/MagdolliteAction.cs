namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The behaviour Magdollite's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum MagdolliteAction
{
    /// <summary>Faces Samus and submerges when the hand cooldown has expired and Samus is within the configured horizontal range.</summary>
    HeadWaiting,
    /// <summary>Waits for the head's submerge animation to finish, then releases the arm to grow.</summary>
    HeadWaitingForAttackAnimation,
    /// <summary>Waits for the arm to land, then selects the facing-dependent head emergence animation.</summary>
    HeadWaitingForBodyLanding,
    /// <summary>Waits for emergence to finish before returning the head to idle.</summary>
    HeadWaitingForReturnAnimation,
    /// <summary>Holds the arm dormant until the head clears its not-emerged flag, then initializes growth at its spawn Y.</summary>
    BodyDormant,
    /// <summary>Adds the preceding head's upward 1/65536-pixel velocity and extends pillar segments until reaching Samus's height or the maximum rise.</summary>
    BodyRising,
    /// <summary>Index-load/return stub that holds the grown arm stationary while the hand throws.</summary>
    NoOpAtApex,
    /// <summary>Adds the head's downward 1/65536-pixel velocity, removes pillar segments, and releases the head when the arm lands.</summary>
    BodyFalling,
    /// <summary>Waits for the head to begin forming the base pillar before tracking arm growth.</summary>
    OverlayWaitingForAttack,
    /// <summary>Waits for the throw animation to finish, restores the captured hand position, and starts arm shrinkage.</summary>
    OverlayWaitingForThrowAnimation,
    /// <summary>Follows the growing arm cap and, at the apex, selects a Samus-facing throw animation and captures its origin.</summary>
    OverlayTrackingRisingBody,
    /// <summary>Follows the shrinking arm cap until the head returns to idle.</summary>
    OverlayTrackingFallingBody,
}
