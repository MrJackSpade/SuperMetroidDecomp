using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Kraid.FakeKraidCueKind;
using P = SuperMetroid.Core.Bosses.Kraid.FakeKraidSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Fake Kraid's action choice, step and spit animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="FakeKraidCue"/> saying what it does.
/// </summary>
internal static class FakeKraidAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FakeKraidSegment, EnemyFrameId, FakeKraidCue> Segments { get; } = new(
        new Dictionary<FakeKraidSegment, AnimationBeat<EnemyFrameId, FakeKraidCue>[]>
    {
        [P.ChooseAction] =
        [
            Cue(new FakeKraidCue.Signal(K.ChooseAction)),
        ],
        [P.StepForwardsFacingLeft] =
        [
            Show(EnemyFrameId.FakeKraidSteppingFacingLeftFeetTogether, 16),
            Show(EnemyFrameId.FakeKraidSteppingFacingLeftFrontFootLifting, 12),
            Show(EnemyFrameId.FakeKraidSteppingFacingLeftFrontFootForward, 8),
            Show(EnemyFrameId.FakeKraidSteppingFacingLeftFeetPlanted, 12),
            Cue(new FakeKraidCue.Signal(K.Move)),
        ],
        [P.StepBackwardsFacingLeft] =
        [
            Show(EnemyFrameId.FakeKraidSteppingFacingLeftFeetTogether, 16),
            Cue(new FakeKraidCue.Signal(K.Move)),
            Show(EnemyFrameId.FakeKraidSteppingFacingLeftFeetPlanted, 12),
            Show(EnemyFrameId.FakeKraidSteppingFacingLeftFrontFootForward, 8),
            Show(EnemyFrameId.FakeKraidSteppingFacingLeftFrontFootLifting, 12),
        ],
        [P.FireSpitFacingLeft] =
        [
            Show(EnemyFrameId.FakeKraidFacingLeftMouthClosed, 16),
            Cue(new FakeKraidCue.Signal(K.PlayCrySound)),
            Show(EnemyFrameId.FakeKraidFacingLeftMouthOpening, 8),
            Cue(new FakeKraidCue.Spit(HorizontalDirection.Left)),
            Show(EnemyFrameId.FakeKraidFacingLeftMouthWide, 16),
            Show(EnemyFrameId.FakeKraidFacingLeftMouthOpening, 8),
        ],
        [P.StepForwardsFacingRight] =
        [
            Show(EnemyFrameId.FakeKraidSteppingFacingRightFeetTogether, 16),
            Show(EnemyFrameId.FakeKraidSteppingFacingRightFrontFootLifting, 12),
            Show(EnemyFrameId.FakeKraidSteppingFacingRightFrontFootForward, 8),
            Show(EnemyFrameId.FakeKraidSteppingFacingRightFeetPlanted, 12),
            Cue(new FakeKraidCue.Signal(K.Move)),
        ],
        [P.StepBackwardsFacingRight] =
        [
            Show(EnemyFrameId.FakeKraidSteppingFacingRightFeetTogether, 16),
            Cue(new FakeKraidCue.Signal(K.Move)),
            Show(EnemyFrameId.FakeKraidSteppingFacingRightFeetPlanted, 12),
            Show(EnemyFrameId.FakeKraidSteppingFacingRightFrontFootForward, 8),
            Show(EnemyFrameId.FakeKraidSteppingFacingRightFrontFootLifting, 12),
        ],
        [P.FireSpitFacingRight] =
        [
            Show(EnemyFrameId.FakeKraidFacingRightMouthClosed, 16),
            Cue(new FakeKraidCue.Signal(K.PlayCrySound)),
            Show(EnemyFrameId.FakeKraidFacingRightMouthOpening, 8),
            Cue(new FakeKraidCue.Spit(HorizontalDirection.Right)),
            Show(EnemyFrameId.FakeKraidFacingRightMouthWide, 16),
            Show(EnemyFrameId.FakeKraidFacingRightMouthOpening, 8),
        ],
    });

    /// <summary>
    /// The animation that starts at each step and spit (#2214): it plays, then chooses the next action, which always
    /// continues at the step or spit it picks, so no animation plays past its choice and none starts at the choice.
    /// </summary>
    internal static IReadOnlyDictionary<FakeKraidSegment, Animation<FakeKraidSegment>> StartingAt { get; } = new Dictionary<FakeKraidSegment, Animation<FakeKraidSegment>>
    {
        [P.StepForwardsFacingLeft] = new([new(P.StepForwardsFacingLeft, 0, 5, 1), new(P.ChooseAction, 0, 1, 1)], AnimationEnd.Holds),
        [P.StepBackwardsFacingLeft] = new([new(P.StepBackwardsFacingLeft, 0, 5, 1), new(P.ChooseAction, 0, 1, 1)], AnimationEnd.Holds),
        [P.FireSpitFacingLeft] = new([new(P.FireSpitFacingLeft, 0, 6, 1), new(P.ChooseAction, 0, 1, 1)], AnimationEnd.Holds),
        [P.StepForwardsFacingRight] = new([new(P.StepForwardsFacingRight, 0, 5, 1), new(P.ChooseAction, 0, 1, 1)], AnimationEnd.Holds),
        [P.StepBackwardsFacingRight] = new([new(P.StepBackwardsFacingRight, 0, 5, 1), new(P.ChooseAction, 0, 1, 1)], AnimationEnd.Holds),
        [P.FireSpitFacingRight] = new([new(P.FireSpitFacingRight, 0, 6, 1), new(P.ChooseAction, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FakeKraidSegment> StartingWith(FakeKraidSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, FakeKraidCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, FakeKraidCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, FakeKraidCue> Cue(FakeKraidCue cue) => new AnimationBeat<EnemyFrameId, FakeKraidCue>.Event(cue);
}
