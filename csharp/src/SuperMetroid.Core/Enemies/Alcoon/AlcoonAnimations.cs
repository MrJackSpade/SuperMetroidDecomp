using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Alcoon.AlcoonCueKind;
using P = SuperMetroid.Core.Enemies.Alcoon.AlcoonProgram;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The Alcoon's walking, fireball and airborne animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="AlcoonFrame"/> pose and each callback becomes a <see cref="AlcoonCue"/> with its typed operands.
/// </summary>
internal static class AlcoonAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<AlcoonProgram, AlcoonFrame, AlcoonCue> Programs { get; } = new(
        new Dictionary<AlcoonProgram, AnimationProgramStep<AlcoonProgram, AlcoonFrame, AlcoonCue>[]>
    {
        // $A8:DBE7
        [P.WalkingLeft] =
        [
            Cue(new(K.Move)),
            FallInto(P.WalkingLeftLoop),
        ],
        // $A8:DBE9
        [P.WalkingLeftLoop] =
        [
            Show(0, 10),
            Cue(new(K.Move)),
            Show(1, 10),
            Cue(new(K.Move)),
            Show(2, 10),
            Cue(new(K.Step)),
            Show(3, 10),
            GoTo(P.WalkingLeft),
        ],
        // $A8:DC03
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
            FallInto(P.AirborneLeftLookingUp),
        ],
        // $A8:DC4B
        [P.AirborneLeftLookingUp] =
        [
            Show(8, 32767),
            Hold,
        ],
        // $A8:DC51
        [P.AirborneLeftLookingForward] =
        [
            Show(3, 32767),
            Hold,
        ],
        // $A8:DC57
        [P.WalkingRight] =
        [
            Cue(new(K.Move)),
            FallInto(P.WalkingRightLoop),
        ],
        // $A8:DC59
        [P.WalkingRightLoop] =
        [
            Show(9, 10),
            Cue(new(K.Move)),
            Show(10, 10),
            Cue(new(K.Move)),
            Show(11, 10),
            Cue(new(K.Step)),
            Show(12, 10),
            GoTo(P.WalkingRight),
        ],
        // $A8:DC73
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
            FallInto(P.AirborneRightLookingUp),
        ],
        // $A8:DCBB
        [P.AirborneRightLookingUp] =
        [
            Show(17, 32767),
            Hold,
        ],
        // $A8:DCC1
        [P.AirborneRightLookingForward] =
        [
            Show(12, 32767),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<AlcoonProgram, AlcoonFrame, AlcoonCue> Show(int pose, int duration) => AnimationProgramStep<AlcoonProgram, AlcoonFrame, AlcoonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<AlcoonProgram, AlcoonFrame, AlcoonCue> Cue(AlcoonCue cue) => AnimationProgramStep<AlcoonProgram, AlcoonFrame, AlcoonCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<AlcoonProgram, AlcoonFrame, AlcoonCue> GoTo(AlcoonProgram program) => AnimationProgramStep<AlcoonProgram, AlcoonFrame, AlcoonCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<AlcoonProgram, AlcoonFrame, AlcoonCue> FallInto(AlcoonProgram program) => AnimationProgramStep<AlcoonProgram, AlcoonFrame, AlcoonCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<AlcoonProgram, AlcoonFrame, AlcoonCue> Hold => AnimationProgramStep<AlcoonProgram, AlcoonFrame, AlcoonCue>.Hold;

}
