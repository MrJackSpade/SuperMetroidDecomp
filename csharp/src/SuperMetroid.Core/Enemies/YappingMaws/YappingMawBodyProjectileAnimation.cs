using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Yapping Maw body link's typed animation.</summary>
internal sealed class YappingMawBodyProjectileAnimation(YappingMawBodyProjectileProgram program)
    : EnemyProjectileTypedAnimation<YappingMawBodyProjectileProgram, YappingMawBodyProjectileFrame, EnemyProjectileCommonCue>(YappingMawBodyProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<YappingMawBodyProjectileProgram, YappingMawBodyProjectileFrame, EnemyProjectileCommonCue> Segments => YappingMawBodyProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<YappingMawBodyProjectileProgram, Animation<YappingMawBodyProjectileProgram>> StartingAt => YappingMawBodyProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(YappingMawBodyProjectileFrame frame) =>
        YappingMawBodyProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
