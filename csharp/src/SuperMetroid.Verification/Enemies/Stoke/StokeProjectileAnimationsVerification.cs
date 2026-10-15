using static SuperMetroid.Verification.Enemies.Stoke.StokeProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Stoke.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>#641: the Stoke's projectile typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class StokeProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyStokeProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, StokeProjectileAnimations.Programs,
            StokeProjectileFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        Console.WriteLine($"  Stoke projectile programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
