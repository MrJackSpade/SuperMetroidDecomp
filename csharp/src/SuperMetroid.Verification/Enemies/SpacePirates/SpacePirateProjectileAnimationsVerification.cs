using static SuperMetroid.TestSupport.MovieReplay.SpacePirateProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>#641: the laser and claw typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class SpacePirateProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySpacePirateProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = LaserFamily.CompareAll(rom) + ClawFamily.CompareAll(rom);
        Console.WriteLine($"  Pirate laser and claw programs: {LaserEntries.Length + ClawEntries.Length} programs ({steps} steps) match bank $86.");
    }
}
