using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Ki-Hunter acid spit's typed animation.</summary>
internal sealed class KiHunterAcidSpitAnimation(KiHunterAcidSpitProgram program)
    : EnemyProjectileFrameAnimation<KiHunterAcidSpitProgram, EnemyProjectileCommonCue>(KiHunterAcidSpitAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KiHunterAcidSpitProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => KiHunterAcidSpitAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KiHunterAcidSpitProgram, Animation<KiHunterAcidSpitProgram>> StartingAt => KiHunterAcidSpitAnimations.StartingAt;
}
