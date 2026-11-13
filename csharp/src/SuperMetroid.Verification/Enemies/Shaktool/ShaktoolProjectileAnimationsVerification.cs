using static SuperMetroid.TestSupport.MovieReplay.ShaktoolProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>#641: Shaktool's attack circles typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class ShaktoolProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyShaktoolProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Shaktool attack circle programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
