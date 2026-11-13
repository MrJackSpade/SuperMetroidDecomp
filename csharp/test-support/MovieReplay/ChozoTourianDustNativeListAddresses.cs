using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Chozo/Tourian dust ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class ChozoTourianDustNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<ChozoTourianDustSegment>[] Entries =
    [
        new NativeProgramEntry<ChozoTourianDustSegment>(ChozoTourianDustSegment.Footsteps, 0xaec4),
        new NativeProgramEntry<ChozoTourianDustSegment>(ChozoTourianDustSegment.SpikeClearingExplosions, 0xaedc),
        new NativeProgramEntry<ChozoTourianDustSegment>(ChozoTourianDustSegment.TourianDescentDust, 0xaf14),
        new NativeProgramEntry<ChozoTourianDustSegment>(ChozoTourianDustSegment.TourianDescentDustLoop, 0xaf18),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:AEC4-AF35, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xaec4, 0xaf36);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(ChozoTourianDustSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, ChozoTourianDustAnimations.Segments, ChozoTourianDustAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
