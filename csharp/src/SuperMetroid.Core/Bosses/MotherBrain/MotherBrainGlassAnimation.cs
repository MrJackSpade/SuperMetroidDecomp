using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A glass shard's or sparkle's typed animation.</summary>
/// <param name="program">The program to play.</param>
internal sealed class MotherBrainGlassAnimation(MotherBrainGlassSegment program)
    : EnemyProjectileTypedAnimation<MotherBrainGlassSegment, MotherBrainGlassFrame, MotherBrainGlassCue>(MotherBrainGlassAnimations.StartingAt[program])
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainGlassSegment, MotherBrainGlassFrame, MotherBrainGlassCue> Segments =>
        MotherBrainGlassAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainGlassSegment, Animation<MotherBrainGlassSegment>> StartingAt =>
        MotherBrainGlassAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MotherBrainGlassFrame frame) =>
        MotherBrainGlassVisualDefinitions.ShownProjectileFrames[frame];
}
