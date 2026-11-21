using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Ki-Hunter acid spit's typed animation.</summary>
internal sealed partial class KiHunterAcidSpitAnimation
    : EnemyProjectileFrameAnimation<KiHunterAcidSpitSegment, KiHunterAcidSpitCue>
{
    /// <summary>Creates a <see cref="KiHunterAcidSpitAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public KiHunterAcidSpitAnimation(IEnemyProjectileCues<KiHunterAcidSpitCue> owner, KiHunterAcidSpitSegment segment)
        : base(segment, KiHunterAcidSpitAnimations.StartingAt[segment], EnemyProjectileCueRunner<KiHunterAcidSpitSegment, KiHunterAcidSpitCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<KiHunterAcidSpitSegment, EnemyProjectileFrameId, KiHunterAcidSpitCue> Segments => KiHunterAcidSpitAnimations.Segments;
}
