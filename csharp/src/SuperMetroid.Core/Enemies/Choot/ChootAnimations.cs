using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Choot.ChootCueKind;
using P = SuperMetroid.Core.Enemies.Choot.ChootProgram;
using SuperMetroid.Core.Enemies.Choot.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// Choot's idle, jumping and falling animations as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="ChootFrame"/> pose and each callback becomes a <see cref="ChootCue"/> with its typed operands.
/// </summary>
internal static class ChootAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<ChootProgram, ChootFrame, ChootCue> Programs { get; } = new(
        new Dictionary<ChootProgram, AnimationProgramStep<ChootProgram, ChootFrame, ChootCue>[]>
    {
        // $A2:D82C
        [P.Idle] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(0, 1),
            Hold,
        ],
        // $A2:D834
        [P.Jumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(1, 8),
            Show(2, 1),
            Hold,
        ],
        // $A2:D840
        [P.Falling] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(1, 8),
            Show(3, 1),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ChootProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => ChootFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<ChootProgram, ChootFrame, ChootCue> Show(int pose, int duration) => AnimationProgramStep<ChootProgram, ChootFrame, ChootCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<ChootProgram, ChootFrame, ChootCue> Cue(ChootCue cue) => AnimationProgramStep<ChootProgram, ChootFrame, ChootCue>.Publish(cue);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<ChootProgram, ChootFrame, ChootCue> Hold => AnimationProgramStep<ChootProgram, ChootFrame, ChootCue>.Hold;

}
