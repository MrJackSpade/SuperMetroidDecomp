using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>What a projectile cue handler acts on: the projectile, Samus, and the camera and short frame counter of the pass running it.</summary>
/// <param name="Projectile">The projectile whose animation published the cue.</param>
/// <param name="Samus">Samus, for cues that act on her; null in fixtures without her.</param>
/// <param name="Camera">The camera of the projectile pass, for cues that test the screen.</param>
/// <param name="ShortCounter">The game's short frame counter for the pass, 0 through 255, which cue cadences sample.</param>
internal readonly record struct EnemyProjectileCueContext(
    EnemyProjectile Projectile,
    SamusState? Samus,
    Position16<RoomPixels> Camera,
    int ShortCounter);
