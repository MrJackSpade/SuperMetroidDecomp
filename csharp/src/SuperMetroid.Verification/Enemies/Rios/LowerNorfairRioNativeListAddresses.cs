using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>
/// The native bank-$A2 lists the Lower Norfair Rio ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class LowerNorfairRioNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<LowerNorfairRioSegment>[] Entries =
    [
        new NativeProgramEntry<LowerNorfairRioSegment>(LowerNorfairRioSegment.Idle, 0xc61a),
        new NativeProgramEntry<LowerNorfairRioSegment>(LowerNorfairRioSegment.IdleLoop, 0xc61c),
        new NativeProgramEntry<LowerNorfairRioSegment>(LowerNorfairRioSegment.PrepareToSwoop, 0xc630),
        new NativeProgramEntry<LowerNorfairRioSegment>(LowerNorfairRioSegment.Descending, 0xc65a),
        new NativeProgramEntry<LowerNorfairRioSegment>(LowerNorfairRioSegment.AscendingPart1, 0xc662),
        new NativeProgramEntry<LowerNorfairRioSegment>(LowerNorfairRioSegment.AscendingPart2, 0xc674),
        new NativeProgramEntry<LowerNorfairRioSegment>(LowerNorfairRioSegment.AscendingPart2Loop, 0xc676),
        new NativeProgramEntry<LowerNorfairRioSegment>(LowerNorfairRioSegment.Cooldown, 0xc686),
        new NativeProgramEntry<LowerNorfairRioSegment>(LowerNorfairRioSegment.Flames, 0xc6b0),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, LowerNorfairRioNativeListAddressesInstruction> Instructions = new()
    {
        [0xc6d2] = new LowerNorfairRioNativeListAddressesInstruction(LowerNorfairRioCueKind.AnimationFinished, ""),
        [0xc6dd] = new LowerNorfairRioNativeListAddressesInstruction(LowerNorfairRioCueKind.HideFlames, ""),
        [0xc6e8] = new LowerNorfairRioNativeListAddressesInstruction(LowerNorfairRioCueKind.ShowFlames, ""),
    };
}
