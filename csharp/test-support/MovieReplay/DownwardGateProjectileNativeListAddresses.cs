using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the downward gate ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class DownwardGateProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<DownwardGateProjectileSegment>[] Entries =
    [
        new NativeProgramEntry<DownwardGateProjectileSegment>(DownwardGateProjectileSegment.Moving, 0xe53c),
        new NativeProgramEntry<DownwardGateProjectileSegment>(DownwardGateProjectileSegment.MovedOneBlock, 0xe54a),
        new NativeProgramEntry<DownwardGateProjectileSegment>(DownwardGateProjectileSegment.MovedTwoBlocks, 0xe550),
        new NativeProgramEntry<DownwardGateProjectileSegment>(DownwardGateProjectileSegment.MovedThreeBlocks, 0xe556),
        new NativeProgramEntry<DownwardGateProjectileSegment>(DownwardGateProjectileSegment.Stopping, 0xe55c),
        new NativeProgramEntry<DownwardGateProjectileSegment>(DownwardGateProjectileSegment.Closed, 0xe55e),
        new NativeProgramEntry<DownwardGateProjectileSegment>(DownwardGateProjectileSegment.Rising, 0xe568),
        new NativeProgramEntry<DownwardGateProjectileSegment>(DownwardGateProjectileSegment.RisenOneBlock, 0xe572),
        new NativeProgramEntry<DownwardGateProjectileSegment>(DownwardGateProjectileSegment.RisenTwoBlocks, 0xe578),
        new NativeProgramEntry<DownwardGateProjectileSegment>(DownwardGateProjectileSegment.RisenThreeBlocks, 0xe57e),
        new NativeProgramEntry<DownwardGateProjectileSegment>(DownwardGateProjectileSegment.Vanishing, 0xe584),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:E53C-E585, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xe53c, 0xe586);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(DownwardGateProjectileSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, DownwardGateProjectileAnimations.Segments, DownwardGateProjectileAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), DownwardGateProjectileCueRules.Rules);
}
