using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<HopperSegment>[] Entries =
    [
        new NativeProgramEntry<HopperSegment>(HopperSegment.SidehopperFloorJumping, 0xaa76),
        new NativeProgramEntry<HopperSegment>(HopperSegment.SidehopperFloorLanded, 0xaa82),
        new NativeProgramEntry<HopperSegment>(HopperSegment.SidehopperCeilingJumping, 0xaa9c),
        new NativeProgramEntry<HopperSegment>(HopperSegment.SidehopperCeilingLanded, 0xaaa8),
        new NativeProgramEntry<HopperSegment>(HopperSegment.DessgeegaFloorJumping, 0xafa5),
        new NativeProgramEntry<HopperSegment>(HopperSegment.DessgeegaFloorLanded, 0xafad),
        new NativeProgramEntry<HopperSegment>(HopperSegment.DessgeegaCeilingJumping, 0xafc3),
        new NativeProgramEntry<HopperSegment>(HopperSegment.DessgeegaCeilingLanded, 0xafcb),
        new NativeProgramEntry<HopperSegment>(HopperSegment.LargeSidehopperFloorJumping, 0xb0c5),
        new NativeProgramEntry<HopperSegment>(HopperSegment.LargeSidehopperFloorLanded, 0xb0d1),
        new NativeProgramEntry<HopperSegment>(HopperSegment.LargeSidehopperCeilingJumping, 0xb0eb),
        new NativeProgramEntry<HopperSegment>(HopperSegment.LargeSidehopperCeilingLanded, 0xb0f7),
        new NativeProgramEntry<HopperSegment>(HopperSegment.LargeDessgeegaFloorJumping, 0xb237),
        new NativeProgramEntry<HopperSegment>(HopperSegment.LargeDessgeegaFloorLanded, 0xb23f),
        new NativeProgramEntry<HopperSegment>(HopperSegment.LargeDessgeegaCeilingJumping, 0xb255),
        new NativeProgramEntry<HopperSegment>(HopperSegment.LargeDessgeegaCeilingLanded, 0xb25d),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, HopperNativeListAddressesInstruction> Instructions = new()
    {
        [0x8173] = new HopperNativeListAddressesInstruction(HopperCueKind.ProcessOffScreen, ""),
        [0x817d] = new HopperNativeListAddressesInstruction(HopperCueKind.StopProcessingOffScreen, ""),
        [0xaa68] = new HopperNativeListAddressesInstruction(HopperCueKind.PlaySound, "V"),
        [0xaafe] = new HopperNativeListAddressesInstruction(HopperCueKind.ReadyToHop, ""),
    };
}
