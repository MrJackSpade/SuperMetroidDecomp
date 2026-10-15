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
    internal static readonly (AlcoonProgram Program, ushort Entry)[] Entries =
    [
        (AlcoonProgram.WalkingLeft, 0xdbe7),
        (AlcoonProgram.WalkingLeftLoop, 0xdbe9),
        (AlcoonProgram.FireLeft, 0xdc03),
        (AlcoonProgram.AirborneLeftLookingUp, 0xdc4b),
        (AlcoonProgram.AirborneLeftLookingForward, 0xdc51),
        (AlcoonProgram.WalkingRight, 0xdc57),
        (AlcoonProgram.WalkingRightLoop, 0xdc59),
        (AlcoonProgram.FireRight, 0xdc73),
        (AlcoonProgram.AirborneRightLookingUp, 0xdcbb),
        (AlcoonProgram.AirborneRightLookingForward, 0xdcc1),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (AlcoonCueKind Kind, string Operands)> Instructions = new()
    {
        [0xdf1c] = (AlcoonCueKind.FireUpward, ""),
        [0xdf33] = (AlcoonCueKind.FireHorizontally, ""),
        [0xdf39] = (AlcoonCueKind.FireDownward, ""),
        [0xdf3f] = (AlcoonCueKind.StartWalking, ""),
        [0xdf63] = (AlcoonCueKind.Step, ""),
        [0xdf71] = (AlcoonCueKind.Move, ""),
    };
}
