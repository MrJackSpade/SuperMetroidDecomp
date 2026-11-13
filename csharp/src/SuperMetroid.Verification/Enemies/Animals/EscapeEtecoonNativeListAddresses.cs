using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<EscapeEtecoonSegment>[] Entries =
    [
        new NativeProgramEntry<EscapeEtecoonSegment>(EscapeEtecoonSegment.RunningLeftLowTide, 0xe556),
        new NativeProgramEntry<EscapeEtecoonSegment>(EscapeEtecoonSegment.RunningLeftLowTideLoop, 0xe55a),
        new NativeProgramEntry<EscapeEtecoonSegment>(EscapeEtecoonSegment.RunningLeftHighTide, 0xe56e),
        new NativeProgramEntry<EscapeEtecoonSegment>(EscapeEtecoonSegment.RunningRightLowTide, 0xe582),
        new NativeProgramEntry<EscapeEtecoonSegment>(EscapeEtecoonSegment.RunningRightLowTideLoop, 0xe586),
        new NativeProgramEntry<EscapeEtecoonSegment>(EscapeEtecoonSegment.RunningRightHighTide, 0xe59a),
        new NativeProgramEntry<EscapeEtecoonSegment>(EscapeEtecoonSegment.RunningForEscape, 0xe5ae),
        new NativeProgramEntry<EscapeEtecoonSegment>(EscapeEtecoonSegment.RunningForEscapeLoop, 0xe5b2),
        new NativeProgramEntry<EscapeEtecoonSegment>(EscapeEtecoonSegment.Stationary, 0xe5c6),
        new NativeProgramEntry<EscapeEtecoonSegment>(EscapeEtecoonSegment.ExpressGratitudeThenEscape, 0xe5da),
        new NativeProgramEntry<EscapeEtecoonSegment>(EscapeEtecoonSegment.ExpressGratitudeThenEscapeLoop, 0xe5e0),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<EscapeEtecoonCue>> Instructions = new()
    {
        [0x806b] = NativeCueShape<EscapeEtecoonCue>.Of<EscapeEtecoonCue.SetFrameBehavior>("E"),
        [0x8074] = NativeCueShape<EscapeEtecoonCue>.Exactly(new EscapeEtecoonCue.Signal(EscapeEtecoonCueKind.ClearFrameBehavior)),
        [0xe545] = NativeCueShape<EscapeEtecoonCue>.Of<EscapeEtecoonCue.GoToIfAcidHigh>("T"),
        [0xe610] = NativeCueShape<EscapeEtecoonCue>.Of<EscapeEtecoonCue.Move>("S"),
    };
}
