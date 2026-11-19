namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The pass-through reactions of Spark and the Blue Brinstar face block. A shot deals them no damage: they only clear the
/// colliding projectile's or bomb's spent mark, so the beam or missile continues instead of becoming an impact. The shot
/// stage must run before <see cref="Samus.Projectiles.ProjectileEnemyImpacts.TryStartEnemyImpact"/>, which would turn it
/// into an impact.
/// </summary>
internal static class ProjectilePassThrough
{
    /// <summary>
    /// Clears the projectile's spent mark (<see cref="Samus.Projectiles.SamusProjectile.IsSpent"/>) after the super-missile
    /// quake.
    /// </summary>
    internal static readonly EnemyShotStage Shot = (in EnemyShotContext c) =>
    {
        c.Projectile.IsSpent = false;
        return true;
    };

    /// <summary>Undoes the spent mark (<see cref="Samus.Projectiles.SamusBomb.IsSpent"/>) the bomb's collision set just before.</summary>
    internal static readonly Action<EnemyBombContext> Bomb =
        c => c.Bomb.IsSpent = false;
}
