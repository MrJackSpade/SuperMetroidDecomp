using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyDeathsAndDrops"/> that only verification reads.</summary>
internal static class EnemyDeathsAndDropsConstants
{
    /// <summary>Starts the selected pickup's program, 400-frame life and collision callback.</summary>
    internal static void BeginEnemyPickup(RoomEnemyProjectileSlot projectile, EnemyPickupKind kind) =>
        projectile.StartAnimation(new EnemyDeathProjectileAnimation(EnemyDeathsAndDrops.ConfigurePickup(projectile, kind)));
}
