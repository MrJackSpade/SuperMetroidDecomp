using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Yapping Maw body link ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class YappingMawBodyProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<YappingMawBodyProjectileSegment>[] Entries =
    [
        new NativeProgramEntry<YappingMawBodyProjectileSegment>(YappingMawBodyProjectileSegment.FacingDown, 0xec56),
        new NativeProgramEntry<YappingMawBodyProjectileSegment>(YappingMawBodyProjectileSegment.FacingUp, 0xec5c),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:EC56-EC61, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xec56, 0xec62);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(YappingMawBodyProjectileSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, YappingMawBodyProjectileAnimations.Segments, YappingMawBodyProjectileAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
