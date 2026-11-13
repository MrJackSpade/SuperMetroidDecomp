using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Ki-Hunter acid spit's typed animation.</summary>
internal sealed class KiHunterAcidSpitAnimation(KiHunterAcidSpitSegment segment)
    : EnemyProjectileFrameAnimation<KiHunterAcidSpitSegment, EnemyProjectileCommonCue>(KiHunterAcidSpitAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KiHunterAcidSpitSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => KiHunterAcidSpitAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KiHunterAcidSpitSegment, Animation<KiHunterAcidSpitSegment>> StartingAt => KiHunterAcidSpitAnimations.StartingAt;
}
