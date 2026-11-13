using static SuperMetroid.TestSupport.MovieReplay.PowampSpikeNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>#641: the Powamp spike's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class PowampSpikeAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyPowampSpikeAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Powamp spike programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
