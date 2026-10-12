using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static Program;
using static RepositoryInputsVerification;
using System.Text;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using static LookupRoomFxFieldsVerification;
using static SuiteTimingVerification;

/// <summary>Represents room fx record definitions verification.</summary>
internal static class RoomFxRecordDefinitionsVerification
{
    /// <summary>Checks the complete generated catalog and every source field against the ROM.</summary>
    internal static void VerifyRoomFxRecordDefinitions(string romPath)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "FX oracle revision");
        SortedDictionary<ushort, RoomFxRecordDefinition> records = RoomFxRecordCatalogSource.CaptureRetailRoomFxRecords(bus);
        Suite(nameof(VerifyRoomFxFields), () => VerifyRoomFxFields(bus, records));
        Suite(nameof(VerifyRoomFxListSelection), () => VerifyRoomFxListSelection(bus));
        string generated = RoomFxRecordCatalogSource.RenderRoomFxDefinitions(records.Values).Replace("\r\n", "\n");
        string checkedIn = File.ReadAllText(RoomFxRecordCatalogSource.GeneratedPath).Replace("\r\n", "\n");
        AssertEqual(generated, checkedIn,
            "checked-in room-FX catalog is deterministic from pinned cartridge and all retail states");
        Suite(nameof(VerifyCompiledCeresRoomFxConsumers), () => VerifyCompiledCeresRoomFxConsumers(bus));
        Console.WriteLine($"Compiled room FX: {records.Count} typed records across {RoomStateDefinitions.All.Count()} room states match every native field.");
    }

    /// <summary>Verifies compiled ceres room fx consumers.</summary>
    private static void VerifyCompiledCeresRoomFxConsumers(SuperMetroidAddressSpace bus)
    {
        LoadStationEntry station = LoadStationDefinitions.Get(AreaId.Ceres, 0);
        CartridgeRoomHeader room = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(bus, station.RoomPointer);
        var guarded = NewRoomFxRecordReadGuard(bus);
        var nativeVram = new SnesVram();
        var compiledVram = new SnesVram();
        var nativeCgram = new SnesCgram();
        var compiledCgram = new SnesCgram();
        RoomFxPaletteBlendCatalog colors = RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(RoomFxPaletteBlendExtractor.Extract(bus)));
        RoomFxLayer3TilemapCatalog tilemaps = RoomFxLayer3TilemapCatalog.Load(
            new MemoryStream(RoomFxLayer3TilemapExtractor.Extract(bus)));
        var nativeFx = new RoomLayer3FxState { PaletteBlendColors = colors, Layer3Tilemaps = tilemaps };
        var compiledFx = new RoomLayer3FxState { PaletteBlendColors = colors, Layer3Tilemaps = tilemaps };
        nativeFx.Load(bus, nativeVram, nativeCgram, room.State.FxPointer,
            station.DoorPointer, randomNumber: 0, room.Pointer);
        compiledFx.Load(guarded, compiledVram, compiledCgram, room.State.FxPointer,
            station.DoorPointer, randomNumber: 0, room.Pointer);
        AssertEqual(nativeFx.Type, compiledFx.Type, "Ceres compiled FX type");
        AssertEqual(nativeFx.BaseYPosition, compiledFx.BaseYPosition, "Ceres compiled FX base Y");
        AssertEqual(nativeFx.TargetYPosition, compiledFx.TargetYPosition, "Ceres compiled FX target Y");
        AssertEqual(nativeFx.PackedYVelocity, compiledFx.PackedYVelocity,
            "Ceres compiled FX velocity");
        AssertEqual(nativeFx.Timer, compiledFx.Timer, "Ceres compiled FX timer");
        AssertEqual(nativeFx.LiquidOptions, compiledFx.LiquidOptions,
            "Ceres compiled FX liquid options");
        AssertTrue(nativeVram.ImageEquals(compiledVram),
            "Ceres compiled FX retains VRAM output");
        AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(compiledCgram)),
            "Ceres compiled FX retains CGRAM output");

        var nativePalette = new RoomPaletteFxSystem();
        var compiledPalette = new RoomPaletteFxSystem();
        nativePalette.LoadRoom(bus, room.State.FxPointer, station.DoorPointer,
            room.AreaIndex, equippedItems: 0, areaMiniBossDefeated: false);
        compiledPalette.LoadRoom(guarded, room.State.FxPointer, station.DoorPointer,
            room.AreaIndex, equippedItems: 0, areaMiniBossDefeated: false);
        AssertEqual(nativePalette.ActiveCount, compiledPalette.ActiveCount,
            "Ceres compiled palette-FX object count");

        var nativeSand = new RoomSandAnimatedTilesState();
        var compiledSand = new RoomSandAnimatedTilesState();
        nativeSand.LoadRoom(bus, room.State.FxPointer, station.DoorPointer, room.AreaIndex);
        compiledSand.LoadRoom(guarded, room.State.FxPointer, station.DoorPointer,
            room.AreaIndex);
        AssertEqual(nativeSand.Count, compiledSand.Count,
            "Ceres compiled sand animation count");

        var nativeTreadmills = new RoomTreadmillAnimatedTilesState();
        var compiledTreadmills = new RoomTreadmillAnimatedTilesState();
        nativeTreadmills.LoadRoom(bus, room.State.FxPointer, station.DoorPointer, room.AreaIndex);
        compiledTreadmills.LoadRoom(guarded, room.State.FxPointer, station.DoorPointer,
            room.AreaIndex);
        AssertEqual(nativeTreadmills.Count, compiledTreadmills.Count,
            "Ceres compiled treadmill animation count");
    }

    /// <summary>Stores forbidden room fx record bytes.</summary>
    private static readonly HashSet<int> ForbiddenRoomFxRecordBytes = BuildForbiddenRoomFxRecordBytes();

    /// <summary>New room fx record read guard.</summary>
    private static CartridgeReadGuard NewRoomFxRecordReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(ForbiddenRoomFxRecordBytes.Contains,
            address => $"Compiled room-FX consumer reread source record byte ${address:X6}.");

    /// <summary>Builds forbidden room fx record bytes.</summary>
    private static HashSet<int> BuildForbiddenRoomFxRecordBytes()
    {
        var result = new HashSet<int>();
        foreach (RoomFxRecordDefinition record in RoomFxRecordDefinitions.All)
        {
            int length = record.DoorPointer == RoomFxRomData.Record.TerminatorDoorPointer
                ? sizeof(ushort) : RoomFxRomData.Record.ByteCount;
            for (int offset = 0; offset < length; offset++)
                result.Add(RoomFxRomDataBanksTooling.RoomDefinitions | (record.Pointer + offset));
        }
        return result;
    }

    /// <summary>Room fx record definitions command.</summary>
    [VerificationCommand("--room-fx-record-definitions")]
    internal static int RoomFxRecordDefinitionsCommand()
    {
        string roomFxDefinitionsRom = RepositoryRomPath;
        Suite(nameof(VerifyRoomFxRecordDefinitions), () => VerifyRoomFxRecordDefinitions(roomFxDefinitionsRom));
        return 0;
    }
}
