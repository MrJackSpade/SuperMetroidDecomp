using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Stoke.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>Stoke projectile's typed animation.</summary>
internal sealed class StokeProjectileAnimation(StokeProjectileProgram program)
    : EnemyProjectileTypedAnimation<StokeProjectileProgram, StokeProjectileFrame, EnemyProjectileCommonCue>(StokeProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<StokeProjectileProgram, StokeProjectileFrame, EnemyProjectileCommonCue> Segments => StokeProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<StokeProjectileProgram, Animation<StokeProjectileProgram>> StartingAt => StokeProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(StokeProjectileFrame frame) =>
        StokeProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
