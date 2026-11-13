using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Metroids;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>
/// The native bank-$A3 lists the Mochtroid ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class MochtroidNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MochtroidSegment>[] Entries =
    [
        new NativeProgramEntry<MochtroidSegment>(MochtroidSegment.FreeFlight, 0xa745),
        new NativeProgramEntry<MochtroidSegment>(MochtroidSegment.Attached, 0xa759),
    ];

}
