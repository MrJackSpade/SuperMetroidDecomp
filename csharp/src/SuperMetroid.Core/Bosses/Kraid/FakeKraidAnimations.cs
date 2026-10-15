using K = SuperMetroid.Core.Bosses.Kraid.FakeKraidCueKind;
using P = SuperMetroid.Core.Bosses.Kraid.FakeKraidProgram;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Mini-Kraid's action choice, step and spit animations as typed programs (#641), in place of their bank-$A6 lists. Generated from the verified native lists: each
/// frame shows a <see cref="FakeKraidFrame"/> pose and each callback becomes a <see cref="FakeKraidCue"/> with its typed operands.
/// </summary>
internal static class FakeKraidAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<FakeKraidProgram, FakeKraidFrame, FakeKraidCue> Programs { get; } = new(
        new Dictionary<FakeKraidProgram, AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue>[]>
    {
        // $A6:99AC
        [P.ChooseActionFacingLeft] =
        [
            Cue(new(K.ChooseAction)),
            FallInto(P.StepForwardsFacingLeft),
        ],
        // $A6:99AE
        [P.StepForwardsFacingLeft] =
        [
            Show(0, 16),
            Show(1, 12),
            Show(2, 8),
            Show(3, 12),
            Cue(new(K.Move)),
            GoTo(P.ChooseActionFacingLeft),
        ],
        // $A6:99C4
        [P.ChooseAlternateActionFacingLeft] =
        [
            Cue(new(K.ChooseAction)),
            FallInto(P.StepBackwardsFacingLeft),
        ],
        // $A6:99C6
        [P.StepBackwardsFacingLeft] =
        [
            Show(0, 16),
            Cue(new(K.Move)),
            Show(3, 12),
            Show(2, 8),
            Show(1, 12),
            GoTo(P.ChooseAlternateActionFacingLeft),
        ],
        // $A6:99DC
        [P.FireSpitFacingLeft] =
        [
            Show(4, 16),
            Cue(new(K.PlayCrySound)),
            Show(5, 8),
            Cue(new(K.SpitLeft)),
            Show(6, 16),
            Show(5, 8),
            GoTo(P.ChooseActionFacingLeft),
        ],
        // $A6:99FA
        [P.ChooseActionFacingRight] =
        [
            Cue(new(K.ChooseAction)),
            FallInto(P.StepForwardsFacingRight),
        ],
        // $A6:99FC
        [P.StepForwardsFacingRight] =
        [
            Show(7, 16),
            Show(8, 12),
            Show(9, 8),
            Show(10, 12),
            Cue(new(K.Move)),
            GoTo(P.ChooseActionFacingRight),
        ],
        // $A6:9A12
        [P.ChooseAlternateActionFacingRight] =
        [
            Cue(new(K.ChooseAction)),
            FallInto(P.StepBackwardsFacingRight),
        ],
        // $A6:9A14
        [P.StepBackwardsFacingRight] =
        [
            Show(7, 16),
            Cue(new(K.Move)),
            Show(10, 12),
            Show(9, 8),
            Show(8, 12),
            GoTo(P.ChooseAlternateActionFacingRight),
        ],
        // $A6:9A2A
        [P.FireSpitFacingRight] =
        [
            Show(11, 16),
            Cue(new(K.PlayCrySound)),
            Show(12, 8),
            Cue(new(K.SpitRight)),
            Show(13, 16),
            Show(12, 8),
            GoTo(P.ChooseActionFacingRight),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FakeKraidProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => FakeKraidFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue> Show(int pose, int duration) => AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue> Cue(FakeKraidCue cue) => AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue> GoTo(FakeKraidProgram program) => AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue> FallInto(FakeKraidProgram program) => AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue>.FallInto(program);

}
