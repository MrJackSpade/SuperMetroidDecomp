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
    internal static readonly (NorfairRioProgram Program, ushort Entry)[] Entries =
    [
        (NorfairRioProgram.Idle, 0xc0f1),
        (NorfairRioProgram.StartDescending, 0xc107),
        (NorfairRioProgram.Descending, 0xc12f),
        (NorfairRioProgram.StartAscending, 0xc145),
        (NorfairRioProgram.Ascending, 0xc179),
        (NorfairRioProgram.FlamesAscending, 0xc18f),
        (NorfairRioProgram.FlamesDescending, 0xc1a3),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (NorfairRioCueKind Kind, string Operands)> Instructions = new()
    {
        [0xc1c9] = (NorfairRioCueKind.SwoopStartFinished, ""),
        [0xc1d4] = (NorfairRioCueKind.FlamesAt8, ""),
        [0xc1df] = (NorfairRioCueKind.FlamesAt8, ""),
        [0xc1ea] = (NorfairRioCueKind.FlamesAt12, ""),
        [0xc1f5] = (NorfairRioCueKind.FlamesAtMinus12, ""),
        [0xc200] = (NorfairRioCueKind.FlamesAt4, ""),
        [0xc20b] = (NorfairRioCueKind.FlamesAt0, ""),
        [0xc216] = (NorfairRioCueKind.FlamesAtMinus4, ""),
        [0xc221] = (NorfairRioCueKind.FlamesAtMinus12, ""),
        [0xc22c] = (NorfairRioCueKind.FlamesAtMinus16, ""),
        [0xc237] = (NorfairRioCueKind.FlamesAt12, ""),
    };
}
