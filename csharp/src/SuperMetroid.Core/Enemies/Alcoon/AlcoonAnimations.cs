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
            Cue(new(K.Move)),
        ],
        [P.WalkingLeftLoop] =
        [
            Show(EnemyFrameId.AlcoonWalkingLeftNearLegStepping, 10),
            Cue(new(K.Move)),
            Show(EnemyFrameId.AlcoonWalkingLeftNearLegPlanted, 10),
            Cue(new(K.Move)),
            Show(EnemyFrameId.AlcoonWalkingLeftFarLegStepping, 10),
            Cue(new(K.Step)),
            Show(EnemyFrameId.AlcoonWalkingLeftFarLegPlanted, 10),
        ],
        [P.FireLeft] =
        [
            Show(EnemyFrameId.AlcoonFacingLeftMouthClosed, 20),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingLeftThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 3),
            Cue(new(K.FireHorizontally)),
            Show(EnemyFrameId.AlcoonFacingLeftMouthWide, 10),
            Show(EnemyFrameId.AlcoonFacingLeftMouthClosed, 10),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingLeftThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 3),
            Cue(new(K.FireUpward)),
            Show(EnemyFrameId.AlcoonFacingLeftMouthWide, 10),
            Show(EnemyFrameId.AlcoonFacingLeftMouthClosed, 10),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingLeftThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingLeftMouthOpening, 3),
            Cue(new(K.FireDownward)),
            Show(EnemyFrameId.AlcoonFacingLeftMouthWide, 40),
            Cue(new(K.StartWalking)),
            Show(EnemyFrameId.AlcoonFacingLeftMouthWide, 1),
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
            Cue(new(K.Move)),
        ],
        [P.WalkingRightLoop] =
        [
            Show(EnemyFrameId.AlcoonWalkingRightNearLegStepping, 10),
            Cue(new(K.Move)),
            Show(EnemyFrameId.AlcoonWalkingRightNearLegPlanted, 10),
            Cue(new(K.Move)),
            Show(EnemyFrameId.AlcoonWalkingRightFarLegStepping, 10),
            Cue(new(K.Step)),
            Show(EnemyFrameId.AlcoonWalkingRightFarLegPlanted, 10),
        ],
        [P.FireRight] =
        [
            Show(EnemyFrameId.AlcoonFacingRightMouthClosed, 20),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingRightThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 3),
            Cue(new(K.FireHorizontally)),
            Show(EnemyFrameId.AlcoonFacingRightMouthWide, 10),
            Show(EnemyFrameId.AlcoonFacingRightMouthClosed, 10),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingRightThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 3),
            Cue(new(K.FireUpward)),
            Show(EnemyFrameId.AlcoonFacingRightMouthWide, 10),
            Show(EnemyFrameId.AlcoonFacingRightMouthClosed, 10),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 9),
            Show(EnemyFrameId.AlcoonFacingRightThroatGlowing, 16),
            Show(EnemyFrameId.AlcoonFacingRightMouthOpening, 3),
            Cue(new(K.FireDownward)),
            Show(EnemyFrameId.AlcoonFacingRightMouthWide, 40),
            Cue(new(K.StartWalking)),
            Show(EnemyFrameId.AlcoonFacingRightMouthWide, 1),
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

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<AlcoonSegment, Animation<AlcoonSegment>> StartingAt { get; } = new Dictionary<AlcoonSegment, Animation<AlcoonSegment>>
    {
        [P.WalkingLeft] = new([new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeftLoop, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WalkingLeftLoop] = new([new(P.WalkingLeftLoop, 0, 7, 1), new(P.WalkingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FireLeft] = new([new(P.FireLeft, 0, 20, 1), new(P.AirborneLeftLookingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.AirborneLeftLookingUp] = new([new(P.AirborneLeftLookingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.AirborneLeftLookingForward] = new([new(P.AirborneLeftLookingForward, 0, 1, 1)], AnimationEnd.Holds),
        [P.WalkingRight] = new([new(P.WalkingRight, 0, 1, 1), new(P.WalkingRightLoop, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WalkingRightLoop] = new([new(P.WalkingRightLoop, 0, 7, 1), new(P.WalkingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FireRight] = new([new(P.FireRight, 0, 20, 1), new(P.AirborneRightLookingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.AirborneRightLookingUp] = new([new(P.AirborneRightLookingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.AirborneRightLookingForward] = new([new(P.AirborneRightLookingForward, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<AlcoonSegment> StartingWith(AlcoonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, AlcoonCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, AlcoonCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Cue(AlcoonCue cue) => new AnimationBeat<EnemyFrameId, AlcoonCue>.Event(cue);
}
