namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile that stays tied to the point it was spawned at: a Tourian statue's decorations follow the statue from
/// it, and the descending statue's dust returns to it whenever its program restarts a puff.
/// </summary>
/// <param name="anchor">The point the projectile is tied to.</param>
internal sealed class AnchoredProjectileState(Position16<RoomPixels> anchor) : EnemyProjectileState
{
    /// <summary>The point the projectile is tied to.</summary>
    internal Position16<RoomPixels> Anchor { get; } = anchor;
}
