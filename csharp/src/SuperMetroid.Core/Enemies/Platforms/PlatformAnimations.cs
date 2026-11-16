using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Platforms.PlatformCueKind;
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
            Cue(new(K.MoveLeft)),
        ],
        [P.SuspensorPlatformLoop] =
        [
            Show(EnemyFrameId.SuspensorPlatformLightDark, 10),
            Show(EnemyFrameId.SuspensorPlatformLightDim, 10),
            Show(EnemyFrameId.SuspensorPlatformLightLit, 10),
            Show(EnemyFrameId.SuspensorPlatformLightBright, 10),
        ],
        [P.SuspensorPlatformHeadingRight] =
        [
            Cue(new(K.MoveRight)),
        ],
        [P.TripperMovingLeft] =
        [
            Cue(new(K.MoveLeft)),
        ],
        [P.TripperMovingLeftLoop] =
        [
            Show(EnemyFrameId.TripperHoveringFacingLeftJetShort, 7),
            Show(EnemyFrameId.TripperHoveringFacingLeftJetLong, 8),
            Show(EnemyFrameId.TripperHoveringFacingLeftJetShort, 7),
            Show(EnemyFrameId.TripperHoveringFacingLeftJetLongTwin, 8),
        ],
        [P.TripperMovingRight] =
        [
            Cue(new(K.MoveRight)),
        ],
        [P.TripperMovingRightLoop] =
        [
            Show(EnemyFrameId.TripperHoveringFacingRightJetShort, 7),
            Show(EnemyFrameId.TripperHoveringFacingRightJetLong, 8),
            Show(EnemyFrameId.TripperHoveringFacingRightJetShort, 7),
            Show(EnemyFrameId.TripperHoveringFacingRightJetLongTwin, 8),
        ],
        [P.TripperStillMovingLeft] =
        [
            Cue(new(K.MoveLeft)),
        ],
        [P.TripperStillMovingLeftLoop] =
        [
            Show(EnemyFrameId.TripperThrustingLeftJetMedium, 7),
            Show(EnemyFrameId.TripperThrustingLeftJetShort, 8),
            Show(EnemyFrameId.TripperThrustingLeftJetMedium, 7),
            Show(EnemyFrameId.TripperThrustingLeftJetLong, 8),
        ],
        [P.TripperStillMovingRight] =
        [
            Cue(new(K.MoveRight)),
        ],
        [P.TripperStillMovingRightLoop] =
        [
            Show(EnemyFrameId.TripperThrustingRightJetMedium, 7),
            Show(EnemyFrameId.TripperThrustingRightJetShort, 8),
            Show(EnemyFrameId.TripperThrustingRightJetMedium, 7),
            Show(EnemyFrameId.TripperThrustingRightJetLong, 8),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at; the loops only run on from the
    /// animations that start them (#2061).
    /// </summary>
    internal static IReadOnlyDictionary<PlatformSegment, Animation<PlatformSegment>> StartingAt { get; } = new Dictionary<PlatformSegment, Animation<PlatformSegment>>
    {
        [P.SuspensorPlatformHeadingLeft] = new([new(P.SuspensorPlatformHeadingLeft, 0, 1, 1), new(P.SuspensorPlatformLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SuspensorPlatformHeadingRight] = new([new(P.SuspensorPlatformHeadingRight, 0, 1, 1), new(P.SuspensorPlatformLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperMovingLeft] = new([new(P.TripperMovingLeft, 0, 1, 1), new(P.TripperMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperMovingRight] = new([new(P.TripperMovingRight, 0, 1, 1), new(P.TripperMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperStillMovingLeft] = new([new(P.TripperStillMovingLeft, 0, 1, 1), new(P.TripperStillMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperStillMovingRight] = new([new(P.TripperStillMovingRight, 0, 1, 1), new(P.TripperStillMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PlatformSegment> StartingWith(PlatformSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, PlatformCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, PlatformCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, PlatformCue> Cue(PlatformCue cue) => new AnimationBeat<EnemyFrameId, PlatformCue>.Event(cue);
}
