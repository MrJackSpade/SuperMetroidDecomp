using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Harness;

/// <summary>The projectile and slot of one port projectile.</summary>
/// <param name="Projectile">The projectile.</param>
/// <param name="Slot">The slot.</param>
internal readonly record struct PortProjectile(RoomEnemyProjectileSlot Projectile, int Slot);
