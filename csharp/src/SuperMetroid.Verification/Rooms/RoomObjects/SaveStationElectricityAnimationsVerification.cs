using static SuperMetroid.TestSupport.MovieReplay.SaveStationElectricityNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>#1296: the save-station electricity's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class SaveStationElectricityAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySaveStationElectricityAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  save-station electricity programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
