using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<FakeKraidProgram>[] Entries =
    [
        new NativeProgramEntry<FakeKraidProgram>(FakeKraidProgram.ChooseActionFacingLeft, 0x99ac),
        new NativeProgramEntry<FakeKraidProgram>(FakeKraidProgram.StepForwardsFacingLeft, 0x99ae),
        new NativeProgramEntry<FakeKraidProgram>(FakeKraidProgram.ChooseAlternateActionFacingLeft, 0x99c4),
        new NativeProgramEntry<FakeKraidProgram>(FakeKraidProgram.StepBackwardsFacingLeft, 0x99c6),
        new NativeProgramEntry<FakeKraidProgram>(FakeKraidProgram.FireSpitFacingLeft, 0x99dc),
        new NativeProgramEntry<FakeKraidProgram>(FakeKraidProgram.ChooseActionFacingRight, 0x99fa),
        new NativeProgramEntry<FakeKraidProgram>(FakeKraidProgram.StepForwardsFacingRight, 0x99fc),
        new NativeProgramEntry<FakeKraidProgram>(FakeKraidProgram.ChooseAlternateActionFacingRight, 0x9a12),
        new NativeProgramEntry<FakeKraidProgram>(FakeKraidProgram.StepBackwardsFacingRight, 0x9a14),
        new NativeProgramEntry<FakeKraidProgram>(FakeKraidProgram.FireSpitFacingRight, 0x9a2a),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, FakeKraidNativeListAddressesInstruction> Instructions = new()
    {
        [0x9b26] = new FakeKraidNativeListAddressesInstruction(FakeKraidCueKind.Move, ""),
        [0x9b74] = new FakeKraidNativeListAddressesInstruction(FakeKraidCueKind.ChooseAction, ""),
        [0x9bb2] = new FakeKraidNativeListAddressesInstruction(FakeKraidCueKind.PlayCrySound, ""),
        [0x9bc4] = new FakeKraidNativeListAddressesInstruction(FakeKraidCueKind.SpitLeft, ""),
        [0x9c02] = new FakeKraidNativeListAddressesInstruction(FakeKraidCueKind.SpitRight, ""),
    };
}
