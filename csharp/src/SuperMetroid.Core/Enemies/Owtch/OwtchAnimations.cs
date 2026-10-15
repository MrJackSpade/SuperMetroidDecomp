using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Owtch.OwtchCueKind;
using P = SuperMetroid.Core.Enemies.Owtch.OwtchProgram;
using SuperMetroid.Core.Enemies.Owtch.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>
/// Owtch's left and right shell-cycling animations as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="OwtchFrame"/> pose and each callback becomes a <see cref="OwtchCue"/> with its typed operands.
/// </summary>
internal static class OwtchAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<OwtchProgram, OwtchFrame, OwtchCue> Programs { get; } = new(
        new Dictionary<OwtchProgram, AnimationProgramStep<OwtchProgram, OwtchFrame, OwtchCue>[]>
    {
        // $A2:A3AB
        [P.MovingLeft] =
        [
            Cue(new(K.SetMovingLeft)),
            FallInto(P.MovingLeftLoop),
        ],
        // $A2:A3AD
        [P.MovingLeftLoop] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            GoTo(P.MovingLeftLoop),
        ],
        // $A2:A3BD
        [P.MovingRight] =
        [
            Cue(new(K.SetMovingRight)),
            FallInto(P.MovingRightLoop),
        ],
        // $A2:A3BF
        [P.MovingRightLoop] =
        [
            Show(2, 8),
            Show(1, 8),
            Show(0, 8),
            GoTo(P.MovingRightLoop),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params OwtchProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => OwtchFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<OwtchProgram, OwtchFrame, OwtchCue> Show(int pose, int duration) => AnimationProgramStep<OwtchProgram, OwtchFrame, OwtchCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<OwtchProgram, OwtchFrame, OwtchCue> Cue(OwtchCue cue) => AnimationProgramStep<OwtchProgram, OwtchFrame, OwtchCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<OwtchProgram, OwtchFrame, OwtchCue> GoTo(OwtchProgram program) => AnimationProgramStep<OwtchProgram, OwtchFrame, OwtchCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<OwtchProgram, OwtchFrame, OwtchCue> FallInto(OwtchProgram program) => AnimationProgramStep<OwtchProgram, OwtchFrame, OwtchCue>.FallInto(program);

}
