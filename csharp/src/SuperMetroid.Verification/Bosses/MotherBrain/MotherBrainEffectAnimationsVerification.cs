using static SuperMetroid.TestSupport.MovieReplay.MotherBrainEffectNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>#641: Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class MotherBrainEffectAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMotherBrainEffectAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Mother Brain effect programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
