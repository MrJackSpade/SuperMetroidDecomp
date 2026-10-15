using SuperMetroid.Core.Enemies.Hoppers;

namespace SuperMetroid.Verification.Enemies.Hoppers;

/// <summary>
/// The native bank-$A3 lists the Hopper ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class HopperNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (HopperProgram Program, ushort Entry)[] Entries =
    [
        (HopperProgram.SidehopperFloorJumping, 0xaa76),
        (HopperProgram.SidehopperFloorLanded, 0xaa82),
        (HopperProgram.SidehopperCeilingJumping, 0xaa9c),
        (HopperProgram.SidehopperCeilingLanded, 0xaaa8),
        (HopperProgram.DessgeegaFloorJumping, 0xafa5),
        (HopperProgram.DessgeegaFloorLanded, 0xafad),
        (HopperProgram.DessgeegaCeilingJumping, 0xafc3),
        (HopperProgram.DessgeegaCeilingLanded, 0xafcb),
        (HopperProgram.LargeSidehopperFloorJumping, 0xb0c5),
        (HopperProgram.LargeSidehopperFloorLanded, 0xb0d1),
        (HopperProgram.LargeSidehopperCeilingJumping, 0xb0eb),
        (HopperProgram.LargeSidehopperCeilingLanded, 0xb0f7),
        (HopperProgram.LargeDessgeegaFloorJumping, 0xb237),
        (HopperProgram.LargeDessgeegaFloorLanded, 0xb23f),
        (HopperProgram.LargeDessgeegaCeilingJumping, 0xb255),
        (HopperProgram.LargeDessgeegaCeilingLanded, 0xb25d),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (HopperCueKind Kind, string Operands)> Instructions = new()
    {
        [0x8173] = (HopperCueKind.ProcessOffScreen, ""),
        [0x817d] = (HopperCueKind.StopProcessingOffScreen, ""),
        [0xaa68] = (HopperCueKind.PlaySound, "V"),
        [0xaafe] = (HopperCueKind.ReadyToHop, ""),
    };
}
