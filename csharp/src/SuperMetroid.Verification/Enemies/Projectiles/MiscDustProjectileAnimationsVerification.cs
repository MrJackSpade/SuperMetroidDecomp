using static SuperMetroid.TestSupport.MovieReplay.MiscDustProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>#641, #1600: the misc dust's typed animations, each segment matched beat by beat to its native bank-$86 list.</summary>
internal static class MiscDustProjectileAnimationsVerification
{
    /// <summary>Every segment matches its native list and every animation the lists' control flow.</summary>
    internal static void VerifyMiscDustProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  misc dust animations: {Entries.Length} segments ({beats} beats) match bank $86.");
    }
}
