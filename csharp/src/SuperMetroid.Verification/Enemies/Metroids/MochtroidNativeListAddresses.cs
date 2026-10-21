using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<MochtroidProgram>[] Entries =
    [
        new NativeProgramEntry<MochtroidProgram>(MochtroidProgram.FreeFlight, 0xa745),
        new NativeProgramEntry<MochtroidProgram>(MochtroidProgram.Attached, 0xa759),
    ];

}
