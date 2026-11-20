using static SuperMetroid.TestSupport.MovieReplay.MotherBrainTopTubeNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>#641: the ceiling-tube typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class MotherBrainTopTubeAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMotherBrainTopTubeAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Mother Brain ceiling-tube programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
