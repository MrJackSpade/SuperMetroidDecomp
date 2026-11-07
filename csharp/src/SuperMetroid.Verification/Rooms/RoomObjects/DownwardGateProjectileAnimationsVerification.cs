using static SuperMetroid.TestSupport.MovieReplay.DownwardGateProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>#641: the downward shot gate's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class DownwardGateProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyDownwardGateProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  downward gate programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
