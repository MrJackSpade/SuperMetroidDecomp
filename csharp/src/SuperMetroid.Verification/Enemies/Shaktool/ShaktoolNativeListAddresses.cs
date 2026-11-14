using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<ShaktoolSegment>[] Entries =
    [
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.SawHandAttackPrimaryPiece, 0xd9ea),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.SawHandAttackFinalPiece, 0xd9f2),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.SawHandHeadBobPrimaryPiece, 0xd9fc),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.SawHandHeadBobFinalPiece, 0xda04),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.SawHandPrimaryPiece, 0xda0e),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.SawHandFinalPiece, 0xda1e),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.ArmPieceAttackBack, 0xda2e),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.ArmPieceAttackFront, 0xda42),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.ArmPieceHeadBobBack, 0xda56),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.ArmPieceHeadBobFront, 0xda62),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.ArmPieceNormal, 0xda72),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadAttack, 0xda7a),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadHeadBob, 0xda90),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadAimingLeft, 0xdaa4),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadAimingUpLeft, 0xdaac),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadAimingUp, 0xdab4),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadAimingUpRight, 0xdabc),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadAimingRight, 0xdac4),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadAimingDownRight, 0xdacc),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadAimingDown, 0xdad4),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadAimingDownLeft, 0xdadc),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, ShaktoolNativeListAddressesInstruction> Instructions = new()
    {
        [0xd931] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.LowerAwayFromCenter, ""),
        [0xd93f] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.RaiseTowardCenter, ""),
        [0xd94a] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.Lower, ""),
        [0xd953] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.Raise, ""),
        [0xd9ba] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.ResetFrameBehaviors, ""),
    };

    /// <summary>
    /// The native routine the head's attack list calls between its two long pauses (#1803): it returned at once, so the
    /// animations have no event for it.
    /// </summary>
    internal static readonly HashSet<ushort> OmittedInstructions = [0xd99f];
}
