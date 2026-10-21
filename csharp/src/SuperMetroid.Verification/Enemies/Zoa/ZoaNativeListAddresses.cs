using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Zoa;

namespace SuperMetroid.Verification.Enemies.Zoa;

/// <summary>
/// The native bank-$A3 lists the Zoa ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class ZoaNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<ZoaProgram>[] Entries =
    [
        new NativeProgramEntry<ZoaProgram>(ZoaProgram.FacingLeftShooting, 0xb3c1),
        new NativeProgramEntry<ZoaProgram>(ZoaProgram.FacingLeftRising, 0xb3d7),
        new NativeProgramEntry<ZoaProgram>(ZoaProgram.FacingRightShooting, 0xb3e7),
        new NativeProgramEntry<ZoaProgram>(ZoaProgram.FacingRightRising, 0xb3fd),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, ZoaNativeListAddressesInstruction> Instructions = new()
    {
        [0xb429] = new ZoaNativeListAddressesInstruction(ZoaCueKind.SetSpeedIndex4, ""),
        [0xb434] = new ZoaNativeListAddressesInstruction(ZoaCueKind.SetSpeedIndex8, ""),
        [0xb43f] = new ZoaNativeListAddressesInstruction(ZoaCueKind.SetSpeedIndex12, ""),
    };
}
