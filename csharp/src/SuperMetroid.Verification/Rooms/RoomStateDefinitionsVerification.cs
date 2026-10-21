using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.LookupRoomStateSettingsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Compares compiled room-state payloads and cartridge-free production selection with the pinned retail ROM.</summary>
internal static class RoomStateDefinitionsVerification
{
    /// <summary>Checks all 323 room-state identities and fifteen settings fields, then confirms Ceres loads the compiled selection without a cartridge reader.</summary>
    internal static void VerifyCompiledRoomStateDefinitions()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "Room-state oracle revision");
        Suite(nameof(VerifyRoomStateSettings), () => VerifyRoomStateSettings(bus));

        RoomHeaderDefinition ceres = LoadStationDefinitions.Get(SuperMetroid.Core.Progression.AreaId.Ceres, 0).Room
            ?? throw new InvalidDataException("Ceres station has no room.");
        ushort ceresStatePointer = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, ceres.Pointer).State.Pointer;
        CartridgeRoomHeader compiledRoom = CartridgeRoomHeader.LoadUsingCompiledSelection(ceres);
        AssertEqual(RoomStateDefinitions.Get(ceresStatePointer), compiledRoom.State,
            "production Ceres header construction uses its compiled room-state settings without a cartridge capability");
        Console.WriteLine(
            "Room state settings: all 323 identities and 15 fields match cartridge data; " +
            "production Ceres state selection has no cartridge-reader capability.");
    }

    /// <summary>Runs the room-state settings comparison through its original command alias.</summary>
    [VerificationCommand("--lookup-room-state-settings")]
    internal static int LookupRoomStateSettingsCommand()
    {
        Suite(nameof(VerifyCompiledRoomStateDefinitions), () => VerifyCompiledRoomStateDefinitions());
        return 0;
    }

    /// <summary>Runs the same compiled room-state check under the payload command alias.</summary>
    [VerificationCommand("--room-state-payloads")]
    internal static int RoomStatePayloadsCommand()
    {
        Suite(nameof(VerifyCompiledRoomStateDefinitions), () => VerifyCompiledRoomStateDefinitions());
        return 0;
    }
}
