using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>
/// The native bank-$A2 lists the Norfair Rio ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class NorfairRioNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<NorfairRioProgram>[] Entries =
    [
        new NativeProgramEntry<NorfairRioProgram>(NorfairRioProgram.Idle, 0xc0f1),
        new NativeProgramEntry<NorfairRioProgram>(NorfairRioProgram.StartDescending, 0xc107),
        new NativeProgramEntry<NorfairRioProgram>(NorfairRioProgram.Descending, 0xc12f),
        new NativeProgramEntry<NorfairRioProgram>(NorfairRioProgram.StartAscending, 0xc145),
        new NativeProgramEntry<NorfairRioProgram>(NorfairRioProgram.Ascending, 0xc179),
        new NativeProgramEntry<NorfairRioProgram>(NorfairRioProgram.FlamesAscending, 0xc18f),
        new NativeProgramEntry<NorfairRioProgram>(NorfairRioProgram.FlamesDescending, 0xc1a3),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NorfairRioNativeListAddressesInstruction> Instructions = new()
    {
        [0xc1c9] = new NorfairRioNativeListAddressesInstruction(NorfairRioCueKind.SwoopStartFinished, ""),
        [0xc1d4] = new NorfairRioNativeListAddressesInstruction(NorfairRioCueKind.FlamesAt8, ""),
        [0xc1df] = new NorfairRioNativeListAddressesInstruction(NorfairRioCueKind.FlamesAt8, ""),
        [0xc1ea] = new NorfairRioNativeListAddressesInstruction(NorfairRioCueKind.FlamesAt12, ""),
        [0xc1f5] = new NorfairRioNativeListAddressesInstruction(NorfairRioCueKind.FlamesAtMinus12, ""),
        [0xc200] = new NorfairRioNativeListAddressesInstruction(NorfairRioCueKind.FlamesAt4, ""),
        [0xc20b] = new NorfairRioNativeListAddressesInstruction(NorfairRioCueKind.FlamesAt0, ""),
        [0xc216] = new NorfairRioNativeListAddressesInstruction(NorfairRioCueKind.FlamesAtMinus4, ""),
        [0xc221] = new NorfairRioNativeListAddressesInstruction(NorfairRioCueKind.FlamesAtMinus12, ""),
        [0xc22c] = new NorfairRioNativeListAddressesInstruction(NorfairRioCueKind.FlamesAtMinus16, ""),
        [0xc237] = new NorfairRioNativeListAddressesInstruction(NorfairRioCueKind.FlamesAt12, ""),
    };
}
