using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>A Kraid rock's (or the Kago bug's initial) typed animation.</summary>
internal sealed class KraidRockAnimation(KraidRockProgram program) : EnemyProjectileTimelineAnimation<KraidRockFrame, KraidRockCue>
{
    /// <summary>The program played.</summary>
    internal KraidRockProgram Program { get; } = program;

    /// <inheritdoc/>
    private protected override AnimationTimeline<KraidRockFrame, KraidRockCue> Timeline => KraidRockAnimations.Timeline(Program);

    /// <inheritdoc/>
    private protected override ushort PresentationKey(KraidRockFrame frame) => KraidRockVisualDefinitions.PresentationKey(frame);
}
