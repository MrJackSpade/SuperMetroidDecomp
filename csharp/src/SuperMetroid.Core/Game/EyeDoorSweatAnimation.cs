using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>An eye-door sweat drop's typed animation.</summary>
internal sealed class EyeDoorSweatAnimation(EyeDoorSweatProgram program)
    : EnemyProjectileTimelineAnimation<EyeDoorSweatFrame, EyeDoorSweatCue>
{
    /// <summary>The program played.</summary>
    internal EyeDoorSweatProgram Program { get; } = program;

    /// <inheritdoc/>
    private protected override AnimationTimeline<EyeDoorSweatFrame, EyeDoorSweatCue> Timeline =>
        EyeDoorSweatAnimations.Timeline(Program);

    /// <inheritdoc/>
    private protected override ushort PresentationKey(EyeDoorSweatFrame frame) => EyeDoorSweatVisualDefinitions.PresentationKey(frame);
}