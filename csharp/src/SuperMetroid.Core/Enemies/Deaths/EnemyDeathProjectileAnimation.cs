using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Deaths.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>Enemy death and drop's typed animation.</summary>
internal sealed class EnemyDeathProjectileAnimation(EnemyDeathProjectileProgram program)
    : EnemyProjectileTypedAnimation<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue>(EnemyDeathProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EnemyDeathProjectileProgram, EnemyDeathProjectileFrame, EnemyDeathProjectileCue> Segments => EnemyDeathProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EnemyDeathProjectileProgram, Animation<EnemyDeathProjectileProgram>> StartingAt => EnemyDeathProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(EnemyDeathProjectileFrame frame) =>
        EnemyDeathProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
