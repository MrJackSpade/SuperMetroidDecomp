using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>A Kraid rock's (or the Kago bug's initial) typed animation.</summary>
internal sealed class KraidRockAnimation(KraidRockProgram program) : EnemyProjectileTimelineAnimation<KraidRockFrame, KraidRockCue>
{
    /// <summary>The program played.</summary>
    internal KraidRockProgram Program { get; } = program;

    /// <inheritdoc/>
    private protected override AnimationTimeline<KraidRockFrame, KraidRockCue> Timeline => KraidRockAnimations.Timeline(Program);

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(KraidRockFrame frame) =>
        KraidRockVisualDefinitions.ShownProjectileFrames[frame];
}
