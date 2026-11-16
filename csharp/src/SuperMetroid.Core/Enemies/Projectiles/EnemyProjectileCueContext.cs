using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a projectile cue acts on: the projectile, Samus, the camera and short frame counter of the pass running it, and
/// Samus's bombs.
/// </summary>
/// <param name="Projectile">The projectile whose animation published the cue.</param>
/// <param name="Samus">Samus, for cues that act on her and for the drops a cue spawns (#2159).</param>
/// <param name="Camera">The camera of the projectile pass, for cues that test the screen.</param>
/// <param name="ShortCounter">The game's short frame counter for the pass, 0 through 255, which cue cadences sample.</param>
/// <param name="SamusBombs">Samus's bombs, whose power bomb explosion a cue may test (#2159).</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct EnemyProjectileCueContext(
    EnemyProjectile Projectile,
    SamusState Samus,
    Position16<RoomPixels> Camera,
    int ShortCounter,
    SamusBombProjectileSystem SamusBombs);
