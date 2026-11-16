using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Misc dust's typed animation.</summary>
internal sealed class MiscDustProjectileAnimation(MiscDustAnimation animation)
    : EnemyProjectileFrameAnimation<MiscDustAnimation, EnemyProjectileCommonCue>(MiscDustProjectileAnimations.StartingWith(animation))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MiscDustAnimation, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => MiscDustProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MiscDustAnimation, Animation<MiscDustAnimation>> StartingAt => MiscDustProjectileAnimations.StartingAt;
}
