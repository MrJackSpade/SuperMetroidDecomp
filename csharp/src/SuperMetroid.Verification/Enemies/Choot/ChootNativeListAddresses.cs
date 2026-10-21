using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Choot;

namespace SuperMetroid.Verification.Enemies.Choot;

/// <summary>
/// The native bank-$A2 lists the Choot ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class ChootNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<ChootProgram>[] Entries =
    [
        new NativeProgramEntry<ChootProgram>(ChootProgram.Idle, 0xd82c),
        new NativeProgramEntry<ChootProgram>(ChootProgram.Jumping, 0xd834),
        new NativeProgramEntry<ChootProgram>(ChootProgram.Falling, 0xd840),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, ChootNativeListAddressesInstruction> Instructions = new()
    {
        [0x8173] = new ChootNativeListAddressesInstruction(ChootCueKind.ProcessOffScreen, ""),
        [0x817d] = new ChootNativeListAddressesInstruction(ChootCueKind.StopProcessingOffScreen, ""),
    };
}
