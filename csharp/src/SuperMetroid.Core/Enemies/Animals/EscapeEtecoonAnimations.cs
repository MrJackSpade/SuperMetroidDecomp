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
            Show(EnemyFrameId.EtecoonRunningLeftStretched, 5),
            Show(EnemyFrameId.EtecoonRunningLeftBunched, 5),
            Show(EnemyFrameId.EtecoonRunningLeftPushingOff, 5),
            Show(EnemyFrameId.EtecoonRunningLeftBunched, 5),
        ],
        [P.RunningLeftHighTide] =
        [
            Show(EnemyFrameId.EtecoonRunningLeftStretched, 3),
            Show(EnemyFrameId.EtecoonRunningLeftBunched, 3),
            Show(EnemyFrameId.EtecoonRunningLeftPushingOff, 3),
            Show(EnemyFrameId.EtecoonRunningLeftBunched, 3),
        ],
        [P.RunningRightLowTide] =
        [
            Cue(new EscapeEtecoonCue.LowTideRunStarts(Running: HorizontalDirection.Right)),
        ],
        [P.RunningRightLowTideStride] =
        [
            Show(EnemyFrameId.EtecoonRunningRightStretched, 6),
            Show(EnemyFrameId.EtecoonRunningRightBunched, 6),
            Show(EnemyFrameId.EtecoonRunningRightPushingOff, 6),
            Show(EnemyFrameId.EtecoonRunningRightBunched, 6),
        ],
        [P.RunningRightHighTide] =
        [
            Show(EnemyFrameId.EtecoonRunningRightStretched, 3),
            Show(EnemyFrameId.EtecoonRunningRightBunched, 3),
            Show(EnemyFrameId.EtecoonRunningRightPushingOff, 3),
            Show(EnemyFrameId.EtecoonRunningRightBunched, 3),
        ],
        [P.RunningForEscape] =
        [
            Cue(new EscapeEtecoonCue.Signal(K.Escapes)),
        ],
        [P.RunningForEscapeStride] =
        [
            Show(EnemyFrameId.EtecoonRunningRightStretched, 3),
            Show(EnemyFrameId.EtecoonRunningRightBunched, 3),
            Show(EnemyFrameId.EtecoonRunningRightPushingOff, 3),
            Show(EnemyFrameId.EtecoonRunningRightBunched, 3),
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
            Show(EnemyFrameId.EtecoonRunningLeftStretched, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EtecoonRunningLeftBunched, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EtecoonRunningLeftPushingOff, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EtecoonRunningLeftBunched, 8),
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
        [P.RunningLeftLowTide] = Animation<EscapeEtecoonSegment>.LeadingInto([Segments.Whole(P.RunningLeftLowTide)], [Segments.Whole(P.RunningLeftLowTideStride)]),
        [P.RunningLeftHighTide] = Animation<EscapeEtecoonSegment>.Looping([Segments.Whole(P.RunningLeftHighTide)]),
        [P.RunningRightLowTide] = Animation<EscapeEtecoonSegment>.LeadingInto([Segments.Whole(P.RunningRightLowTide)], [Segments.Whole(P.RunningRightLowTideStride)]),
        [P.RunningRightHighTide] = Animation<EscapeEtecoonSegment>.Looping([Segments.Whole(P.RunningRightHighTide)]),
        [P.RunningForEscape] = Animation<EscapeEtecoonSegment>.LeadingInto([Segments.Whole(P.RunningForEscape)], [Segments.Whole(P.RunningForEscapeStride)]),
        [P.Stationary] = Animation<EscapeEtecoonSegment>.Looping([Segments.Whole(P.Stationary)]),
        [P.ExpressGratitudeThenEscape] = Animation<EscapeEtecoonSegment>.LeadingInto(
            [Segments.Whole(P.ExpressGratitudeThenEscape), Segments.Repeated(P.ThankingSteps, ThankingStepRuns), Segments.Whole(P.ThankingLook),
                Segments.Whole(P.RunningForEscape)], [Segments.Whole(P.RunningForEscapeStride)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EscapeEtecoonSegment> StartingWith(EscapeEtecoonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Cue(EscapeEtecoonCue cue) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Event(cue);
}
