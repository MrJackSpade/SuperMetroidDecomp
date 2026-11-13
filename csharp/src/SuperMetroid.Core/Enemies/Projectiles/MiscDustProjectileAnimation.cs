using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Misc dust's typed animation.</summary>
internal sealed class MiscDustProjectileAnimation(MiscDustProjectileSegment segment)
    : EnemyProjectileFrameAnimation<MiscDustProjectileSegment, EnemyProjectileCommonCue>(MiscDustProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MiscDustProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => MiscDustProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MiscDustProjectileSegment, Animation<MiscDustProjectileSegment>> StartingAt => MiscDustProjectileAnimations.StartingAt;
}
