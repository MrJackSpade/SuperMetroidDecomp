using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Ki-Hunter acid spit's typed animation.</summary>
internal sealed class KiHunterAcidSpitAnimation(KiHunterAcidSpitProgram program)
    : EnemyProjectileTypedAnimation<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue>(KiHunterAcidSpitAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue> Segments => KiHunterAcidSpitAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KiHunterAcidSpitProgram, Animation<KiHunterAcidSpitProgram>> StartingAt => KiHunterAcidSpitAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(KiHunterAcidSpitFrame frame) =>
        KiHunterAcidSpitFrameDefinitions.ShownProjectileFrames[frame];
}
