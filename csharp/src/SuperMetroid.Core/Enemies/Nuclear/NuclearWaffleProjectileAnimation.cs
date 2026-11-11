using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Nuclear Waffle body's typed animation.</summary>
internal sealed class NuclearWaffleProjectileAnimation(NuclearWaffleProjectileProgram program)
    : EnemyProjectileTypedAnimation<NuclearWaffleProjectileProgram, NuclearWaffleProjectileFrame, EnemyProjectileCommonCue>(NuclearWaffleProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<NuclearWaffleProjectileProgram, NuclearWaffleProjectileFrame, EnemyProjectileCommonCue> Segments => NuclearWaffleProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<NuclearWaffleProjectileProgram, Animation<NuclearWaffleProjectileProgram>> StartingAt => NuclearWaffleProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(NuclearWaffleProjectileFrame frame) =>
        NuclearWaffleProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
