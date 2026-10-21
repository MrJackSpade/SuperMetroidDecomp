using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<EscapeDachoraProgram>[] Entries =
    [
        new NativeProgramEntry<EscapeDachoraProgram>(EscapeDachoraProgram.RunningAroundLowTide, 0xe964),
        new NativeProgramEntry<EscapeDachoraProgram>(EscapeDachoraProgram.RunningAroundLowTideLeft, 0xe968),
        new NativeProgramEntry<EscapeDachoraProgram>(EscapeDachoraProgram.RunningAroundLowTideRight, 0xe99c),
        new NativeProgramEntry<EscapeDachoraProgram>(EscapeDachoraProgram.RunningAroundHighTide, 0xe9d0),
        new NativeProgramEntry<EscapeDachoraProgram>(EscapeDachoraProgram.RunningAroundHighTideLeft, 0xe9d4),
        new NativeProgramEntry<EscapeDachoraProgram>(EscapeDachoraProgram.RunningAroundHighTideLeftLoop, 0xe9fc),
        new NativeProgramEntry<EscapeDachoraProgram>(EscapeDachoraProgram.RunningAroundHighTideRight, 0xea04),
        new NativeProgramEntry<EscapeDachoraProgram>(EscapeDachoraProgram.RunningAroundHighTideRightLoop, 0xea2c),
        new NativeProgramEntry<EscapeDachoraProgram>(EscapeDachoraProgram.RunningForEscape, 0xea34),
        new NativeProgramEntry<EscapeDachoraProgram>(EscapeDachoraProgram.RunningForEscapeAccelerating, 0xea38),
        new NativeProgramEntry<EscapeDachoraProgram>(EscapeDachoraProgram.RunningForEscapeMaximumSpeed, 0xea80),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, EscapeDachoraNativeListAddressesInstruction> Instructions = new()
    {
        [0xeaa8] = new EscapeDachoraNativeListAddressesInstruction(EscapeDachoraCueKind.GoToIfAcidHigh, "T"),
        [0xeab8] = new EscapeDachoraNativeListAddressesInstruction(EscapeDachoraCueKind.GoToIfCrittersEscaped, "T"),
        [0xeac9] = new EscapeDachoraNativeListAddressesInstruction(EscapeDachoraCueKind.MoveLeft, ""),
        [0xead7] = new EscapeDachoraNativeListAddressesInstruction(EscapeDachoraCueKind.MoveRight, ""),
    };
}
