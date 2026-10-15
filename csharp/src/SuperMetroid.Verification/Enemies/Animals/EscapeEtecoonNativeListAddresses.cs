using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>
/// The native bank-$B3 lists the escape Etecoon ROM oracle (#1290) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class EscapeEtecoonNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xb3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (EscapeEtecoonProgram Program, ushort Entry)[] Entries =
    [
        (EscapeEtecoonProgram.RunningLeftLowTide, 0xe556),
        (EscapeEtecoonProgram.RunningLeftLowTideLoop, 0xe55a),
        (EscapeEtecoonProgram.RunningLeftHighTide, 0xe56e),
        (EscapeEtecoonProgram.RunningRightLowTide, 0xe582),
        (EscapeEtecoonProgram.RunningRightLowTideLoop, 0xe586),
        (EscapeEtecoonProgram.RunningRightHighTide, 0xe59a),
        (EscapeEtecoonProgram.RunningForEscape, 0xe5ae),
        (EscapeEtecoonProgram.RunningForEscapeLoop, 0xe5b2),
        (EscapeEtecoonProgram.Stationary, 0xe5c6),
        (EscapeEtecoonProgram.ExpressGratitudeThenEscape, 0xe5da),
        (EscapeEtecoonProgram.ExpressGratitudeThenEscapeLoop, 0xe5e0),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (EscapeEtecoonCueKind Kind, string Operands)> Instructions = new()
    {
        [0x806b] = (EscapeEtecoonCueKind.SetPreInstruction, "E"),
        [0x8074] = (EscapeEtecoonCueKind.ClearPreInstruction, ""),
        [0xe545] = (EscapeEtecoonCueKind.GoToIfAcidHigh, "T"),
        [0xe610] = (EscapeEtecoonCueKind.Move, "S"),
    };
}
