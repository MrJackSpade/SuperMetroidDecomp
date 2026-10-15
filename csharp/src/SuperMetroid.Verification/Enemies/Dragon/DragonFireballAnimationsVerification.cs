using static SuperMetroid.Verification.Enemies.Dragon.DragonFireballNativeListAddresses;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>#641: Dragon's fireball typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class DragonFireballAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyDragonFireballAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, DragonFireballAnimations.Programs,
            DragonFireballFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        Console.WriteLine($"  Dragon fireball programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
