using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.Core.Enemies.KiHunters;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Ki-Hunter acid spit ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class KiHunterAcidSpitNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<KiHunterAcidSpitSegment>[] Entries =
    [
        new NativeProgramEntry<KiHunterAcidSpitSegment>(KiHunterAcidSpitSegment.Left, 0xcf34),
        new NativeProgramEntry<KiHunterAcidSpitSegment>(KiHunterAcidSpitSegment.HitFloor, 0xcf56),
        new NativeProgramEntry<KiHunterAcidSpitSegment>(KiHunterAcidSpitSegment.Right, 0xcf6e),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:CF34-CF8F, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xcf34, 0xcf90);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, KiHunterAcidSpitAnimations.Segments, KiHunterAcidSpitAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
