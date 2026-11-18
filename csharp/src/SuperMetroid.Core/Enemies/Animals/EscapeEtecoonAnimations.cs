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
        [P.RunningLeftLowTideStride] = Stride(EtecoonStride.Left, 5),
        [P.RunningLeftHighTide] = Stride(EtecoonStride.Left, 3),
        [P.RunningRightLowTide] =
        [
            Cue(new EscapeEtecoonCue.LowTideRunStarts(Running: HorizontalDirection.Right)),
        ],
        [P.RunningRightLowTideStride] = Stride(EtecoonStride.Right, 6),
        [P.RunningRightHighTide] = Stride(EtecoonStride.Right, 3),
        [P.RunningForEscape] =
        [
            Cue(new EscapeEtecoonCue.Signal(K.Escapes)),
        ],
        [P.RunningForEscapeStride] = Stride(EtecoonStride.Right, 3),
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
        [P.ThankingSteps] = SteppingStride(EtecoonStride.Left, 8, -ThankingStepPixels),
        [P.ThankingLook] =
        [
            Show(EnemyFrameId.EscapeEtecoonLookingRight, 64),
            Show(EnemyFrameId.EscapeEtecoonTurningLeft, 8),
        ],
    });

    /// <summary>How many times a thanking Etecoon takes its four steps to the left.</summary>
    private const int ThankingStepRuns = 8;

    /// <summary>The pixels a thanking Etecoon steps left with each pose of its stride.</summary>
    private const int ThankingStepPixels = 3;

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

    /// <summary>One <paramref name="stride"/> (#2477), each of its poses shown for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue>[] Stride(EtecoonStride stride, int updates) =>
        [.. stride.Poses.Select(pose => Show(pose, updates))];

    /// <summary>
    /// One <paramref name="stride"/> taken a step at a time (#2477): each pose shown for <paramref name="updates"/> updates
    /// and followed by a step of <paramref name="step"/> pixels.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue>[] SteppingStride(EtecoonStride stride, int updates, int step) =>
        [.. stride.Poses.SelectMany(pose => new[] { Show(pose, updates), Cue(new EscapeEtecoonCue.Move(Offset: step)) })];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Cue(EscapeEtecoonCue cue) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Event(cue);
}
