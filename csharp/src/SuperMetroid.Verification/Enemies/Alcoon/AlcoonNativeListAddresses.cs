using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<AlcoonSegment>[] Entries =
    [
        new NativeProgramEntry<AlcoonSegment>(AlcoonSegment.WalkingLeft, 0xdbe7),
        new NativeProgramEntry<AlcoonSegment>(AlcoonSegment.WalkingLeftLoop, 0xdbe9),
        new NativeProgramEntry<AlcoonSegment>(AlcoonSegment.FireLeft, 0xdc03),
        new NativeProgramEntry<AlcoonSegment>(AlcoonSegment.AirborneLeftLookingUp, 0xdc4b),
        new NativeProgramEntry<AlcoonSegment>(AlcoonSegment.AirborneLeftLookingForward, 0xdc51),
        new NativeProgramEntry<AlcoonSegment>(AlcoonSegment.WalkingRight, 0xdc57),
        new NativeProgramEntry<AlcoonSegment>(AlcoonSegment.WalkingRightLoop, 0xdc59),
        new NativeProgramEntry<AlcoonSegment>(AlcoonSegment.FireRight, 0xdc73),
        new NativeProgramEntry<AlcoonSegment>(AlcoonSegment.AirborneRightLookingUp, 0xdcbb),
        new NativeProgramEntry<AlcoonSegment>(AlcoonSegment.AirborneRightLookingForward, 0xdcc1),
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
