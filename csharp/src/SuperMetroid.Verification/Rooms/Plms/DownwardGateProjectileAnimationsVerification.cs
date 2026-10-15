using static SuperMetroid.Verification.Rooms.Plms.DownwardGateProjectileNativeListAddresses;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Plms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>#641: the downward shot gate's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class DownwardGateProjectileAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, DownwardGateProjectileAnimations.Programs, DownwardGateProjectileFrameDefinitions.PresentationKey, DownwardGateProjectileCueRules.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyDownwardGateProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  downward gate programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
