using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Alcoon;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>
/// The native bank-$A8 lists the Alcoon ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class AlcoonNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<AlcoonProgram>[] Entries =
    [
        new NativeProgramEntry<AlcoonProgram>(AlcoonProgram.WalkingLeft, 0xdbe7),
        new NativeProgramEntry<AlcoonProgram>(AlcoonProgram.WalkingLeftLoop, 0xdbe9),
        new NativeProgramEntry<AlcoonProgram>(AlcoonProgram.FireLeft, 0xdc03),
        new NativeProgramEntry<AlcoonProgram>(AlcoonProgram.AirborneLeftLookingUp, 0xdc4b),
        new NativeProgramEntry<AlcoonProgram>(AlcoonProgram.AirborneLeftLookingForward, 0xdc51),
        new NativeProgramEntry<AlcoonProgram>(AlcoonProgram.WalkingRight, 0xdc57),
        new NativeProgramEntry<AlcoonProgram>(AlcoonProgram.WalkingRightLoop, 0xdc59),
        new NativeProgramEntry<AlcoonProgram>(AlcoonProgram.FireRight, 0xdc73),
        new NativeProgramEntry<AlcoonProgram>(AlcoonProgram.AirborneRightLookingUp, 0xdcbb),
        new NativeProgramEntry<AlcoonProgram>(AlcoonProgram.AirborneRightLookingForward, 0xdcc1),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, AlcoonNativeListAddressesInstruction> Instructions = new()
    {
        [0xdf1c] = new AlcoonNativeListAddressesInstruction(AlcoonCueKind.FireUpward, ""),
        [0xdf33] = new AlcoonNativeListAddressesInstruction(AlcoonCueKind.FireHorizontally, ""),
        [0xdf39] = new AlcoonNativeListAddressesInstruction(AlcoonCueKind.FireDownward, ""),
        [0xdf3f] = new AlcoonNativeListAddressesInstruction(AlcoonCueKind.StartWalking, ""),
        [0xdf63] = new AlcoonNativeListAddressesInstruction(AlcoonCueKind.Step, ""),
        [0xdf71] = new AlcoonNativeListAddressesInstruction(AlcoonCueKind.Move, ""),
    };
}
