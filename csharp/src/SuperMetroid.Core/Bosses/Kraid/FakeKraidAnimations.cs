using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Kraid.FakeKraidCueKind;
using P = SuperMetroid.Core.Bosses.Kraid.FakeKraidProgram;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Mini-Kraid's action choice, step and spit animations as typed programs (#641), in place of their lists. Generated from the verified lists: each
/// frame shows a <see cref="FakeKraidFrame"/> pose and each callback becomes a <see cref="FakeKraidCue"/> with its typed operands.
/// </summary>
internal static class FakeKraidAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FakeKraidProgram, FakeKraidFrame, FakeKraidCue> Segments { get; } = new(
        new Dictionary<FakeKraidProgram, AnimationBeat<FakeKraidFrame, FakeKraidCue>[]>
    {
        [P.ChooseActionFacingLeft] =
        [
            Cue(new(K.ChooseAction)),
        ],
        [P.StepForwardsFacingLeft] =
        [
            Show(0, 16),
            Show(1, 12),
            Show(2, 8),
            Show(3, 12),
            Cue(new(K.Move)),
        ],
        [P.ChooseAlternateActionFacingLeft] =
        [
            Cue(new(K.ChooseAction)),
        ],
        [P.StepBackwardsFacingLeft] =
        [
            Show(0, 16),
            Cue(new(K.Move)),
            Show(3, 12),
            Show(2, 8),
            Show(1, 12),
        ],
        [P.FireSpitFacingLeft] =
        [
            Show(4, 16),
            Cue(new(K.PlayCrySound)),
            Show(5, 8),
            Cue(new(K.SpitLeft)),
            Show(6, 16),
            Show(5, 8),
        ],
        [P.ChooseActionFacingRight] =
        [
            Cue(new(K.ChooseAction)),
        ],
        [P.StepForwardsFacingRight] =
        [
            Show(7, 16),
            Show(8, 12),
            Show(9, 8),
            Show(10, 12),
            Cue(new(K.Move)),
        ],
        [P.ChooseAlternateActionFacingRight] =
        [
            Cue(new(K.ChooseAction)),
        ],
        [P.StepBackwardsFacingRight] =
        [
            Show(7, 16),
            Cue(new(K.Move)),
            Show(10, 12),
            Show(9, 8),
            Show(8, 12),
        ],
        [P.FireSpitFacingRight] =
        [
            Show(11, 16),
            Cue(new(K.PlayCrySound)),
            Show(12, 8),
            Cue(new(K.SpitRight)),
            Show(13, 16),
            Show(12, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FakeKraidProgram, Animation<FakeKraidProgram>> StartingAt { get; } = new Dictionary<FakeKraidProgram, Animation<FakeKraidProgram>>
    {
        [P.ChooseActionFacingLeft] = new([new(P.ChooseActionFacingLeft, 0, 1, 1), new(P.StepForwardsFacingLeft, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StepForwardsFacingLeft] = new([new(P.StepForwardsFacingLeft, 0, 5, 1), new(P.ChooseActionFacingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChooseAlternateActionFacingLeft] = new([new(P.ChooseAlternateActionFacingLeft, 0, 1, 1), new(P.StepBackwardsFacingLeft, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StepBackwardsFacingLeft] = new([new(P.StepBackwardsFacingLeft, 0, 5, 1), new(P.ChooseAlternateActionFacingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FireSpitFacingLeft] = new([new(P.FireSpitFacingLeft, 0, 6, 1), new(P.ChooseActionFacingLeft, 0, 1, 1), new(P.StepForwardsFacingLeft, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChooseActionFacingRight] = new([new(P.ChooseActionFacingRight, 0, 1, 1), new(P.StepForwardsFacingRight, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StepForwardsFacingRight] = new([new(P.StepForwardsFacingRight, 0, 5, 1), new(P.ChooseActionFacingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChooseAlternateActionFacingRight] = new([new(P.ChooseAlternateActionFacingRight, 0, 1, 1), new(P.StepBackwardsFacingRight, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StepBackwardsFacingRight] = new([new(P.StepBackwardsFacingRight, 0, 5, 1), new(P.ChooseAlternateActionFacingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FireSpitFacingRight] = new([new(P.FireSpitFacingRight, 0, 6, 1), new(P.ChooseActionFacingRight, 0, 1, 1), new(P.StepForwardsFacingRight, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FakeKraidProgram> StartingWith(FakeKraidProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<FakeKraidFrame, FakeKraidCue> Show(int pose, int duration) => new AnimationBeat<FakeKraidFrame, FakeKraidCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<FakeKraidFrame, FakeKraidCue> Cue(FakeKraidCue cue) => new AnimationBeat<FakeKraidFrame, FakeKraidCue>.Event(cue);
}
