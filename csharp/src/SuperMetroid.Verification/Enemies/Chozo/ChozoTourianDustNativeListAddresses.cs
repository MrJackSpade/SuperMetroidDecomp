using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>
/// The native bank-$86 lists the Chozo/Tourian dust ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class ChozoTourianDustNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (ChozoTourianDustProgram Program, ushort Entry)[] Entries =
    [
        (ChozoTourianDustProgram.Footsteps, 0xaec4),
        (ChozoTourianDustProgram.SpikeClearingExplosions, 0xaedc),
        (ChozoTourianDustProgram.TourianDescentDust, 0xaf14),
        (ChozoTourianDustProgram.TourianDescentDustLoop, 0xaf18),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:AEC4-AF35, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xaec4, 0xaf36);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(ChozoTourianDustProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
