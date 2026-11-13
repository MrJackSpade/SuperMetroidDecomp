using static SuperMetroid.TestSupport.MovieReplay.EyeDoorSweatNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>#1296: the eye-door sweat's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class EyeDoorSweatAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEyeDoorSweatAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  eye-door sweat programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
