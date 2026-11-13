using static SuperMetroid.TestSupport.MovieReplay.SpacePirateProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>#641: the laser and claw typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class SpacePirateProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySpacePirateProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Pirate laser and claw programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
