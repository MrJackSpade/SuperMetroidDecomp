using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.FallingSparks;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the falling spark ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class FallingSparkNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<FallingSparkSegment>[] Entries =
    [
        new NativeProgramEntry<FallingSparkSegment>(FallingSparkSegment.Falling, 0xf353),
        new NativeProgramEntry<FallingSparkSegment>(FallingSparkSegment.HitFloor, 0xf363),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:F353-F390, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xf353, 0xf391);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, FallingSparkAnimations.Segments, FallingSparkAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
