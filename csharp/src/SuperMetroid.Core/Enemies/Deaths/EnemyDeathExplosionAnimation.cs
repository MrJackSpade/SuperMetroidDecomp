using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// An enemy death explosion's typed animation (#2061, #2554), started by the explosion
/// (<see cref="EnemyDeathExplosionAnimations.Explosions"/>): its cue to become its drop hands the projectile's play over to
/// the drop's own animation within the same pass.
/// </summary>
/// <param name="animation">The animation.</param>
/// <param name="cues">The explosion's behaviour, which owns its cues.</param>
internal sealed class EnemyDeathExplosionAnimation(Animation<EnemyDeathExplosionSegment> animation, IEnemyProjectileHandingOverCues<EnemyDeathExplosionCue> cues)
    : EnemyProjectileFrameAnimation<EnemyDeathExplosionSegment, EnemyDeathExplosionCue>(animation, cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EnemyDeathExplosionSegment, EnemyProjectileFrameId, EnemyDeathExplosionCue> Segments => EnemyDeathExplosionAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EnemyDeathExplosionSegment, Animation<EnemyDeathExplosionSegment>> StartingAt => EnemyDeathExplosionAnimations.StartingAt;
}
