using static SuperMetroid.TestSupport.MovieReplay.EyeDoorProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>#641: the eye door's projectile typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class EyeDoorProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEyeDoorProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  eye door projectile programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
