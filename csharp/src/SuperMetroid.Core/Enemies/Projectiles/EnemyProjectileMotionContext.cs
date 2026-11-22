using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Everything a projectile's motion receives for one projectile in one projectile pass.</summary>
/// <param name="Projectile">The projectile whose motion runs.</param>
/// <param name="Level">The room's blocks, which a moving projectile collides with.</param>
/// <param name="Samus">Samus, for behaviours that act on her.</param>
/// <param name="Camera">The camera of the pass, for behaviours that test the screen.</param>
/// <param name="ShortCounter">The enemies' short update count for the pass, which cadences sample.</param>
/// <param name="FrameCounter">The game's update count for the pass, which cadences sample.</param>
/// <param name="SamusBombs">Samus's bombs, for behaviours that react to them.</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly partial record struct EnemyProjectileMotionContext(
    EnemyProjectile Projectile,
    RoomLevelData Level,
    SamusState Samus,
    Position16<RoomPixels> Camera,
    Compatibility.ShortFrameCount ShortCounter,
    int FrameCounter,
    SamusBombProjectileSystem SamusBombs);
