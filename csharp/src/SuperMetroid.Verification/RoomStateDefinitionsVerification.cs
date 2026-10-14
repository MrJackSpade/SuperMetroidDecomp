using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;
using static LookupRoomStateSettingsVerification;
using static SuiteTimingVerification;

/// <summary>Compares compiled room-state payloads and cartridge-free production selection with the pinned retail ROM.</summary>
internal static class RoomStateDefinitionsVerification
{
    /// <summary>Checks all 323 room-state identities and fifteen settings fields, then confirms Ceres loads the compiled selection without a cartridge reader.</summary>
    internal static void VerifyCompiledRoomStateDefinitions()
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
