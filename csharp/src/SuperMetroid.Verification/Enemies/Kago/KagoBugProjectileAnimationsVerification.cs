using static SuperMetroid.TestSupport.MovieReplay.KagoBugProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Kago;

/// <summary>#641: the Kago's bugs typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class KagoBugProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyKagoBugProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Kago bug programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
