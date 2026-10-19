using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Ki-Hunter acid spit's typed animation.</summary>
internal sealed class KiHunterAcidSpitAnimation(KiHunterAcidSpitProgram program)
    : EnemyProjectileProgramAnimation<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue> Programs => KiHunterAcidSpitAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(KiHunterAcidSpitFrame frame) =>
        KiHunterAcidSpitFrameDefinitions.ShownProjectileFrames[frame];
}
