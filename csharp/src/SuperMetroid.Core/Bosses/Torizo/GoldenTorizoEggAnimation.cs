using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>A typed animation of Golden Torizo's eggs and their hatchlings (#2554), starting at one of its segments.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class GoldenTorizoEggAnimation(GoldenTorizoEggSegment segment, IEnemyProjectileBranchingCues<GoldenTorizoEggSegment, TorizoFamilyCue<GoldenTorizoEggCueKind>> cues)
    : EnemyProjectileFrameAnimation<GoldenTorizoEggSegment, TorizoFamilyCue<GoldenTorizoEggCueKind>>(GoldenTorizoEggAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<GoldenTorizoEggSegment, EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>> Segments => GoldenTorizoEggAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<GoldenTorizoEggSegment, Animation<GoldenTorizoEggSegment>> StartingAt => GoldenTorizoEggAnimations.StartingAt;
}
