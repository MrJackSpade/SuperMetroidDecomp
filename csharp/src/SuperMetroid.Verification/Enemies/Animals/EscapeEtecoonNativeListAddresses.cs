using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<EscapeEtecoonProgram>[] Entries =
    [
        new NativeProgramEntry<EscapeEtecoonProgram>(EscapeEtecoonProgram.RunningLeftLowTide, 0xe556),
        new NativeProgramEntry<EscapeEtecoonProgram>(EscapeEtecoonProgram.RunningLeftLowTideLoop, 0xe55a),
        new NativeProgramEntry<EscapeEtecoonProgram>(EscapeEtecoonProgram.RunningLeftHighTide, 0xe56e),
        new NativeProgramEntry<EscapeEtecoonProgram>(EscapeEtecoonProgram.RunningRightLowTide, 0xe582),
        new NativeProgramEntry<EscapeEtecoonProgram>(EscapeEtecoonProgram.RunningRightLowTideLoop, 0xe586),
        new NativeProgramEntry<EscapeEtecoonProgram>(EscapeEtecoonProgram.RunningRightHighTide, 0xe59a),
        new NativeProgramEntry<EscapeEtecoonProgram>(EscapeEtecoonProgram.RunningForEscape, 0xe5ae),
        new NativeProgramEntry<EscapeEtecoonProgram>(EscapeEtecoonProgram.RunningForEscapeLoop, 0xe5b2),
        new NativeProgramEntry<EscapeEtecoonProgram>(EscapeEtecoonProgram.Stationary, 0xe5c6),
        new NativeProgramEntry<EscapeEtecoonProgram>(EscapeEtecoonProgram.ExpressGratitudeThenEscape, 0xe5da),
        new NativeProgramEntry<EscapeEtecoonProgram>(EscapeEtecoonProgram.ExpressGratitudeThenEscapeLoop, 0xe5e0),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<EscapeEtecoonCue>> Instructions = new()
    {
        [0x806b] = NativeCueShape<EscapeEtecoonCue>.Of<EscapeEtecoonCue.SetPreInstruction>("E"),
        [0x8074] = NativeCueShape<EscapeEtecoonCue>.Exactly(new EscapeEtecoonCue.Command(EscapeEtecoonCueKind.ClearPreInstruction)),
        [0xe545] = NativeCueShape<EscapeEtecoonCue>.Of<EscapeEtecoonCue.GoToIfAcidHigh>("T"),
        [0xe610] = NativeCueShape<EscapeEtecoonCue>.Of<EscapeEtecoonCue.Move>("S"),
    };
}
