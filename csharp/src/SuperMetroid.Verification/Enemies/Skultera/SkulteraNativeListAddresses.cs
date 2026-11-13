using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<SkulteraSegment>[] Entries =
    [
        new NativeProgramEntry<SkulteraSegment>(SkulteraSegment.SwimmingLeft, 0x902a),
        new NativeProgramEntry<SkulteraSegment>(SkulteraSegment.SwimmingLeftLoop, 0x902c),
        new NativeProgramEntry<SkulteraSegment>(SkulteraSegment.TurningRight, 0x903c),
        new NativeProgramEntry<SkulteraSegment>(SkulteraSegment.SwimmingRight, 0x9060),
        new NativeProgramEntry<SkulteraSegment>(SkulteraSegment.SwimmingRightLoop, 0x9062),
        new NativeProgramEntry<SkulteraSegment>(SkulteraSegment.TurningLeft, 0x9072),
    ];

    /// <summary>
    /// The native routines that set the fish's sprite draw layer (#1602): their only effect was the order its sprites were
    /// written in, which the port does not keep, so its animations have no event for them.
    /// </summary>
    internal static readonly HashSet<ushort> OmittedInstructions = [0x9096, 0x90a0];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, SkulteraNativeListAddressesInstruction> Instructions = new()
    {
        [0x90aa] = new SkulteraNativeListAddressesInstruction(SkulteraCueKind.FinishTurn, ""),
    };
}
