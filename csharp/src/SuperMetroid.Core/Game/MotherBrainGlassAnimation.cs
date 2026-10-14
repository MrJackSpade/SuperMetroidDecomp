using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

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
    private protected override ushort PresentationKey(MotherBrainGlassFrame frame) => MotherBrainGlassVisualDefinitions.PresentationKey(frame);
}
