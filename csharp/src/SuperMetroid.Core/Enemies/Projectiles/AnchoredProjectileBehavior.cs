namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile that holds still where its animation places it and returns to <paramref name="anchor"/> whenever its
/// animation says (#2554): the descending Tourian statue's dust.
/// </summary>
/// <param name="anchor">The point the projectile is tied to.</param>
internal sealed class AnchoredProjectileBehavior(Position16<RoomPixels> anchor) : EnemyProjectileBehavior, IAnchoredProjectileBehavior
{
    /// <inheritdoc/>
    public Position16<RoomPixels> Anchor => anchor;
}
