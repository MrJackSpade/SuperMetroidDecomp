using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Ki-Hunter acid spit's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class KiHunterAcidSpitAnimation(IEnemyProjectileCues<KiHunterAcidSpitCue> owner, KiHunterAcidSpitSegment segment)
    : EnemyProjectileFrameAnimation<KiHunterAcidSpitSegment, KiHunterAcidSpitCue>(KiHunterAcidSpitAnimations.StartingWith(segment), EnemyProjectileCueRunner<KiHunterAcidSpitSegment, KiHunterAcidSpitCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KiHunterAcidSpitSegment, EnemyProjectileFrameId, KiHunterAcidSpitCue> Segments => KiHunterAcidSpitAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<KiHunterAcidSpitSegment, Animation<KiHunterAcidSpitSegment>>> StartingAt => KiHunterAcidSpitAnimations.StartingAt;
}
