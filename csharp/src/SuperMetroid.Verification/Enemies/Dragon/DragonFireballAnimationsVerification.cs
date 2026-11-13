using static SuperMetroid.TestSupport.MovieReplay.DragonFireballNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>#641: Dragon's fireball typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class DragonFireballAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyDragonFireballAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Dragon fireball programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
