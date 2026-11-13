using static SuperMetroid.TestSupport.MovieReplay.SkreeMetareeParticleNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>#641: the Skree's and Metaree's death particles typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class SkreeMetareeParticleAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySkreeMetareeParticleAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Skree/Metaree particle programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
