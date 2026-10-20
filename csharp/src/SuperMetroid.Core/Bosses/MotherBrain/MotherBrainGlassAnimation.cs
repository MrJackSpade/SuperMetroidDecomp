using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A glass shard's or sparkle's typed animation.</summary>
internal sealed class MotherBrainGlassAnimation(MotherBrainGlassProgram program)
    : EnemyProjectileTimelineAnimation<MotherBrainGlassFrame, MotherBrainGlassCue>
{
    /// <summary>The program played.</summary>
    internal MotherBrainGlassProgram Program { get; } = program;

    /// <inheritdoc/>
    private protected override AnimationTimeline<MotherBrainGlassFrame, MotherBrainGlassCue> Timeline =>
        MotherBrainGlassAnimations.Timeline(Program);

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MotherBrainGlassFrame frame) =>
        MotherBrainGlassVisualDefinitions.ShownProjectileFrames[frame];
}
