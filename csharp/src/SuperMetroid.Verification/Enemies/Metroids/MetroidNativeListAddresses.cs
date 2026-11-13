using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Metroids;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>
/// The native bank-$A3 lists the Metroid ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class MetroidNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MetroidSegment>[] Entries =
    [
        new NativeProgramEntry<MetroidSegment>(MetroidSegment.ChasingSamus, 0xe9cf),
        new NativeProgramEntry<MetroidSegment>(MetroidSegment.DrainingSamus, 0xea25),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, MetroidNativeListAddressesInstruction> Instructions = new()
    {
        [0xeaa5] = new MetroidNativeListAddressesInstruction(MetroidCueKind.PlayDrainingSound, ""),
        [0xeab1] = new MetroidNativeListAddressesInstruction(MetroidCueKind.PlayRandomCry, ""),
    };
}
