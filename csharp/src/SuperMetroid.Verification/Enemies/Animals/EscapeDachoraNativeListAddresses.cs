using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>
/// The native bank-$B3 lists the escape Dachora ROM oracle (#1290) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class EscapeDachoraNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xb3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (EscapeDachoraProgram Program, ushort Entry)[] Entries =
    [
        (EscapeDachoraProgram.RunningAroundLowTide, 0xe964),
        (EscapeDachoraProgram.RunningAroundLowTideLeft, 0xe968),
        (EscapeDachoraProgram.RunningAroundLowTideRight, 0xe99c),
        (EscapeDachoraProgram.RunningAroundHighTide, 0xe9d0),
        (EscapeDachoraProgram.RunningAroundHighTideLeft, 0xe9d4),
        (EscapeDachoraProgram.RunningAroundHighTideLeftLoop, 0xe9fc),
        (EscapeDachoraProgram.RunningAroundHighTideRight, 0xea04),
        (EscapeDachoraProgram.RunningAroundHighTideRightLoop, 0xea2c),
        (EscapeDachoraProgram.RunningForEscape, 0xea34),
        (EscapeDachoraProgram.RunningForEscapeAccelerating, 0xea38),
        (EscapeDachoraProgram.RunningForEscapeMaximumSpeed, 0xea80),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (EscapeDachoraCueKind Kind, string Operands)> Instructions = new()
    {
        [0xeaa8] = (EscapeDachoraCueKind.GoToIfAcidHigh, "T"),
        [0xeab8] = (EscapeDachoraCueKind.GoToIfCrittersEscaped, "T"),
        [0xeac9] = (EscapeDachoraCueKind.MoveLeft, ""),
        [0xead7] = (EscapeDachoraCueKind.MoveRight, ""),
    };
}
