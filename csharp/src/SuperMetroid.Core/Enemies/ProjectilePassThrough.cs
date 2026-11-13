namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The pass-through reactions of Spark and the Blue Brinstar face block. They never enter common
/// shot AI: they only clear the hit mark on the colliding projectile or bomb, so the beam or missile continues instead
/// of becoming an impact. The shot stage must run before TryStartEnemyImpact, which would turn it into an impact.
/// </summary>
internal static class ProjectilePassThrough
{
    /// <summary>Clears the projectile's spent bit after the super-missile quake.</summary>
    internal static readonly EnemyShotStage Shot = (in EnemyShotContext c) =>
    {
        c.Projectile.IsSpent = false;
        return true;
    };

    /// <summary>Undoes the spent mark the bomb walker installed just before.</summary>
    internal static readonly Action<EnemyBombContext> Bomb =
        c => c.Bomb.IsSpent = false;
}
