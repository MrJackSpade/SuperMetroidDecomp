using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Everything a bank-$86 pre-instruction receives for one projectile in one projectile pass.</summary>
internal readonly record struct EnemyProjectilePreInstructionContext(
    RoomEnemyProjectileSlot Projectile,
    RoomLevelData Level,
    SamusState? Samus,
    Position16<RoomPixels> Camera,
    byte NmiFrameCounter8,
    SamusBombProjectileSystem? SamusBombs);
