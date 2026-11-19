using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.MaridiaSnails;

namespace SuperMetroid.Verification.Enemies.MaridiaSnails;

/// <summary>
/// The native bank-$A2 lists the Oum ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class OumNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<OumSegment>[] Entries =
    [
        new NativeProgramEntry<OumSegment>(OumSegment.FacingLeftIdle, 0xca4b),
        new NativeProgramEntry<OumSegment>(OumSegment.FacingLeftAttacking, 0xca51),
        new NativeProgramEntry<OumSegment>(OumSegment.FacingLeftRollingForwards, 0xca8b),
        new NativeProgramEntry<OumSegment>(OumSegment.FacingLeftRollingBackwards, 0xcab3),
        new NativeProgramEntry<OumSegment>(OumSegment.FacingRightIdle, 0xcadb),
        new NativeProgramEntry<OumSegment>(OumSegment.FacingRightAttacking, 0xcae1),
        new NativeProgramEntry<OumSegment>(OumSegment.FacingRightRollingForwards, 0xcb1b),
        new NativeProgramEntry<OumSegment>(OumSegment.FacingRightRollingBackwards, 0xcb43),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, OumNativeListAddressesInstruction> Instructions = new()
    {
        [0xcb6b] = new OumNativeListAddressesInstruction(OumCueKind.PlaySplash, ""),
        [0xccb3] = new OumNativeListAddressesInstruction(OumCueKind.FinishAttack, ""),
        [0xccbe] = new OumNativeListAddressesInstruction(OumCueKind.AllowAttackRotation, ""),
        [0xccc9] = new OumNativeListAddressesInstruction(OumCueKind.DisallowAttackRotation, ""),
    };
}
