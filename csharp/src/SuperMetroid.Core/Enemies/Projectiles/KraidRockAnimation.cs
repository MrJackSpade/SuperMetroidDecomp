using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>A Kraid rock's (or the Kago bug's initial) typed animation.</summary>
/// <param name="segment">The program to play.</param>
internal sealed class KraidRockAnimation(KraidRockSegment segment)
    : EnemyProjectileTypedAnimation<KraidRockSegment, KraidRockFrame, KraidRockCue>(KraidRockAnimations.StartingWith(segment))
{

    /// <inheritdoc/>
    private protected override AnimationSegments<KraidRockSegment, KraidRockFrame, KraidRockCue> Segments => KraidRockAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KraidRockSegment, Animation<KraidRockSegment>> StartingAt => KraidRockAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(KraidRockFrame frame) =>
        KraidRockVisualDefinitions.ShownProjectileFrames[frame];
}
