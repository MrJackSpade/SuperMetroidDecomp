using static SuperMetroid.TestSupport.MovieReplay.CacatacProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>#641: the Cacatac's spike typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class CacatacProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyCacatacProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Cacatac spike programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
