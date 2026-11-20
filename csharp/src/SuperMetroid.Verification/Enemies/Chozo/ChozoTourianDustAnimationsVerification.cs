using static SuperMetroid.TestSupport.MovieReplay.ChozoTourianDustNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>#641: the Wrecked Ship Chozo's and Tourian statue's dust typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class ChozoTourianDustAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyChozoTourianDustAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Chozo/Tourian dust programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
