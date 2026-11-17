using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Platforms.PlatformSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The Tripper's and the suspensor platform's vertically moving and still platform loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="PlatformCue"/> naming what it does.
/// </summary>
internal static class PlatformAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PlatformSegment, EnemyFrameId, PlatformCue> Segments { get; } = new(
        new Dictionary<PlatformSegment, AnimationBeat<EnemyFrameId, PlatformCue>[]>
    {
        [P.SuspensorPlatformHeadingLeft] =
        [
            Cue(new(HorizontalDirection.Left)),
        ],
        [P.SuspensorPlatformLights] =
        [
            Show(EnemyFrameId.SuspensorPlatformLightDark, 10),
            Show(EnemyFrameId.SuspensorPlatformLightDim, 10),
            Show(EnemyFrameId.SuspensorPlatformLightLit, 10),
            Show(EnemyFrameId.SuspensorPlatformLightBright, 10),
        ],
        [P.SuspensorPlatformHeadingRight] =
        [
            Cue(new(HorizontalDirection.Right)),
        ],
        [P.TripperMovingLeft] =
        [
            Cue(new(HorizontalDirection.Left)),
        ],
        [P.TripperHoveringLeft] =
        [
            Show(EnemyFrameId.TripperHoveringFacingLeftJetShort, 7),
            Show(EnemyFrameId.TripperHoveringFacingLeftJetLong, 8),
            Show(EnemyFrameId.TripperHoveringFacingLeftJetShort, 7),
            Show(EnemyFrameId.TripperHoveringFacingLeftJetLong, 8),
        ],
        [P.TripperMovingRight] =
        [
            Cue(new(HorizontalDirection.Right)),
        ],
        [P.TripperHoveringRight] =
        [
            Show(EnemyFrameId.TripperHoveringFacingRightJetShort, 7),
            Show(EnemyFrameId.TripperHoveringFacingRightJetLong, 8),
            Show(EnemyFrameId.TripperHoveringFacingRightJetShort, 7),
            Show(EnemyFrameId.TripperHoveringFacingRightJetLong, 8),
        ],
        [P.TripperStillMovingLeft] =
        [
            Cue(new(HorizontalDirection.Left)),
        ],
        [P.TripperThrustingLeft] =
        [
            Show(EnemyFrameId.TripperThrustingLeftJetMedium, 7),
            Show(EnemyFrameId.TripperThrustingLeftJetShort, 8),
            Show(EnemyFrameId.TripperThrustingLeftJetMedium, 7),
            Show(EnemyFrameId.TripperThrustingLeftJetLong, 8),
        ],
        [P.TripperStillMovingRight] =
        [
            Cue(new(HorizontalDirection.Right)),
        ],
        [P.TripperThrustingRight] =
        [
            Show(EnemyFrameId.TripperThrustingRightJetMedium, 7),
            Show(EnemyFrameId.TripperThrustingRightJetShort, 8),
            Show(EnemyFrameId.TripperThrustingRightJetMedium, 7),
            Show(EnemyFrameId.TripperThrustingRightJetLong, 8),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at: its heading cue, then the lights, hover or
    /// thrust it loops, which only run on from the animations that start them (#2061, #2276).
    /// </summary>
    internal static IReadOnlyDictionary<PlatformSegment, Animation<PlatformSegment>> StartingAt { get; } = new Dictionary<PlatformSegment, Animation<PlatformSegment>>
    {
        [P.SuspensorPlatformHeadingLeft] = new([Whole(P.SuspensorPlatformHeadingLeft), Whole(P.SuspensorPlatformLights)], AnimationEnd.LoopsFrom(1)),
        [P.SuspensorPlatformHeadingRight] = new([Whole(P.SuspensorPlatformHeadingRight), Whole(P.SuspensorPlatformLights)], AnimationEnd.LoopsFrom(1)),
        [P.TripperMovingLeft] = new([Whole(P.TripperMovingLeft), Whole(P.TripperHoveringLeft)], AnimationEnd.LoopsFrom(1)),
        [P.TripperMovingRight] = new([Whole(P.TripperMovingRight), Whole(P.TripperHoveringRight)], AnimationEnd.LoopsFrom(1)),
        [P.TripperStillMovingLeft] = new([Whole(P.TripperStillMovingLeft), Whole(P.TripperThrustingLeft)], AnimationEnd.LoopsFrom(1)),
        [P.TripperStillMovingRight] = new([Whole(P.TripperStillMovingRight), Whole(P.TripperThrustingRight)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PlatformSegment> StartingWith(PlatformSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<PlatformSegment> Whole(PlatformSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, PlatformCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, PlatformCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, PlatformCue> Cue(PlatformCue cue) => new AnimationBeat<EnemyFrameId, PlatformCue>.Event(cue);
}
