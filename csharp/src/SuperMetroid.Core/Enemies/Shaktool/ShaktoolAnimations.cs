using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Shaktool.ShaktoolCueKind;
using P = SuperMetroid.Core.Enemies.Shaktool.ShaktoolProgram;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The Shaktool's saw-hand, arm and head animations as typed programs (#641), in place of their bank-$AA lists. Generated from the verified native lists: each
/// frame shows a <see cref="ShaktoolFrame"/> pose and each callback becomes a <see cref="ShaktoolCue"/> with its typed operands.
/// </summary>
internal static class ShaktoolAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<ShaktoolProgram, ShaktoolFrame, ShaktoolCue> Programs { get; } = new(
        new Dictionary<ShaktoolProgram, AnimationProgramStep<ShaktoolProgram, ShaktoolFrame, ShaktoolCue>[]>
    {
        [P.SawHandAttackPrimaryPiece] =
        [
            Wait(576),
            GoTo(P.SawHandPrimaryPiece),
        ],
        [P.SawHandAttackFinalPiece] =
        [
            Wait(576),
            Cue(new(K.ResetFunctions)),
            GoTo(P.SawHandFinalPiece),
        ],
        [P.SawHandHeadBobPrimaryPiece] =
        [
            Wait(20),
            GoTo(P.SawHandPrimaryPiece),
        ],
        [P.SawHandHeadBobFinalPiece] =
        [
            Wait(20),
            Cue(new(K.ResetFunctions)),
            GoTo(P.SawHandFinalPiece),
        ],
        [P.SawHandPrimaryPiece] =
        [
            Show(12, 10),
            Show(13, 10),
            Show(14, 10),
            GoTo(P.SawHandPrimaryPiece),
        ],
        [P.SawHandFinalPiece] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            GoTo(P.SawHandFinalPiece),
        ],
        [P.ArmPieceAttackBack] =
        [
            Wait(192),
            Cue(new(K.LowerAwayFromCenter)),
            Wait(128),
            Cue(new(K.RaiseTowardCenter)),
            Wait(256),
            GoTo(P.ArmPieceNormal),
        ],
        [P.ArmPieceAttackFront] =
        [
            Wait(256),
            Cue(new(K.LowerAwayFromCenter)),
            Wait(128),
            Cue(new(K.RaiseTowardCenter)),
            Wait(192),
            GoTo(P.ArmPieceNormal),
        ],
        [P.ArmPieceHeadBobBack] =
        [
            Cue(new(K.Lower)),
            Wait(20),
            Cue(new(K.Raise)),
            GoTo(P.ArmPieceNormal),
        ],
        [P.ArmPieceHeadBobFront] =
        [
            Wait(4),
            Cue(new(K.Lower)),
            Wait(12),
            Cue(new(K.Raise)),
            Wait(4),
            FallInto(P.ArmPieceNormal),
        ],
        [P.ArmPieceNormal] =
        [
            Show(3, 119),
            GoTo(P.ArmPieceNormal),
        ],
        [P.HeadAttack] =
        [
            Wait(128),
            Cue(new(K.LowerAwayFromCenter)),
            Cue(new(K.Nothing)),
            Wait(128),
            Cue(new(K.RaiseTowardCenter)),
            Wait(320),
            Wait(1),
            FallInto(P.HeadHeadBob),
        ],
        [P.HeadHeadBob] =
        [
            Wait(8),
            Cue(new(K.Lower)),
            Wait(4),
            Cue(new(K.Raise)),
            Wait(8),
            Wait(1),
            FallInto(P.HeadAimingLeft),
        ],
        [P.HeadAimingLeft] =
        [
            Show(4, 1908),
            GoTo(P.HeadAimingLeft),
        ],
        [P.HeadAimingUpLeft] =
        [
            Show(5, 1909),
            GoTo(P.HeadAimingUpLeft),
        ],
        [P.HeadAimingUp] =
        [
            Show(6, 1910),
            GoTo(P.HeadAimingUp),
        ],
        [P.HeadAimingUpRight] =
        [
            Show(7, 1911),
            GoTo(P.HeadAimingUpRight),
        ],
        [P.HeadAimingRight] =
        [
            Show(8, 1912),
            GoTo(P.HeadAimingRight),
        ],
        [P.HeadAimingDownRight] =
        [
            Show(9, 1913),
            GoTo(P.HeadAimingDownRight),
        ],
        [P.HeadAimingDown] =
        [
            Show(10, 1914),
            GoTo(P.HeadAimingDown),
        ],
        [P.HeadAimingDownLeft] =
        [
            Show(11, 1915),
            GoTo(P.HeadAimingDownLeft),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<ShaktoolProgram, ShaktoolFrame, ShaktoolCue> Show(int pose, int duration) => AnimationProgramStep<ShaktoolProgram, ShaktoolFrame, ShaktoolCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<ShaktoolProgram, ShaktoolFrame, ShaktoolCue> Cue(ShaktoolCue cue) => AnimationProgramStep<ShaktoolProgram, ShaktoolFrame, ShaktoolCue>.Publish(cue);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    private static AnimationProgramStep<ShaktoolProgram, ShaktoolFrame, ShaktoolCue> Wait(int updates) => AnimationProgramStep<ShaktoolProgram, ShaktoolFrame, ShaktoolCue>.Wait(updates);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<ShaktoolProgram, ShaktoolFrame, ShaktoolCue> GoTo(ShaktoolProgram program) => AnimationProgramStep<ShaktoolProgram, ShaktoolFrame, ShaktoolCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<ShaktoolProgram, ShaktoolFrame, ShaktoolCue> FallInto(ShaktoolProgram program) => AnimationProgramStep<ShaktoolProgram, ShaktoolFrame, ShaktoolCue>.FallInto(program);

}
