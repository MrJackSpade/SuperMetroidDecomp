namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// What a Magdollite's tracking overlay, its hand, is doing. Its values are arbitrary.
/// </summary>
public enum MagdolliteOverlayAction
{
    /// <summary>Waits for the head to begin forming the base pillar before tracking arm growth.</summary>
    WaitingForAttack,
    /// <summary>Waits for the throw animation to finish, restores the captured hand position, and starts arm shrinkage.</summary>
    WaitingForThrowAnimation,
    /// <summary>Follows the growing arm cap and, at the apex, selects a Samus-facing throw animation and captures its origin.</summary>
    TrackingRisingBody,
    /// <summary>Follows the shrinking arm cap until the head returns to idle.</summary>
    TrackingFallingBody,
}
