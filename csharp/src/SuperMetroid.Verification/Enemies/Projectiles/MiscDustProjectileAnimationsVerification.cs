using static SuperMetroid.Verification.Enemies.Projectiles.MiscDustProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>#641: the misc dust's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class MiscDustProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMiscDustProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, MiscDustProjectileAnimations.Programs,
            MiscDustProjectileFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        Console.WriteLine($"  misc dust programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
