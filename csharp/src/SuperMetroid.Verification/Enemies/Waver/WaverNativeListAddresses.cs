using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Waver;

namespace SuperMetroid.Verification.Enemies.Waver;

/// <summary>
/// The native bank-$A3 lists the Waver ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class WaverNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<WaverProgram>[] Entries =
    [
        new NativeProgramEntry<WaverProgram>(WaverProgram.SteadyFacingLeft, 0x86a7),
        new NativeProgramEntry<WaverProgram>(WaverProgram.SteadyFacingRight, 0x86ad),
        new NativeProgramEntry<WaverProgram>(WaverProgram.SpinningFacingLeft, 0x86b3),
        new NativeProgramEntry<WaverProgram>(WaverProgram.SpinningFacingRight, 0x86c7),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, WaverNativeListAddressesInstruction> Instructions = new()
    {
        [0x86e3] = new WaverNativeListAddressesInstruction(WaverCueKind.FinishSpin, ""),
    };
}
