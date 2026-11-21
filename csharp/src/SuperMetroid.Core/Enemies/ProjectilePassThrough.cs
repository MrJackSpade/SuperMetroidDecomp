namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The pass-through reactions of Spark and the Blue Brinstar face block. A shot deals them no damage: the colliding
/// projectile or bomb flies on as though it had struck nothing (#2562), so the beam or missile continues instead of becoming
/// an impact, even a shot an earlier collision this update spent, as the game's reaction does. The shot stage must run
/// before <see cref="Samus.Projectiles.ProjectileEnemyImpacts.TryStartEnemyImpact"/>, which would turn it into an impact.
/// </summary>
internal static partial class ProjectilePassThrough
{
    /// <summary>
    /// Has the projectile fly on (<see cref="Samus.Projectiles.SamusProjectileOccupant.FlyOn"/>) after the super-missile quake.
    /// </summary>
    internal static readonly EnemyShotStage Shot = (in EnemyShotContext c) =>
    {
        c.Shot.FlyOn();
        return true;
    };

    /// <summary>Has the bomb fly on (<see cref="Samus.Projectiles.SamusBombOccupant.FlyOn"/>) past the enemy its collision met.</summary>
    internal static readonly Action<EnemyBombContext> Bomb =
        c => c.PlacedBomb.FlyOn();
}
