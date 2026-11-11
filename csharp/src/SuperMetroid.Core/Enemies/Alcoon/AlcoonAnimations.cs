using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Alcoon.AlcoonCueKind;
using P = SuperMetroid.Core.Enemies.Alcoon.AlcoonProgram;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The Alcoon's walking, fireball and airborne animations as typed programs (#641). Each
/// frame shows a <see cref="AlcoonFrame"/> pose and each callback becomes a <see cref="AlcoonCue"/> with its typed operands.
/// </summary>
internal static class AlcoonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<AlcoonProgram, AlcoonFrame, AlcoonCue> Segments { get; } = new(
        new Dictionary<AlcoonProgram, AnimationBeat<AlcoonFrame, AlcoonCue>[]>
    {
        [P.WalkingLeft] =
        [
            Cue(new(K.Move)),
        ],
        [P.WalkingLeftLoop] =
        [
            Show(0, 10),
            Cue(new(K.Move)),
            Show(1, 10),
            Cue(new(K.Move)),
            Show(2, 10),
            Cue(new(K.Step)),
            Show(3, 10),
        ],
        [P.FireLeft] =
        [
            Show(4, 20),
            Show(5, 9),
            Show(6, 16),
            Show(5, 3),
            Cue(new(K.FireHorizontally)),
            Show(7, 10),
            Show(4, 10),
            Show(5, 9),
            Show(6, 16),
            Show(5, 3),
            Cue(new(K.FireUpward)),
            Show(7, 10),
            Show(4, 10),
            Show(5, 9),
            Show(6, 16),
            Show(5, 3),
            Cue(new(K.FireDownward)),
            Show(7, 40),
            Cue(new(K.StartWalking)),
            Show(7, 1),
        ],
        [P.AirborneLeftLookingUp] =
        [
            Hold(8),
        ],
        [P.AirborneLeftLookingForward] =
        [
            Hold(3),
        ],
        [P.WalkingRight] =
        [
            Cue(new(K.Move)),
        ],
        [P.WalkingRightLoop] =
        [
            Show(9, 10),
            Cue(new(K.Move)),
            Show(10, 10),
            Cue(new(K.Move)),
            Show(11, 10),
            Cue(new(K.Step)),
            Show(12, 10),
        ],
        [P.FireRight] =
        [
            Show(13, 20),
            Show(14, 9),
            Show(15, 16),
            Show(14, 3),
            Cue(new(K.FireHorizontally)),
            Show(16, 10),
            Show(13, 10),
            Show(14, 9),
            Show(15, 16),
            Show(14, 3),
            Cue(new(K.FireUpward)),
            Show(16, 10),
            Show(13, 10),
            Show(14, 9),
            Show(15, 16),
            Show(14, 3),
            Cue(new(K.FireDownward)),
            Show(16, 40),
            Cue(new(K.StartWalking)),
            Show(16, 1),
        ],
        [P.AirborneRightLookingUp] =
        [
            Hold(17),
        ],
        [P.AirborneRightLookingForward] =
        [
            Hold(12),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<AlcoonFrame, AlcoonCue> Show(int pose, int duration) => new AnimationBeat<AlcoonFrame, AlcoonCue>.Frame(new(pose), duration);

    /// <summary>Shows pose <paramref name="pose"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<AlcoonFrame, AlcoonCue> Hold(int pose) => new AnimationBeat<AlcoonFrame, AlcoonCue>.HeldFrame(new(pose));

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<AlcoonFrame, AlcoonCue> Cue(AlcoonCue cue) => new AnimationBeat<AlcoonFrame, AlcoonCue>.Event(cue);
}
