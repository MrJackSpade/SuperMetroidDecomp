using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyDeathsAndDrops"/> that only verification reads.</summary>
internal static class EnemyDeathsAndDropsConstants
{
    /// <summary>Starts the selected pickup's program, 400-frame life and collision callback.</summary>
    internal static void BeginEnemyPickup(EnemyProjectile projectile, EnemyPickupKind kind) =>
        projectile.StartAnimation(new EnemyDeathAndDropAnimation(EnemyDeathsAndDrops.ConfigurePickup(projectile, kind, KilledEnemy.None)));
}
