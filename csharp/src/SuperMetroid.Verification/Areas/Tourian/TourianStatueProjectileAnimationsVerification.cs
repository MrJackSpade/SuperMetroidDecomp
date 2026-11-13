using static SuperMetroid.TestSupport.MovieReplay.TourianStatueProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>#641: the Tourian statue projectile typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class TourianStatueProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyTourianStatueProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Tourian statue projectile programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
