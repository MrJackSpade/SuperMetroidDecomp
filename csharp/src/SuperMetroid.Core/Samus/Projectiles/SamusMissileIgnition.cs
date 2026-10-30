namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where a missile or Super Missile is in its ignition. A fired or reflected missile coasts until its first update,
/// then ignites: its velocity is rebuilt from its ignition speed and Samus's inherited motion, and only then do the
/// missile accelerations and a Super Missile's collision link begin.
/// </summary>
public enum SamusMissileIgnition
{
    /// <summary>Fired and not yet ignited; it ignites at one pixel per update.</summary>
    Pending,
    /// <summary>Reflected by an enemy and not yet re-ignited; it re-ignites with the reflection's head start.</summary>
    PendingAfterReflection,
    /// <summary>Ignited.</summary>
    Ignited,
}
