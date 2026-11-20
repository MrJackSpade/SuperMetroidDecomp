using static SuperMetroid.TestSupport.MovieReplay.EnemyDeathProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Deaths;

/// <summary>#641: an enemy's death explosion and drop typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class EnemyDeathProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEnemyDeathProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  enemy death and drop programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
