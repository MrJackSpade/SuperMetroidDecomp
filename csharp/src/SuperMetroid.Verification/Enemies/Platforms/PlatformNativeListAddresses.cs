using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Platforms;

namespace SuperMetroid.Verification.Enemies.Platforms;

/// <summary>
/// The native bank-$A3 lists the Tripper or Kamer ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class PlatformNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<PlatformProgram>[] Entries =
    [
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.KamerMovingLeft, 0x9bbb),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.KamerMovingLeftLoop, 0x9bbd),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.KamerMovingRight, 0x9bd1),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.KamerMovingRightLoop, 0x9bd3),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.KamerStillLeft, 0x9be7),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.KamerStillLeftLoop, 0x9be9),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.KamerStillRight, 0x9bfd),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.KamerStillRightLoop, 0x9bff),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.TripperMovingLeft, 0x9c13),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.TripperMovingLeftLoop, 0x9c15),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.TripperMovingRight, 0x9c29),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.TripperMovingRightLoop, 0x9c2b),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.TripperStillMovingLeft, 0x9c3f),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.TripperStillMovingLeftLoop, 0x9c41),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.TripperStillMovingRight, 0x9c55),
        new NativeProgramEntry<PlatformProgram>(PlatformProgram.TripperStillMovingRightLoop, 0x9c57),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, PlatformNativeListAddressesInstruction> Instructions = new()
    {
        [0x9c6b] = new PlatformNativeListAddressesInstruction(PlatformCueKind.MoveLeft, ""),
        [0x9c76] = new PlatformNativeListAddressesInstruction(PlatformCueKind.MoveRight, ""),
        [0x9c81] = new PlatformNativeListAddressesInstruction(PlatformCueKind.MoveLeft, ""),
        [0x9c8c] = new PlatformNativeListAddressesInstruction(PlatformCueKind.MoveRight, ""),
    };
}
