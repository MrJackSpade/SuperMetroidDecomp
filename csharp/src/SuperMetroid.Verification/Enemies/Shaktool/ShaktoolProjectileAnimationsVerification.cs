using static SuperMetroid.Verification.Enemies.Shaktool.ShaktoolProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Shaktool.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>#641: Shaktool's attack circles typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class ShaktoolProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyShaktoolProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, ShaktoolProjectileAnimations.Programs,
            ShaktoolProjectileFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        Console.WriteLine($"  Shaktool attack circle programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
