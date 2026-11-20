using static SuperMetroid.TestSupport.MovieReplay.KiHunterAcidSpitNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>#641: the Ki-Hunter's acid spit typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class KiHunterAcidSpitAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyKiHunterAcidSpitAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Ki-Hunter acid spit programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
