using static SuperMetroid.TestSupport.MovieReplay.YappingMawBodyProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>#641: the Yapping Maw's body links typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class YappingMawBodyProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyYappingMawBodyProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Yapping Maw body link programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
