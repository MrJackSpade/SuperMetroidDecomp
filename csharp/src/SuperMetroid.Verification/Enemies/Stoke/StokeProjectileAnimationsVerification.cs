using static SuperMetroid.TestSupport.MovieReplay.StokeProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>#641: the Stoke's projectile typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class StokeProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyStokeProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Stoke projectile programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
