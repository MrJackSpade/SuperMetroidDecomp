using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Yapping Maw body link's typed animation.</summary>
internal sealed class YappingMawBodyProjectileAnimation(YappingMawBodyProjectileProgram program)
    : EnemyProjectileProgramAnimation<YappingMawBodyProjectileProgram, YappingMawBodyProjectileFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<YappingMawBodyProjectileProgram, YappingMawBodyProjectileFrame, EnemyProjectileCommonCue> Programs => YappingMawBodyProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(YappingMawBodyProjectileFrame frame) =>
        EnemyProjectileFramePresentation.Installed(YappingMawBodyProjectileFrameDefinitions.PresentationKey(frame));
}
