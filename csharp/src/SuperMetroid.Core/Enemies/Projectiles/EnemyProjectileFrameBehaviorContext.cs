using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Everything a frame behaviour receives for one projectile in one projectile pass.</summary>
/// <param name="Projectile">The projectile whose frame behaviour runs.</param>
/// <param name="Level">The room's blocks, which a moving projectile collides with.</param>
/// <param name="Samus">Samus, for behaviours that act on her; null in fixtures without her.</param>
/// <param name="Camera">The camera of the pass, for behaviours that test the screen.</param>
/// <param name="ShortCounter">The game's short frame counter for the pass, 0 through 255, which cadences sample.</param>
/// <param name="FrameCounter">The game's long frame counter for the pass, 0 through 65535, which cadences sample.</param>
/// <param name="SamusBombs">Samus's bombs, for behaviours that react to them.</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct EnemyProjectileFrameBehaviorContext(
    EnemyProjectile Projectile,
    RoomLevelData Level,
    SamusState? Samus,
    Position16<RoomPixels> Camera,
    int ShortCounter,
    int FrameCounter,
    SamusBombProjectileSystem? SamusBombs);
