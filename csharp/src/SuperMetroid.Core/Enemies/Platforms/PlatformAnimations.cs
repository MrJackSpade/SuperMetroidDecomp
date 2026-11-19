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
            Show(EnemyFrameId.PlatformLightDark, 10),
            Show(EnemyFrameId.PlatformLightDim, 10),
            Show(EnemyFrameId.PlatformLightLit, 10),
            Show(EnemyFrameId.PlatformLightBright, 10),
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
        ],
        [P.TripperMovingRight] =
        [
            Cue(new(HorizontalDirection.Right)),
        ],
        [P.TripperHoveringRight] =
        [
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
    /// thrust it loops, which only run on from the animations that start them (#2061, #2276). A hover flickers its jet
    /// <see cref="HoverFlickers"/> times a loop (#2439).
    /// </summary>
    private static Dictionary<PlatformSegment, Animation<PlatformSegment>> StartingAt { get; } = new Dictionary<PlatformSegment, Animation<PlatformSegment>>
    {
        [P.SuspensorPlatformHeadingLeft] = Animation<PlatformSegment>.LeadingInto([Segments.Whole(P.SuspensorPlatformHeadingLeft)], [Segments.Whole(P.SuspensorPlatformLights)]),
        [P.SuspensorPlatformHeadingRight] = Animation<PlatformSegment>.LeadingInto([Segments.Whole(P.SuspensorPlatformHeadingRight)], [Segments.Whole(P.SuspensorPlatformLights)]),
        [P.TripperMovingLeft] = Animation<PlatformSegment>.LeadingInto([Segments.Whole(P.TripperMovingLeft)], [Segments.Repeated(P.TripperHoveringLeft, HoverFlickers)]),
        [P.TripperMovingRight] = Animation<PlatformSegment>.LeadingInto([Segments.Whole(P.TripperMovingRight)], [Segments.Repeated(P.TripperHoveringRight, HoverFlickers)]),
        [P.TripperStillMovingLeft] = Animation<PlatformSegment>.LeadingInto([Segments.Whole(P.TripperStillMovingLeft)], [Segments.Whole(P.TripperThrustingLeft)]),
        [P.TripperStillMovingRight] = Animation<PlatformSegment>.LeadingInto([Segments.Whole(P.TripperStillMovingRight)], [Segments.Whole(P.TripperThrustingRight)]),
    };

    /// <summary>The flickers of its jet, short then long, a hovering Tripper's loop shows (#2439).</summary>
    private const int HoverFlickers = 2;

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PlatformSegment> StartingWith(PlatformSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, PlatformCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, PlatformCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, PlatformCue> Cue(PlatformCue cue) => new AnimationBeat<EnemyFrameId, PlatformCue>.Event(cue);
}
