using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Alcoon.AlcoonCueKind;
using P = SuperMetroid.Core.Enemies.Alcoon.AlcoonProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The Alcoon's walking, fireball and airborne animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="AlcoonCue"/> with its typed operands.
/// </summary>
internal static class AlcoonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<AlcoonProgram, EnemyFrameId, AlcoonCue> Segments { get; } = new(
        new Dictionary<AlcoonProgram, AnimationBeat<EnemyFrameId, AlcoonCue>[]>
    {
        [P.WalkingLeft] =
        [
            Cue(new(K.Move)),
        ],
        [P.WalkingLeftLoop] =
        [
            Show(EnemyFrameId.AlcoonLeftWalk0, 10),
            Cue(new(K.Move)),
            Show(EnemyFrameId.AlcoonLeftWalk1, 10),
            Cue(new(K.Move)),
            Show(EnemyFrameId.AlcoonLeftWalk2, 10),
            Cue(new(K.Step)),
            Show(EnemyFrameId.AlcoonLeftWalk3, 10),
        ],
        [P.FireLeft] =
        [
            Show(EnemyFrameId.AlcoonLeftFire0, 20),
            Show(EnemyFrameId.AlcoonLeftFire1, 9),
            Show(EnemyFrameId.AlcoonLeftFire2, 16),
            Show(EnemyFrameId.AlcoonLeftFire1, 3),
            Cue(new(K.FireHorizontally)),
            Show(EnemyFrameId.AlcoonLeftFire3, 10),
            Show(EnemyFrameId.AlcoonLeftFire0, 10),
            Show(EnemyFrameId.AlcoonLeftFire1, 9),
            Show(EnemyFrameId.AlcoonLeftFire2, 16),
            Show(EnemyFrameId.AlcoonLeftFire1, 3),
            Cue(new(K.FireUpward)),
            Show(EnemyFrameId.AlcoonLeftFire3, 10),
            Show(EnemyFrameId.AlcoonLeftFire0, 10),
            Show(EnemyFrameId.AlcoonLeftFire1, 9),
            Show(EnemyFrameId.AlcoonLeftFire2, 16),
            Show(EnemyFrameId.AlcoonLeftFire1, 3),
            Cue(new(K.FireDownward)),
            Show(EnemyFrameId.AlcoonLeftFire3, 40),
            Cue(new(K.StartWalking)),
            Show(EnemyFrameId.AlcoonLeftFire3, 1),
        ],
        [P.AirborneLeftLookingUp] =
        [
            Hold(EnemyFrameId.AlcoonLeftAirUp),
        ],
        [P.AirborneLeftLookingForward] =
        [
            Hold(EnemyFrameId.AlcoonLeftWalk3),
        ],
        [P.WalkingRight] =
        [
            Cue(new(K.Move)),
        ],
        [P.WalkingRightLoop] =
        [
            Show(EnemyFrameId.AlcoonRightWalk0, 10),
            Cue(new(K.Move)),
            Show(EnemyFrameId.AlcoonRightWalk1, 10),
            Cue(new(K.Move)),
            Show(EnemyFrameId.AlcoonRightWalk2, 10),
            Cue(new(K.Step)),
            Show(EnemyFrameId.AlcoonRightWalk3, 10),
        ],
        [P.FireRight] =
        [
            Show(EnemyFrameId.AlcoonRightFire0, 20),
            Show(EnemyFrameId.AlcoonRightFire1, 9),
            Show(EnemyFrameId.AlcoonRightFire2, 16),
            Show(EnemyFrameId.AlcoonRightFire1, 3),
            Cue(new(K.FireHorizontally)),
            Show(EnemyFrameId.AlcoonRightFire3, 10),
            Show(EnemyFrameId.AlcoonRightFire0, 10),
            Show(EnemyFrameId.AlcoonRightFire1, 9),
            Show(EnemyFrameId.AlcoonRightFire2, 16),
            Show(EnemyFrameId.AlcoonRightFire1, 3),
            Cue(new(K.FireUpward)),
            Show(EnemyFrameId.AlcoonRightFire3, 10),
            Show(EnemyFrameId.AlcoonRightFire0, 10),
            Show(EnemyFrameId.AlcoonRightFire1, 9),
            Show(EnemyFrameId.AlcoonRightFire2, 16),
            Show(EnemyFrameId.AlcoonRightFire1, 3),
            Cue(new(K.FireDownward)),
            Show(EnemyFrameId.AlcoonRightFire3, 40),
            Cue(new(K.StartWalking)),
            Show(EnemyFrameId.AlcoonRightFire3, 1),
        ],
        [P.AirborneRightLookingUp] =
        [
            Hold(EnemyFrameId.AlcoonRightAirUp),
        ],
        [P.AirborneRightLookingForward] =
        [
            Hold(EnemyFrameId.AlcoonRightWalk3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<AlcoonProgram, Animation<AlcoonProgram>> StartingAt { get; } = new Dictionary<AlcoonProgram, Animation<AlcoonProgram>>
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
    internal static Animation<AlcoonProgram> StartingWith(AlcoonProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, AlcoonCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, AlcoonCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Cue(AlcoonCue cue) => new AnimationBeat<EnemyFrameId, AlcoonCue>.Event(cue);
}
