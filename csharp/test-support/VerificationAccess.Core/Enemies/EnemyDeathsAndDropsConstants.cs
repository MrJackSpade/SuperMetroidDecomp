using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyDeathsAndDrops"/> that only verification reads.</summary>
internal static class EnemyDeathsAndDropsConstants
{
    /// <summary>Starts the selected pickup's program, 400-frame life and collision callback, run by <paramref name="drops"/>.</summary>
    /// <param name="drops">The room's deaths and drops, the pickup's behaviour.</param>
    /// <param name="projectile">The pickup.</param>
    /// <param name="kind">What the pickup restores.</param>
    internal static void BeginEnemyPickup(EnemyDeathsAndDrops drops, EnemyProjectile projectile, EnemyPickupKind kind) =>
        projectile.StartAnimation(new EnemyDeathAndDropAnimation(drops, drops.ConfigurePickup(projectile, kind)));
}
