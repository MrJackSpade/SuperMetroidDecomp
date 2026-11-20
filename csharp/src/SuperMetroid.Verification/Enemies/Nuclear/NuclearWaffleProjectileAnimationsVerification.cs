using static SuperMetroid.TestSupport.MovieReplay.NuclearWaffleProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Nuclear;

/// <summary>#641: the Nuclear Waffle's body typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class NuclearWaffleProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyNuclearWaffleProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Nuclear Waffle body programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
