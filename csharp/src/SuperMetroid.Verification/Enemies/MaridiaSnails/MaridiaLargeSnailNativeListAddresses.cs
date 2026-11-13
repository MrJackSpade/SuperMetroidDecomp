using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.MaridiaSnails;

namespace SuperMetroid.Verification.Enemies.MaridiaSnails;

/// <summary>
/// The native bank-$A2 lists the Oum ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class MaridiaLargeSnailNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MaridiaLargeSnailSegment>[] Entries =
    [
        new NativeProgramEntry<MaridiaLargeSnailSegment>(MaridiaLargeSnailSegment.FacingLeftIdle, 0xca4b),
        new NativeProgramEntry<MaridiaLargeSnailSegment>(MaridiaLargeSnailSegment.FacingLeftAttacking, 0xca51),
        new NativeProgramEntry<MaridiaLargeSnailSegment>(MaridiaLargeSnailSegment.FacingLeftRollingForwards, 0xca8b),
        new NativeProgramEntry<MaridiaLargeSnailSegment>(MaridiaLargeSnailSegment.FacingLeftRollingBackwards, 0xcab3),
        new NativeProgramEntry<MaridiaLargeSnailSegment>(MaridiaLargeSnailSegment.FacingRightIdle, 0xcadb),
        new NativeProgramEntry<MaridiaLargeSnailSegment>(MaridiaLargeSnailSegment.FacingRightAttacking, 0xcae1),
        new NativeProgramEntry<MaridiaLargeSnailSegment>(MaridiaLargeSnailSegment.FacingRightRollingForwards, 0xcb1b),
        new NativeProgramEntry<MaridiaLargeSnailSegment>(MaridiaLargeSnailSegment.FacingRightRollingBackwards, 0xcb43),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, MaridiaLargeSnailNativeListAddressesInstruction> Instructions = new()
    {
        [0xcb6b] = new MaridiaLargeSnailNativeListAddressesInstruction(MaridiaLargeSnailCueKind.PlaySplash, ""),
        [0xccb3] = new MaridiaLargeSnailNativeListAddressesInstruction(MaridiaLargeSnailCueKind.FinishAttack, ""),
        [0xccbe] = new MaridiaLargeSnailNativeListAddressesInstruction(MaridiaLargeSnailCueKind.AllowAttackRotation, ""),
        [0xccc9] = new MaridiaLargeSnailNativeListAddressesInstruction(MaridiaLargeSnailCueKind.DisallowAttackRotation, ""),
    };
}
