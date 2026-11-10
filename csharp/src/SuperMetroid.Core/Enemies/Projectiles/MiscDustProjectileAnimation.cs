using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Misc dust's typed animation.</summary>
internal sealed class MiscDustProjectileAnimation(MiscDustProjectileProgram program)
    : EnemyProjectileTypedAnimation<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue>(MiscDustProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue> Segments => MiscDustProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MiscDustProjectileProgram, Animation<MiscDustProjectileProgram>> StartingAt => MiscDustProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MiscDustProjectileFrame frame) =>
        MiscDustProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
