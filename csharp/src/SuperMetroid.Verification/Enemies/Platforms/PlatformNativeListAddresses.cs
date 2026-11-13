using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<PlatformSegment>[] Entries =
    [
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.KamerMovingLeft, 0x9bbb),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.KamerMovingLeftLoop, 0x9bbd),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.KamerMovingRight, 0x9bd1),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.KamerMovingRightLoop, 0x9bd3),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.KamerStillLeft, 0x9be7),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.KamerStillLeftLoop, 0x9be9),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.KamerStillRight, 0x9bfd),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.KamerStillRightLoop, 0x9bff),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.TripperMovingLeft, 0x9c13),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.TripperMovingLeftLoop, 0x9c15),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.TripperMovingRight, 0x9c29),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.TripperMovingRightLoop, 0x9c2b),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.TripperStillMovingLeft, 0x9c3f),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.TripperStillMovingLeftLoop, 0x9c41),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.TripperStillMovingRight, 0x9c55),
        new NativeProgramEntry<PlatformSegment>(PlatformSegment.TripperStillMovingRightLoop, 0x9c57),
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
