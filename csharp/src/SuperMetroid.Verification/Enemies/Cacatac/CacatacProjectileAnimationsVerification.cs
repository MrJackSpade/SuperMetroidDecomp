using static SuperMetroid.Verification.Enemies.Cacatac.CacatacProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>#641: the Cacatac's spike typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class CacatacProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyCacatacProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, CacatacProjectileAnimations.Programs,
            CacatacProjectileFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        Console.WriteLine($"  Cacatac spike programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
