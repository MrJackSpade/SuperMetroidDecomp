using SuperMetroid.Core.Programs;
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
        [P.ChooseActionFacingLeft] =
        [
            Cue(new(K.ChooseAction)),
            FallInto(P.StepForwardsFacingLeft),
        ],
        [P.StepForwardsFacingLeft] =
        [
            Show(0, 16),
            Show(1, 12),
            Show(2, 8),
            Show(3, 12),
            Cue(new(K.Move)),
            GoTo(P.ChooseActionFacingLeft),
        ],
        [P.ChooseAlternateActionFacingLeft] =
        [
            Cue(new(K.ChooseAction)),
            FallInto(P.StepBackwardsFacingLeft),
        ],
        [P.StepBackwardsFacingLeft] =
        [
            Show(0, 16),
            Cue(new(K.Move)),
            Show(3, 12),
            Show(2, 8),
            Show(1, 12),
            GoTo(P.ChooseAlternateActionFacingLeft),
        ],
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
        [P.ChooseActionFacingRight] =
        [
            Cue(new(K.ChooseAction)),
            FallInto(P.StepForwardsFacingRight),
        ],
        [P.StepForwardsFacingRight] =
        [
            Show(7, 16),
            Show(8, 12),
            Show(9, 8),
            Show(10, 12),
            Cue(new(K.Move)),
            GoTo(P.ChooseActionFacingRight),
        ],
        [P.ChooseAlternateActionFacingRight] =
        [
            Cue(new(K.ChooseAction)),
            FallInto(P.StepBackwardsFacingRight),
        ],
        [P.StepBackwardsFacingRight] =
        [
            Show(7, 16),
            Cue(new(K.Move)),
            Show(10, 12),
            Show(9, 8),
            Show(8, 12),
            GoTo(P.ChooseAlternateActionFacingRight),
        ],
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue> Show(int pose, int duration) => AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue> Cue(FakeKraidCue cue) => AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue> GoTo(FakeKraidProgram program) => AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue> FallInto(FakeKraidProgram program) => AnimationProgramStep<FakeKraidProgram, FakeKraidFrame, FakeKraidCue>.FallInto(program);

}
