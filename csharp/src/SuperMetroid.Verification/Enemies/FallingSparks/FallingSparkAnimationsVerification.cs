using static SuperMetroid.TestSupport.MovieReplay.FallingSparkNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.FallingSparks;

/// <summary>#641: the falling spark's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class FallingSparkAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyFallingSparkAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  falling spark programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
