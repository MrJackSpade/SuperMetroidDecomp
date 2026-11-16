using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Enemies.Cacatac;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Cacatac spike ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class CacatacProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<CacatacSpikeDirection>[] Entries =
    [
        new NativeProgramEntry<CacatacSpikeDirection>(CacatacSpikeDirection.LeftFacingUp, 0xd92e),
        new NativeProgramEntry<CacatacSpikeDirection>(CacatacSpikeDirection.UpLeft, 0xd934),
        new NativeProgramEntry<CacatacSpikeDirection>(CacatacSpikeDirection.Up, 0xd93a),
        new NativeProgramEntry<CacatacSpikeDirection>(CacatacSpikeDirection.UpRight, 0xd940),
        new NativeProgramEntry<CacatacSpikeDirection>(CacatacSpikeDirection.RightFacingUp, 0xd946),
        new NativeProgramEntry<CacatacSpikeDirection>(CacatacSpikeDirection.LeftFacingDown, 0xd94c),
        new NativeProgramEntry<CacatacSpikeDirection>(CacatacSpikeDirection.DownLeft, 0xd952),
        new NativeProgramEntry<CacatacSpikeDirection>(CacatacSpikeDirection.Down, 0xd958),
        new NativeProgramEntry<CacatacSpikeDirection>(CacatacSpikeDirection.DownRight, 0xd95e),
        new NativeProgramEntry<CacatacSpikeDirection>(CacatacSpikeDirection.RightFacingDown, 0xd964),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:D92E-D969, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xd92e, 0xd96a);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(CacatacSpikeDirection program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, CacatacProjectileAnimations.Segments, CacatacProjectileAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
