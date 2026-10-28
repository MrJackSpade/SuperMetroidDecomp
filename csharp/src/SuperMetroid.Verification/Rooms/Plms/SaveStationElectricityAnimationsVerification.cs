using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Rooms.Plms.SaveStationElectricityNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies.SaveStation;
using SuperMetroid.Core.Enemies.SaveStation.Artwork;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>#1296: the save-station electricity's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class SaveStationElectricityAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, SaveStationElectricityAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(SaveStationElectricityFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySaveStationElectricityAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  save-station electricity programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
