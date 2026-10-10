using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    /// <summary>
    /// Checks compiled room-state settings against the retail cartridge and confirms production
    /// Ceres header construction selects its state without a cartridge-reader capability.
    /// </summary>
    private static void VerifyCompiledRoomStateDefinitions()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "Room-state oracle revision");
        Suite(nameof(VerifyRoomStateSettings), () => VerifyRoomStateSettings(bus));

        ushort ceresPointer = LoadStationDefinitions.Get(
            SuperMetroid.Core.Game.AreaId.Ceres, 0).RoomPointer;
        ushort ceresStatePointer = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(bus, ceresPointer).State.Pointer;
        CartridgeRoomHeader compiledRoom = CartridgeRoomHeader.LoadUsingCompiledSelection(ceresPointer);
        AssertEqual(RoomStateDefinitions.Get(ceresStatePointer), compiledRoom.State,
            "production Ceres header construction uses its compiled room-state settings without a cartridge capability");
        Console.WriteLine(
            "Room state settings: all 323 identities and 15 fields match cartridge data; " +
            "production Ceres state selection has no cartridge-reader capability.");
    }
}
