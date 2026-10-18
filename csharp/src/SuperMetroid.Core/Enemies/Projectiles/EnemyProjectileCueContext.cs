using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>What a projectile cue handler acts on: the projectile, Samus, and the camera of the pass running it.</summary>
/// <param name="Projectile">The projectile whose program published the cue.</param>
/// <param name="Samus">Samus, for cues that act on her; null in fixtures without her.</param>
/// <param name="Camera">The camera of the projectile pass, for cues that test the screen.</param>
internal readonly record struct EnemyProjectileCueContext(
    RoomEnemyProjectileSlot Projectile,
    SamusState? Samus,
    Position16<RoomPixels> Camera);
