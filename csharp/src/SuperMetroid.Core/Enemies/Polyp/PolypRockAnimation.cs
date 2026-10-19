using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Polyp lava rock's typed animation.</summary>
internal sealed class PolypRockAnimation(PolypRockProgram program)
    : EnemyProjectileProgramAnimation<PolypRockProgram, PolypRockFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<PolypRockProgram, PolypRockFrame, EnemyProjectileCommonCue> Programs => PolypRockAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(PolypRockFrame frame) =>
        PolypRockFrameDefinitions.ShownProjectileFrames[frame];
}
