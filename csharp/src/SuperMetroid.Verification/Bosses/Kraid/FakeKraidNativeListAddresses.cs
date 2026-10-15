using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// The native bank-$A6 lists the Mini-Kraid ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class FakeKraidNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (FakeKraidProgram Program, ushort Entry)[] Entries =
    [
        (FakeKraidProgram.ChooseActionFacingLeft, 0x99ac),
        (FakeKraidProgram.StepForwardsFacingLeft, 0x99ae),
        (FakeKraidProgram.ChooseAlternateActionFacingLeft, 0x99c4),
        (FakeKraidProgram.StepBackwardsFacingLeft, 0x99c6),
        (FakeKraidProgram.FireSpitFacingLeft, 0x99dc),
        (FakeKraidProgram.ChooseActionFacingRight, 0x99fa),
        (FakeKraidProgram.StepForwardsFacingRight, 0x99fc),
        (FakeKraidProgram.ChooseAlternateActionFacingRight, 0x9a12),
        (FakeKraidProgram.StepBackwardsFacingRight, 0x9a14),
        (FakeKraidProgram.FireSpitFacingRight, 0x9a2a),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (FakeKraidCueKind Kind, string Operands)> Instructions = new()
    {
        [0x9b26] = (FakeKraidCueKind.Move, ""),
        [0x9b74] = (FakeKraidCueKind.ChooseAction, ""),
        [0x9bb2] = (FakeKraidCueKind.PlayCrySound, ""),
        [0x9bc4] = (FakeKraidCueKind.SpitLeft, ""),
        [0x9c02] = (FakeKraidCueKind.SpitRight, ""),
    };
}
