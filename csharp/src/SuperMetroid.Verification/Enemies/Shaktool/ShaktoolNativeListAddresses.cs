using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>
/// The native bank-$AA lists the Shaktool ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class ShaktoolNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xaa;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (ShaktoolProgram Program, ushort Entry)[] Entries =
    [
        (ShaktoolProgram.SawHandAttackPrimaryPiece, 0xd9ea),
        (ShaktoolProgram.SawHandAttackFinalPiece, 0xd9f2),
        (ShaktoolProgram.SawHandHeadBobPrimaryPiece, 0xd9fc),
        (ShaktoolProgram.SawHandHeadBobFinalPiece, 0xda04),
        (ShaktoolProgram.SawHandPrimaryPiece, 0xda0e),
        (ShaktoolProgram.SawHandFinalPiece, 0xda1e),
        (ShaktoolProgram.ArmPieceAttackBack, 0xda2e),
        (ShaktoolProgram.ArmPieceAttackFront, 0xda42),
        (ShaktoolProgram.ArmPieceHeadBobBack, 0xda56),
        (ShaktoolProgram.ArmPieceHeadBobFront, 0xda62),
        (ShaktoolProgram.ArmPieceNormal, 0xda72),
        (ShaktoolProgram.HeadAttack, 0xda7a),
        (ShaktoolProgram.HeadHeadBob, 0xda90),
        (ShaktoolProgram.HeadAimingLeft, 0xdaa4),
        (ShaktoolProgram.HeadAimingUpLeft, 0xdaac),
        (ShaktoolProgram.HeadAimingUp, 0xdab4),
        (ShaktoolProgram.HeadAimingUpRight, 0xdabc),
        (ShaktoolProgram.HeadAimingRight, 0xdac4),
        (ShaktoolProgram.HeadAimingDownRight, 0xdacc),
        (ShaktoolProgram.HeadAimingDown, 0xdad4),
        (ShaktoolProgram.HeadAimingDownLeft, 0xdadc),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (ShaktoolCueKind Kind, string Operands)> Instructions = new()
    {
        [0xd931] = (ShaktoolCueKind.LowerAwayFromCenter, ""),
        [0xd93f] = (ShaktoolCueKind.RaiseTowardCenter, ""),
        [0xd94a] = (ShaktoolCueKind.Lower, ""),
        [0xd953] = (ShaktoolCueKind.Raise, ""),
        [0xd99f] = (ShaktoolCueKind.Nothing, ""),
        [0xd9ba] = (ShaktoolCueKind.ResetFunctions, ""),
    };
}
