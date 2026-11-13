using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>A Kraid rock's (or the Kago bug's initial) typed animation.</summary>
/// <param name="program">The program to play.</param>
internal sealed class KraidRockAnimation(KraidRockSegment program)
    : EnemyProjectileTypedAnimation<KraidRockSegment, KraidRockFrame, KraidRockCue>(KraidRockAnimations.StartingWith(program))
{

    /// <inheritdoc/>
    private protected override AnimationSegments<KraidRockSegment, KraidRockFrame, KraidRockCue> Segments => KraidRockAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KraidRockSegment, Animation<KraidRockSegment>> StartingAt => KraidRockAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(KraidRockFrame frame) =>
        KraidRockVisualDefinitions.ShownProjectileFrames[frame];
}
