using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Fx.LookupRoomFxFieldsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CatalogInventories;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Checks compiled room FX records and Ceres consumers against the pinned cartridge.</summary>
internal static class RoomFxRecordDefinitionsVerification
{
    /// <summary>Checks the complete generated catalog and every source field against the ROM.</summary>
    internal static void VerifyRoomFxRecordDefinitions(string romPath)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "FX oracle revision");
        RoomFxRecordCatalogSource.RetailRoomFxCapture capture = RoomFxRecordCatalogSource.CaptureRetailRoomFx(bus);
        Suite(nameof(VerifyRoomFxFields), () => VerifyRoomFxFields(bus, capture.Records));
        Suite(nameof(VerifyRoomFxListSelection), () => VerifyRoomFxListSelection(bus));
        string generated = RoomFxRecordCatalogSource.RenderRoomFxDefinitions(capture).Replace("\r\n", "\n");
        string checkedIn = File.ReadAllText(RoomFxRecordCatalogSource.GeneratedPath).Replace("\r\n", "\n");
        AssertEqual(generated, checkedIn,
            "checked-in room-FX catalog is deterministic from pinned cartridge and all retail states");
        Suite(nameof(VerifyCompiledCeresRoomFxConsumers), () => VerifyCompiledCeresRoomFxConsumers(bus));
        Console.WriteLine($"Compiled room FX: {capture.Lists.Count} headers with {capture.Records.Count} typed records across {RoomStateDefinitions.All.Count} room states match every native field.");
    }

    /// <summary>Compares Ceres FX outputs with native reads and guards compiled consumers from rereading record bytes.</summary>
    private static void VerifyCompiledCeresRoomFxConsumers(SuperMetroidAddressSpace bus)
    {
        LoadStationEntry station = LoadStationDefinitions.Get(AreaId.Ceres, 0);
        CartridgeRoomHeader room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, (station.Room ?? throw new InvalidDataException("Ceres station has no room.")).Pointer);
        var guarded = NewRoomFxRecordReadGuard(bus);
        var nativeVram = new SnesVram();
        var compiledVram = new SnesVram();
        var nativeCgram = new SnesCgram();
        var compiledCgram = new SnesCgram();
        RoomFxPaletteBlendCatalog colors = RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(RoomFxPaletteBlendExtractor.Extract(bus)));
        RoomFxLayer3TilemapCatalog tilemaps = RoomFxLayer3TilemapCatalog.Load(
            new MemoryStream(RoomFxLayer3TilemapExtractor.Extract(bus)));
        var nativeFxSession = new RoomFxSession{ PaletteBlendColors = colors, Layer3Tilemaps = tilemaps };
        var nativeFx = new RoomLayer3FxState(nativeFxSession);
        var compiledFxSession = new RoomFxSession{ PaletteBlendColors = colors, Layer3Tilemaps = tilemaps };
        var compiledFx = new RoomLayer3FxState(compiledFxSession);
        nativeFx = RoomLayer3FxState.ForRoom(nativeFxSession, nativeCgram, room.State.Fx,
            station.Door, randomNumber: 0, room.Definition);
        compiledFx = RoomLayer3FxState.ForRoom(compiledFxSession, compiledCgram, room.State.Fx,
            station.Door, randomNumber: 0, room.Definition);
        AssertEqual(nativeFx.Type, compiledFx.Type, "Ceres compiled FX type");
        AssertEqual(nativeFx.BaseY.Whole, compiledFx.BaseY.Whole, "Ceres compiled FX base Y");
        AssertEqual(nativeFx.TargetSurface.Y, compiledFx.TargetSurface.Y, "Ceres compiled FX target Y");
        AssertEqual(nativeFx.YVelocity.RawValue, compiledFx.YVelocity.RawValue,
            "Ceres compiled FX velocity");
        AssertEqual(nativeFx.Timer.RawValue, compiledFx.Timer.RawValue, "Ceres compiled FX timer");
        AssertEqual(nativeFx.LiquidOptions, compiledFx.LiquidOptions,
            "Ceres compiled FX liquid options");
        AssertTrue(nativeVram.ImageEquals(compiledVram),
            "Ceres compiled FX retains VRAM output");
        AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(compiledCgram)),
            "Ceres compiled FX retains CGRAM output");

        var nativePaletteSession = new PaletteFxSessionState();
        var nativePalette = new RoomPaletteFxSystem(nativePaletteSession);
        var compiledPaletteSession = new PaletteFxSessionState();
        var compiledPalette = new RoomPaletteFxSystem(compiledPaletteSession);
        nativePalette = RoomPaletteFxSystem.ForRoom(nativePaletteSession, room.State.Fx, station.Door,
            room.AreaIndex, equippedItems: 0, areaMiniBossDefeated: false);
        compiledPalette = RoomPaletteFxSystem.ForRoom(compiledPaletteSession, room.State.Fx, station.Door,
            room.AreaIndex, equippedItems: 0, areaMiniBossDefeated: false);
        AssertEqual(nativePalette.ActiveCount, compiledPalette.ActiveCount,
            "Ceres compiled palette-FX object count");

        var nativeSand = new RoomSandAnimatedTilesState(room.State.Fx, station.Door, room.AreaIndex);
        var compiledSand = new RoomSandAnimatedTilesState(room.State.Fx, station.Door,
            room.AreaIndex);
        AssertEqual(nativeSand.Count, compiledSand.Count,
            "Ceres compiled sand animation count");

        var nativeTreadmills = new RoomTreadmillAnimatedTilesState(room.State.Fx, station.Door, room.AreaIndex);
        var compiledTreadmills = new RoomTreadmillAnimatedTilesState(room.State.Fx, station.Door,
            room.AreaIndex);
        AssertEqual(nativeTreadmills.Count, compiledTreadmills.Count,
            "Ceres compiled treadmill animation count");
    }

    /// <summary>The native record bytes compiled room FX consumers must no longer request.</summary>
    private static readonly HashSet<int> ForbiddenRoomFxRecordBytes = BuildForbiddenRoomFxRecordBytes();

    /// <summary>Wraps the source bus so any room FX record reread fails with the offending address.</summary>
    private static CartridgeReadGuard NewRoomFxRecordReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(ForbiddenRoomFxRecordBytes.Contains,
            address => $"Compiled room-FX consumer reread source record byte ${address:X6}.");

    /// <summary>Collects each exported FX record's native byte range, including short terminator records.</summary>
    private static HashSet<int> BuildForbiddenRoomFxRecordBytes()
    {
        var result = new HashSet<int>();
        foreach (ushort pointer in RoomFxDefinitions.RecordPointers)
        {
            int length = RoomFxDefinitions.RecordAt(pointer).Terminates
                ? sizeof(ushort) : RoomFxToolingRomData.Record.ByteCount;
            for (int offset = 0; offset < length; offset++)
                result.Add(RoomFxRomDataBanksTooling.RoomDefinitions | (pointer + offset));
        }
        return result;
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
