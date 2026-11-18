using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Alcoon.AlcoonCueKind;
using P = SuperMetroid.Core.Enemies.Alcoon.AlcoonSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The Alcoon's walking, fireball and airborne animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="AlcoonCue"/> naming what it does.
/// </summary>
internal static class AlcoonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<AlcoonSegment, EnemyFrameId, AlcoonCue> Segments { get; } = new(
        new Dictionary<AlcoonSegment, AnimationBeat<EnemyFrameId, AlcoonCue>[]>
    {
        [P.WalkingLeft] =
        [
            Cue(new AlcoonCue.Signal(K.Move)),
            Show(EnemyFrameId.AlcoonWalkingLeftNearLegStepping, 10),
            Cue(new AlcoonCue.Signal(K.Move)),
            Show(EnemyFrameId.AlcoonWalkingLeftNearLegPlanted, 10),
            Cue(new AlcoonCue.Signal(K.Move)),
            Show(EnemyFrameId.AlcoonWalkingLeftFarLegStepping, 10),
            Cue(new AlcoonCue.Signal(K.CompleteWalkCycle)),
            Show(EnemyFrameId.AlcoonWalkingLeftFarLegPlanted, 10),
        ],
        [P.FireLeft] =
        [
            Show(EnemyFrameId.AlcoonFacingLeftMouthClosed, 20),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingLeftThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 3),
            Cue(new AlcoonCue.Fire(AlcoonFireballLaunchDefinitions.Level)),
            Show(EnemyFrameId.AlcoonFacingLeftMouthWide, 10),
            Show(EnemyFrameId.AlcoonFacingLeftMouthClosed, 10),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingLeftThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 3),
            Cue(new AlcoonCue.Fire(AlcoonFireballLaunchDefinitions.Upward)),
            Show(EnemyFrameId.AlcoonFacingLeftMouthWide, 10),
            Show(EnemyFrameId.AlcoonFacingLeftMouthClosed, 10),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingLeftThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 3),
            Cue(new AlcoonCue.Fire(AlcoonFireballLaunchDefinitions.Downward)),
            Show(EnemyFrameId.AlcoonFacingLeftMouthWide, 40),
            Cue(new AlcoonCue.Signal(K.StartWalking)),
        ],
        [P.AirborneLeftLookingUp] =
        [
            Hold(EnemyFrameId.AlcoonLeapingFacingLeft),
        ],
        [P.AirborneLeftLookingForward] =
        [
            Hold(EnemyFrameId.AlcoonWalkingLeftFarLegPlanted),
        ],
        [P.WalkingRight] =
        [
            Cue(new AlcoonCue.Signal(K.Move)),
            Show(EnemyFrameId.AlcoonWalkingRightNearLegStepping, 10),
            Cue(new AlcoonCue.Signal(K.Move)),
            Show(EnemyFrameId.AlcoonWalkingRightNearLegPlanted, 10),
            Cue(new AlcoonCue.Signal(K.Move)),
            Show(EnemyFrameId.AlcoonWalkingRightFarLegStepping, 10),
            Cue(new AlcoonCue.Signal(K.CompleteWalkCycle)),
            Show(EnemyFrameId.AlcoonWalkingRightFarLegPlanted, 10),
        ],
        [P.FireRight] =
        [
            Show(EnemyFrameId.AlcoonFacingRightMouthClosed, 20),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingRightThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 3),
            Cue(new AlcoonCue.Fire(AlcoonFireballLaunchDefinitions.Level)),
            Show(EnemyFrameId.AlcoonFacingRightMouthWide, 10),
            Show(EnemyFrameId.AlcoonFacingRightMouthClosed, 10),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingRightThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 3),
            Cue(new AlcoonCue.Fire(AlcoonFireballLaunchDefinitions.Upward)),
            Show(EnemyFrameId.AlcoonFacingRightMouthWide, 10),
            Show(EnemyFrameId.AlcoonFacingRightMouthClosed, 10),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingRightThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 3),
            Cue(new AlcoonCue.Fire(AlcoonFireballLaunchDefinitions.Downward)),
            Show(EnemyFrameId.AlcoonFacingRightMouthWide, 40),
            Cue(new AlcoonCue.Signal(K.StartWalking)),
        ],
        [P.AirborneRightLookingUp] =
        [
            Hold(EnemyFrameId.AlcoonLeapingFacingRight),
        ],
        [P.AirborneRightLookingForward] =
        [
            Hold(EnemyFrameId.AlcoonWalkingRightFarLegPlanted),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. A walk is one cycle, its move cue and
    /// its four strides, played for good (#2275). A fire animation ends at its start-walking cue, which always continues at
    /// the walk for the Alcoon's facing (#2061).
    /// </summary>
    internal static IReadOnlyDictionary<AlcoonSegment, Animation<AlcoonSegment>> StartingAt { get; } = new Dictionary<AlcoonSegment, Animation<AlcoonSegment>>
    {
        [P.WalkingLeft] = Animation<AlcoonSegment>.Looping([Segments.Whole(P.WalkingLeft)]),
        [P.FireLeft] = new([Segments.Whole(P.FireLeft)], AnimationEnd.Holds),
        [P.AirborneLeftLookingUp] = new([Segments.Whole(P.AirborneLeftLookingUp)], AnimationEnd.Holds),
        [P.AirborneLeftLookingForward] = new([Segments.Whole(P.AirborneLeftLookingForward)], AnimationEnd.Holds),
        [P.WalkingRight] = Animation<AlcoonSegment>.Looping([Segments.Whole(P.WalkingRight)]),
        [P.FireRight] = new([Segments.Whole(P.FireRight)], AnimationEnd.Holds),
        [P.AirborneRightLookingUp] = new([Segments.Whole(P.AirborneRightLookingUp)], AnimationEnd.Holds),
        [P.AirborneRightLookingForward] = new([Segments.Whole(P.AirborneRightLookingForward)], AnimationEnd.Holds),
    };

    /// <summary>The left walk entered at its first stride, past its move cue, as a wall turning the Alcoon left enters it.</summary>
    internal static AnimationContinuation<AlcoonSegment> WalkingLeftFromFirstStride { get; } = new(StartingAt[P.WalkingLeft],
        Segments.PlaceOfFrame(StartingAt[P.WalkingLeft], P.WalkingLeft, EnemyFrameId.AlcoonWalkingLeftNearLegStepping));

    /// <summary>The right walk entered at its first stride, past its move cue, as a wall turning the Alcoon right enters it.</summary>
    internal static AnimationContinuation<AlcoonSegment> WalkingRightFromFirstStride { get; } = new(StartingAt[P.WalkingRight],
        Segments.PlaceOfFrame(StartingAt[P.WalkingRight], P.WalkingRight, EnemyFrameId.AlcoonWalkingRightNearLegStepping));

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<AlcoonSegment> StartingWith(AlcoonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, AlcoonCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, AlcoonCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Cue(AlcoonCue cue) => new AnimationBeat<EnemyFrameId, AlcoonCue>.Event(cue);
}
