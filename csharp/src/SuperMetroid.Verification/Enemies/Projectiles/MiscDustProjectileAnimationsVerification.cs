using static SuperMetroid.TestSupport.MovieReplay.MiscDustProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>#641: the misc dust's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class MiscDustProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMiscDustProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  misc dust programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
