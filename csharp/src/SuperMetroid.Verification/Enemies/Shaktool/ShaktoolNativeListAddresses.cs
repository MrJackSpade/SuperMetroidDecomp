using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<ShaktoolProgram>[] Entries =
    [
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.SawHandAttackPrimaryPiece, 0xd9ea),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.SawHandAttackFinalPiece, 0xd9f2),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.SawHandHeadBobPrimaryPiece, 0xd9fc),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.SawHandHeadBobFinalPiece, 0xda04),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.SawHandPrimaryPiece, 0xda0e),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.SawHandFinalPiece, 0xda1e),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.ArmPieceAttackBack, 0xda2e),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.ArmPieceAttackFront, 0xda42),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.ArmPieceHeadBobBack, 0xda56),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.ArmPieceHeadBobFront, 0xda62),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.ArmPieceNormal, 0xda72),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.HeadAttack, 0xda7a),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.HeadHeadBob, 0xda90),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.HeadAimingLeft, 0xdaa4),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.HeadAimingUpLeft, 0xdaac),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.HeadAimingUp, 0xdab4),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.HeadAimingUpRight, 0xdabc),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.HeadAimingRight, 0xdac4),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.HeadAimingDownRight, 0xdacc),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.HeadAimingDown, 0xdad4),
        new NativeProgramEntry<ShaktoolProgram>(ShaktoolProgram.HeadAimingDownLeft, 0xdadc),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, ShaktoolNativeListAddressesInstruction> Instructions = new()
    {
        [0xd931] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.LowerAwayFromCenter, ""),
        [0xd93f] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.RaiseTowardCenter, ""),
        [0xd94a] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.Lower, ""),
        [0xd953] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.Raise, ""),
        [0xd99f] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.Nothing, ""),
        [0xd9ba] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.ResetFunctions, ""),
    };
}
