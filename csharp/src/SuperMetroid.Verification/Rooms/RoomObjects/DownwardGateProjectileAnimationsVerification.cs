using static SuperMetroid.TestSupport.MovieReplay.DownwardGateProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>#641, #1600: the downward shot gate's typed animations, each segment matched beat by beat to its native bank-$86 list.</summary>
internal static class DownwardGateProjectileAnimationsVerification
{
    /// <summary>Every segment matches its native list and every animation the lists' control flow.</summary>
    internal static void VerifyDownwardGateProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  downward gate animations: {Entries.Length} segments ({beats} beats) match bank $86.");
    }
}
