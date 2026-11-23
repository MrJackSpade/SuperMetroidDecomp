using static SuperMetroid.TestSupport.MovieReplay.DustExplosionProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>#641, #1600: the dust explosion's typed animations, each segment matched beat by beat to its native bank-$86 list.</summary>
internal static class DustExplosionProjectileAnimationsVerification
{
    /// <summary>Every segment matches its native list and every animation the lists' control flow.</summary>
    internal static void VerifyDustExplosionProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = Family.CompareAll(rom);
        Console.WriteLine($"  misc dust animations: {Entries.Length} segments ({beats} beats) match bank $86.");
    }
}
