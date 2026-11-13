using static SuperMetroid.TestSupport.MovieReplay.MagdolliteLavaNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>#641: the Magdollite's lava typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class MagdolliteLavaAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMagdolliteLavaAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Magdollite lava programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
