using static SuperMetroid.Verification.Enemies.KiHunters.KiHunterAcidSpitNativeListAddresses;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>#641: the Ki-Hunter's acid spit typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class KiHunterAcidSpitAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, KiHunterAcidSpitAnimations.Programs, (frame => EnemyProjectileFrames.ProgramFrameKey(KiHunterAcidSpitFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyKiHunterAcidSpitAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Ki-Hunter acid spit programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
