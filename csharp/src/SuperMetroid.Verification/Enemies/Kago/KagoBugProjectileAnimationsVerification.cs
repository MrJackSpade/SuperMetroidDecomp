using static SuperMetroid.Verification.Enemies.Kago.KagoBugProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.Kago.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Kago;

/// <summary>#641: the Kago's bugs typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class KagoBugProjectileAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, KagoBugProjectileAnimations.Programs, KagoBugProjectileFrameDefinitions.PresentationKey, KagoBugProjectileCueRules.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyKagoBugProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Kago bug programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
