using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// An enemy drop's typed animation (#2554): a pickup's or the respawn pause's, which start at their segments; a death
/// explosion's own animation hands its projectile over to one as it becomes its drop.
/// </summary>
/// <param name="animation">The animation.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class EnemyDeathAndDropAnimation(Animation<EnemyDeathAndDropSegment> animation, IEnemyProjectileCues<EnemyDeathAndDropCue> cues)
    : EnemyProjectileFrameAnimation<EnemyDeathAndDropSegment, EnemyDeathAndDropCue>(animation, cues)
{
    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal EnemyDeathAndDropAnimation(EnemyDeathAndDropSegment segment, IEnemyProjectileCues<EnemyDeathAndDropCue> cues)
        : this(EnemyDeathAndDropAnimations.StartingWith(segment), cues)
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<EnemyDeathAndDropSegment, EnemyProjectileFrameId, EnemyDeathAndDropCue> Segments => EnemyDeathAndDropAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EnemyDeathAndDropSegment, Animation<EnemyDeathAndDropSegment>> StartingAt => EnemyDeathAndDropAnimations.StartingAt;
}
