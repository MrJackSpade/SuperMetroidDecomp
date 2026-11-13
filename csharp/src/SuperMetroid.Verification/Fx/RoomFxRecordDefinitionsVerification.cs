using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Fx.LookupRoomFxFieldsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CatalogInventories;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Checks compiled room FX records and Ceres consumers against the pinned cartridge.</summary>
internal static class RoomFxRecordDefinitionsVerification
{
    /// <summary>
    /// The pointer of every terminator word that ends a header with no default record: the header itself when it lists
    /// nothing, otherwise the record after its last door record.
    /// </summary>
    private static SortedSet<ushort> TerminatorsOf(RoomFxRecordCatalogSource.RetailRoomFxCapture capture) =>
    [
        .. capture.Lists
            .Where(list => list.Value.Length == 0 || capture.Doors.ContainsKey(list.Value[^1]))
            .Select(list => list.Value.Length == 0
                ? list.Key
                : unchecked((ushort)(list.Value[^1] + RoomFxToolingRomData.Record.ByteCount))),
    ];

    /// <summary>Checks the complete generated catalog and every source field against the ROM.</summary>
    internal static void VerifyRoomFxRecordDefinitions(string romPath)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "FX oracle revision");
        RoomFxRecordCatalogSource.RetailRoomFxCapture capture = RoomFxRecordCatalogSource.CaptureRetailRoomFx(bus);
        Suite(nameof(VerifyRoomFxFields), () => VerifyRoomFxFields(bus, capture.Records, TerminatorsOf(capture)));
        Suite(nameof(VerifyRoomFxListSelection), () => VerifyRoomFxListSelection(bus));
        string generated = RoomFxRecordCatalogSource.RenderRoomFxDefinitions(capture).Replace("\r\n", "\n");
        string checkedIn = File.ReadAllText(RoomFxRecordCatalogSource.GeneratedPath).Replace("\r\n", "\n");
        AssertEqual(generated, checkedIn,
            "checked-in room-FX catalog is deterministic from pinned cartridge and all retail states");
        Suite(nameof(VerifyCompiledCeresRoomFxConsumers), () => VerifyCompiledCeresRoomFxConsumers(bus));
        Console.WriteLine($"Compiled room FX: {capture.Lists.Count} headers with {capture.Records.Count} typed records across {RoomStateDefinitions.All.Count} room states match every native field.");
    }

    /// <summary>Compares Ceres FX outputs with native reads and guards compiled consumers from rereading record bytes.</summary>
    private static void VerifyCompiledCeresRoomFxConsumers(CartridgeImportAddressSpace bus)
    {
        LoadStationPlacement station = LoadStationDefinitions.CeresElevator.Placement;
        SelectedRoom room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, station.Room.Pointer);
        var nativeVram = new SnesVram();
        var compiledVram = new SnesVram();
        var nativeFxSession = new RoomFxSession();
        RoomFxOverlayState nativeFx;
        var compiledFxSession = new RoomFxSession();
        RoomFxOverlayState compiledFx;
        nativeFx = RoomFxOverlayState.ForRoom(nativeFxSession, room.State.Fx,
            station.Door, randomNumber: 0, room.Definition);
        compiledFx = RoomFxOverlayState.ForRoom(compiledFxSession, room.State.Fx,
            station.Door, randomNumber: 0, room.Definition);
        AssertEqual(nativeFx.Type, compiledFx.Type, "Ceres compiled FX type");
        AssertEqual(nativeFx.BaseY?.Whole, compiledFx.BaseY?.Whole, "Ceres compiled FX base Y");
        AssertEqual(nativeFx.TargetSurface, compiledFx.TargetSurface, "Ceres compiled FX target Y");
        AssertEqual(nativeFx.YVelocity.RawValue, compiledFx.YVelocity.RawValue,
            "Ceres compiled FX velocity");
        AssertEqual(nativeFx.Timer.RawValue, compiledFx.Timer.RawValue, "Ceres compiled FX timer");
        AssertEqual(nativeFx.Liquid, compiledFx.Liquid,
            "Ceres compiled FX liquid");
        AssertTrue(nativeVram.ImageEquals(compiledVram),
            "Ceres compiled FX retains VRAM output");
        // The blend's colours are the layer-3 pictures' shade of it (#1512); the room names the same blend.
        AssertEqual(nativeFx.PaletteBlend, compiledFx.PaletteBlend, "Ceres compiled FX palette blend");

        var nativePaletteSession = new PaletteFxSessionState();
        RoomPaletteFxSystem nativePalette;
        var compiledPaletteSession = new PaletteFxSessionState();
        RoomPaletteFxSystem compiledPalette;
        nativePalette = RoomPaletteFxSystem.ForRoom(nativePaletteSession, room.State.Fx, station.Door,
            equippedItems: 0, areaMiniBossDefeated: false);
        compiledPalette = RoomPaletteFxSystem.ForRoom(compiledPaletteSession, room.State.Fx, station.Door,
            equippedItems: 0, areaMiniBossDefeated: false);
        AssertEqual(nativePalette.ActiveCount, compiledPalette.ActiveCount,
            "Ceres compiled palette-FX object count");

        var nativeSand = new RoomSandAnimatedTilesState(room.State.Fx, station.Door);
        var compiledSand = new RoomSandAnimatedTilesState(room.State.Fx, station.Door);
        AssertEqual(nativeSand.Count, compiledSand.Count,
            "Ceres compiled sand animation count");

        var nativeTreadmills = new RoomTreadmillAnimatedTilesState(room.State.Fx, station.Door);
        var compiledTreadmills = new RoomTreadmillAnimatedTilesState(room.State.Fx, station.Door);
        AssertEqual(nativeTreadmills.Count, compiledTreadmills.Count,
            "Ceres compiled treadmill animation count");
    }

    /// <summary>Runs the room FX catalog comparison through the verification command dispatcher.</summary>
    [VerificationCommand("--room-fx-record-definitions")]
    internal static int RoomFxRecordDefinitionsCommand()
    {
        string roomFxDefinitionsRom = RepositoryRomPath;
        Suite(nameof(VerifyRoomFxRecordDefinitions), () => VerifyRoomFxRecordDefinitions(roomFxDefinitionsRom));
        return 0;
    }
}
