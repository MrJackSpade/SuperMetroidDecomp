using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A glass shard's or sparkle's typed animation.</summary>
/// <param name="segment">The animation to play.</param>
internal sealed class MotherBrainGlassAnimation(MotherBrainGlassSegment segment)
    : EnemyProjectileFrameAnimation<MotherBrainGlassSegment, MotherBrainGlassCue>(MotherBrainGlassAnimations.StartingAt[segment])
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainGlassSegment, EnemyProjectileFrameId, MotherBrainGlassCue> Segments =>
        MotherBrainGlassAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainGlassSegment, Animation<MotherBrainGlassSegment>> StartingAt =>
        MotherBrainGlassAnimations.StartingAt;
}
