using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Everything a pre-instruction receives for one projectile in one projectile pass.</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct EnemyProjectileFrameBehaviorContext(
    RoomEnemyProjectileSlot Projectile,
    RoomLevelData Level,
    SamusState? Samus,
    Position16<RoomPixels> Camera,
    int ShortCounter,
    SamusBombProjectileSystem? SamusBombs);
