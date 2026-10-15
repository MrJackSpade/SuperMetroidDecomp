using SuperMetroid.Core.Enemies.Skultera;

namespace SuperMetroid.Verification.Enemies.Skultera;

/// <summary>
/// The native bank-$A3 lists the Skultera ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class SkulteraNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (SkulteraProgram Program, ushort Entry)[] Entries =
    [
        (SkulteraProgram.SwimmingLeft, 0x902a),
        (SkulteraProgram.SwimmingLeftLoop, 0x902c),
        (SkulteraProgram.TurningRight, 0x903c),
        (SkulteraProgram.SwimmingRight, 0x9060),
        (SkulteraProgram.SwimmingRightLoop, 0x9062),
        (SkulteraProgram.TurningLeft, 0x9072),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (SkulteraCueKind Kind, string Operands)> Instructions = new()
    {
        [0x9096] = (SkulteraCueKind.DrawAboveScenery, ""),
        [0x90a0] = (SkulteraCueKind.DrawBelowScenery, ""),
        [0x90aa] = (SkulteraCueKind.FinishTurn, ""),
    };
}
