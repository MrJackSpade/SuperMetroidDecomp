using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Polyp lava rock's typed animation.</summary>
internal sealed class PolypRockAnimation(PolypRockProgram program)
    : EnemyProjectileTypedAnimation<PolypRockProgram, PolypRockFrame, EnemyProjectileCommonCue>(PolypRockAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PolypRockProgram, PolypRockFrame, EnemyProjectileCommonCue> Segments => PolypRockAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PolypRockProgram, Animation<PolypRockProgram>> StartingAt => PolypRockAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(PolypRockFrame frame) =>
        PolypRockFrameDefinitions.ShownProjectileFrames[frame];
}
