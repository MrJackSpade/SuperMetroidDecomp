using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.SaveStation;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the save-station electricity ROM oracle (#1296) walks: each program's entry point. Only verification reads
/// them; the port runs typed programs.
/// </summary>
internal static class SaveStationElectricityNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<SaveStationElectricitySegment>[] Entries =
    [
        new NativeProgramEntry<SaveStationElectricitySegment>(SaveStationElectricitySegment.Start, 0xe683),
        new NativeProgramEntry<SaveStationElectricitySegment>(SaveStationElectricitySegment.Cycle, 0xe687),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xe683, 0xe6ad);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(SaveStationElectricitySegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, SaveStationElectricityAnimations.Segments, SaveStationElectricityAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
