using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Ki-Hunter acid spit's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class KiHunterAcidSpitAnimation(KiHunterAcidSpitSegment segment, IEnemyProjectileCues<KiHunterAcidSpitCue> cues)
    : EnemyProjectileFrameAnimation<KiHunterAcidSpitSegment, KiHunterAcidSpitCue>(KiHunterAcidSpitAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KiHunterAcidSpitSegment, EnemyProjectileFrameId, KiHunterAcidSpitCue> Segments => KiHunterAcidSpitAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KiHunterAcidSpitSegment, Animation<KiHunterAcidSpitSegment>> StartingAt => KiHunterAcidSpitAnimations.StartingAt;
}
