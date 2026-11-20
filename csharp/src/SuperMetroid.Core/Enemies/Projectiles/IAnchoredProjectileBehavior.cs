namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The behaviour of a projectile that stays tied to the point it was spawned at (#2554): the descending Tourian statue's
/// dust returns to it whenever its animation restarts a puff.
/// </summary>
internal interface IAnchoredProjectileBehavior
{
    /// <summary>The point the projectile is tied to.</summary>
    Position16<RoomPixels> Anchor { get; }
}
