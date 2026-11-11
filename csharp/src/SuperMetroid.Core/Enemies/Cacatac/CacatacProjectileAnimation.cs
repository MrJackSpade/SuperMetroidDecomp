using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Cacatac spike's typed animation.</summary>
internal sealed class CacatacProjectileAnimation(CacatacProjectileProgram program)
    : EnemyProjectileTypedAnimation<CacatacProjectileProgram, CacatacProjectileFrame, EnemyProjectileCommonCue>(CacatacProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<CacatacProjectileProgram, CacatacProjectileFrame, EnemyProjectileCommonCue> Segments => CacatacProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<CacatacProjectileProgram, Animation<CacatacProjectileProgram>> StartingAt => CacatacProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(CacatacProjectileFrame frame) =>
        CacatacProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
