using static SuperMetroid.Verification.Enemies.KiHunters.KiHunterAcidSpitNativeListAddresses;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>#641: the Ki-Hunter's acid spit typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class KiHunterAcidSpitAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyKiHunterAcidSpitAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, KiHunterAcidSpitAnimations.Programs,
            KiHunterAcidSpitFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        Console.WriteLine($"  Ki-Hunter acid spit programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
