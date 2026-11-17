using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonCueKind;
using P = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Etecoons' typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="EscapeEtecoonCue"/> with the values it acts on.
/// </summary>
internal static class EscapeEtecoonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EscapeEtecoonSegment, EnemyFrameId, EscapeEtecoonCue> Segments { get; } = new(
        new Dictionary<EscapeEtecoonSegment, AnimationBeat<EnemyFrameId, EscapeEtecoonCue>[]>
    {
        [P.RunningLeftLowTide] =
        [
            Cue(new EscapeEtecoonCue.LowTideRunStarts(Running: HorizontalDirection.Left)),
        ],
        [P.RunningLeftLowTideStride] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningLeftStretched, 5),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 5),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftPushingOff, 5),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 5),
        ],
        [P.RunningLeftHighTide] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningLeftStretched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftPushingOff, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 3),
        ],
        [P.RunningRightLowTide] =
        [
            Cue(new EscapeEtecoonCue.LowTideRunStarts(Running: HorizontalDirection.Right)),
        ],
        [P.RunningRightLowTideStride] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningRightStretched, 6),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 6),
            Show(EnemyFrameId.EscapeEtecoonRunningRightPushingOff, 6),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 6),
        ],
        [P.RunningRightHighTide] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningRightStretched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightPushingOff, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 3),
        ],
        [P.RunningForEscape] =
        [
            Cue(new EscapeEtecoonCue.Signal(K.Escapes)),
        ],
        [P.RunningForEscapeStride] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningRightStretched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightPushingOff, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 3),
        ],
        [P.Stationary] =
        [
            Show(EnemyFrameId.EscapeEtecoonLookingRight, 64),
            Show(EnemyFrameId.EscapeEtecoonTurningLeft, 8),
            Show(EnemyFrameId.EscapeEtecoonLookingLeft, 64),
            Show(EnemyFrameId.EscapeEtecoonTurningRight, 8),
        ],
        [P.ExpressGratitudeThenEscape] =
        [
            Cue(new EscapeEtecoonCue.Signal(K.ExpressesGratitude)),
        ],
        [P.ThankingSteps] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningLeftStretched, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftPushingOff, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
        ],
        [P.ThankingLook] =
        [
            Show(EnemyFrameId.EscapeEtecoonLookingRight, 64),
            Show(EnemyFrameId.EscapeEtecoonTurningLeft, 8),
        ],
    });

    /// <summary>How many times a thanking Etecoon takes its four steps to the left.</summary>
    private const int ThankingStepRuns = 8;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2275): a low-tide or escape run raises
    /// its own entry cue and then strides for good; a thanking Etecoon thanks Samus, steps left eight times, looks, and runs
    /// for the escape.
    /// </summary>
    internal static IReadOnlyDictionary<EscapeEtecoonSegment, Animation<EscapeEtecoonSegment>> StartingAt { get; } = new Dictionary<EscapeEtecoonSegment, Animation<EscapeEtecoonSegment>>
    {
        [P.RunningLeftLowTide] = new([Whole(P.RunningLeftLowTide), Whole(P.RunningLeftLowTideStride)], AnimationEnd.LoopsFrom(1)),
        [P.RunningLeftHighTide] = new([Whole(P.RunningLeftHighTide)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightLowTide] = new([Whole(P.RunningRightLowTide), Whole(P.RunningRightLowTideStride)], AnimationEnd.LoopsFrom(1)),
        [P.RunningRightHighTide] = new([Whole(P.RunningRightHighTide)], AnimationEnd.LoopsFrom(0)),
        [P.RunningForEscape] = new([Whole(P.RunningForEscape), Whole(P.RunningForEscapeStride)], AnimationEnd.LoopsFrom(1)),
        [P.Stationary] = new([Whole(P.Stationary)], AnimationEnd.LoopsFrom(0)),
        [P.ExpressGratitudeThenEscape] = new(
            [Whole(P.ExpressGratitudeThenEscape), Repeated(P.ThankingSteps, ThankingStepRuns), Whole(P.ThankingLook),
                Whole(P.RunningForEscape), Whole(P.RunningForEscapeStride)],
            AnimationEnd.LoopsFrom(4)),
    };

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<EscapeEtecoonSegment> Whole(EscapeEtecoonSegment segment) => Repeated(segment, 1);

    /// <summary><paramref name="plays"/> plays of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<EscapeEtecoonSegment> Repeated(EscapeEtecoonSegment segment, int plays) =>
        new(segment, 0, Segments.Beats(segment).Count, plays);

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EscapeEtecoonSegment> StartingWith(EscapeEtecoonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Cue(EscapeEtecoonCue cue) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Event(cue);
}
