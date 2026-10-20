using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.InstalledFileSelectMapVerification;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks file-select map area order and save-station anchors against native metadata and menu behavior.</summary>
internal static class MapLoadAnchorsVerification
{
    /// <summary>Verification-only coordinates preserved from 5b23a2aa, independent of the replacement projection formula.</summary>
    private static readonly (AreaId Area, int Station, ushort X, ushort Y)[] originalMapLoadAnchors =
    [
        (AreaId.Crateria, 0, 216, 40),
        (AreaId.Crateria, 1, 144, 56),
        (AreaId.Crateria, 8, 416, 88),
        (AreaId.Crateria, 9, 272, 64),
        (AreaId.Crateria, 10, 184, 144),
        (AreaId.Crateria, 11, 48, 72),
        (AreaId.Crateria, 12, 136, 80),
        (AreaId.Brinstar, 0, 120, 40),
        (AreaId.Brinstar, 1, 64, 48),
        (AreaId.Brinstar, 2, 40, 96),
        (AreaId.Brinstar, 3, 392, 152),
        (AreaId.Brinstar, 4, 304, 72),
        (AreaId.Brinstar, 8, 72, 24),
        (AreaId.Brinstar, 9, 208, 88),
        (AreaId.Brinstar, 10, 296, 56),
        (AreaId.Brinstar, 11, 328, 152),
        (AreaId.Norfair, 0, 96, 96),
        (AreaId.Norfair, 1, 168, 32),
        (AreaId.Norfair, 2, 88, 48),
        (AreaId.Norfair, 3, 128, 72),
        (AreaId.Norfair, 4, 160, 88),
        (AreaId.Norfair, 5, 288, 104),
        (AreaId.Norfair, 8, 80, 24),
        (AreaId.Norfair, 9, 168, 88),
        (AreaId.Norfair, 10, 168, 112),
        (AreaId.WreckedShip, 0, 136, 120),
        (AreaId.Maridia, 0, 96, 160),
        (AreaId.Maridia, 1, 280, 40),
        (AreaId.Maridia, 2, 152, 96),
        (AreaId.Maridia, 3, 328, 56),
        (AreaId.Maridia, 8, 272, 24),
        (AreaId.Tourian, 0, 128, 144),
        (AreaId.Tourian, 1, 168, 104),
        (AreaId.Tourian, 8, 160, 96),
    ];

    /// <summary>Compares six file-select display areas with the native area-order words and rejects unsupported menu indexes.</summary>
    internal static void VerifyFileSelectMapAreaCases(ISnesAddressSpace bus)
    {
        for (int displayIndex = 0; displayIndex < 6; displayIndex++)
            AssertEqual(ReadVerificationWord(bus, 0x81aaa0 + displayIndex * 2),
                (ushort)FileSelectMapAreaOrder.Get(displayIndex), "native menu-to-game area identity");
        foreach (int invalid in new[] { int.MinValue, -1, 6, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapAreaOrder.Get(invalid),
                "unsupported menu area identity is not clamped or narrowed");
    }

    /// <summary>Checks all 34 horizontal save-station anchors against preserved coordinates and native records.</summary>
    internal static void VerifyMapLoadAnchorX(ISnesAddressSpace bus) => VerifyMapLoadAnchorField(bus, vertical: false);
    /// <summary>Checks all 34 vertical save-station anchors against preserved coordinates and native records.</summary>
    internal static void VerifyMapLoadAnchorY(ISnesAddressSpace bus) => VerifyMapLoadAnchorField(bus, vertical: true);

    /// <summary>Checks one coordinate axis and rejects unused stations and invalid area or station indexes.</summary>
    private static void VerifyMapLoadAnchorField(ISnesAddressSpace bus, bool vertical)
    {
        int anchors = 0;
        for (int area = 0; area < 6; area++)
        {
            var typedArea = (AreaId)area;
            for (int station = 0; station < 16; station++)
            {
                if (!MapSaveMarkerDefinitions.Indices(typedArea).Contains(station))
                {
                    AssertThrows<InvalidDataException>(() => FileSelectMapLoadAnchors.Get(typedArea, station),
                        "unused map-load station remains rejected");
                    continue;
                }
                var expected = originalMapLoadAnchors.Single(entry => entry.Area == typedArea && entry.Station == station);
                Position16<MapPixels> native = ReadNativeMapLoadAnchor(bus, typedArea, station);
                AssertEqual(vertical ? expected.Y : expected.X, vertical ? native.Y : native.X,
                    "preserved table agrees with original load/room records");
                Position16<MapPixels> actual = FileSelectMapLoadAnchors.Get(typedArea, station);
                AssertEqual(vertical ? expected.Y : expected.X, vertical ? actual.Y : actual.X,
                    $"original projected map anchor {typedArea}/{station} vertical={vertical}");
                anchors++;
            }
            foreach (int invalid in new[] { int.MinValue, -1, 16, 256, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapLoadAnchors.Get(typedArea, invalid),
                    "station bounds checked before narrowing");
        }
        AssertEqual(34, anchors, "complete original map-load anchor field");
        foreach (AreaId invalid in new[] { AreaId.Ceres, (AreaId)7, (AreaId)255 })
            AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapLoadAnchors.Get(invalid, 0),
                "unsupported map-load area remains rejected");
    }

    /// <summary>Compares native and compiled map scrolling, saved-station entry, and area rendering with cartridge access forbidden on the compiled path.</summary>
    internal static void VerifyMapLoadAnchors(ISnesAddressSpace bus, AreaMapPresentationCatalog catalog)
    {
        // One store for every saved-station fixture, as one SRAM held them.
        var stationSaves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        Suite(nameof(VerifyMapLoadAnchorX), () => VerifyMapLoadAnchorX(bus));
        Suite(nameof(VerifyMapLoadAnchorY), () => VerifyMapLoadAnchorY(bus));
        Suite(nameof(VerifyFileSelectMapAreaCases), () => VerifyFileSelectMapAreaCases(bus));
        var guard = NewForbiddenMapBus();
        int anchors = 0;
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            var typedArea = (AreaId)area;
            for (int stationIndex = 0; stationIndex < MapSaveMarkerDefinitions.SlotsPerArea; stationIndex++)
            {
                if (!MapSaveMarkerDefinitions.Indices(typedArea).Contains(stationIndex))
                {
                    AssertThrows<InvalidDataException>(() => FileSelectMapLoadAnchors.Get(typedArea, stationIndex), "unused load-map index rejected");
                    continue;
                }
                var expected = ReadNativeMapLoadAnchor(bus, typedArea, stationIndex);
                var actual = FileSelectMapLoadAnchors.Get(typedArea, stationIndex);
                anchors++;
                for (int visibility = 0; visibility < 4; visibility++)
                {
                    var system = new Bank80SystemState();
                    if (visibility == 1)
                    {
                        system.MarkExploredMapTile(typedArea, new(0, 0));
                        system.MarkExploredMapTile(typedArea, new(63, 31));
                    }
                    if (visibility == 2) system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
                    if (visibility == 3)
                    {
                        var stations = new byte[Bank80SystemState.MapStationByteCount]; stations[area] = 1;
                        system.LoadMapStationBytes(stations);
                    }
                    var native = new FileSelectMapScroll(SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, typedArea), system, expected);
                    var compiled = new FileSelectMapScroll(catalog.Get(typedArea), system, actual);
                    AssertEqual(ScrollState(native), ScrollState(compiled), "compiled anchors retain bounds and initial clipping for empty/sparse/full/downloaded maps");
                    foreach (SnesButton input in MapScrollControls.Buttons)
                        for (int tick = 0; tick < 40; tick++)
                        {
                            AssertEqual(native.Step(input), compiled.Step(input), "anchor migration retains scroll sound timing");
                            AssertEqual(ScrollState(native), ScrollState(compiled), "anchor migration retains actual scroll trajectory");
                        }
                }
                VerifyStationMenu(typedArea, stationIndex);
            }
        }
        AssertEqual(34, anchors, "all native valid saved-map anchors checked");
        AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapLoadAnchors.Get(AreaId.Ceres, 0), "Ceres has no file-select map anchor");
        AssertThrows<ArgumentOutOfRangeException>(() => FileSelectMapLoadAnchors.Get(AreaId.Maridia, 16), "invalid station index not clamped");
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(catalog, catalog));
        SuperMetroidSaveSlot savedSlot = stationSaves.ReadSlot(0) ??
            throw new InvalidDataException("Verification requires the fixture's saved slot 0.");
        AssertThrows<InvalidOperationException>(() => new FileSelectMapMenuState(new CartridgeAudioState(), savedSlot, 0),
            "file-select map cannot construct a cartridge-backed fallback graph");
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            var native = new FileSelectAreaMapGraphics(area, catalog.Tiles, catalog.Palettes,
                catalog.Screens, catalog.WorldArtwork, catalog.Sprites);
            native.BindLabels(catalog.Labels);
            var compiled = new FileSelectAreaMapGraphics(area, catalog.Tiles, catalog.Palettes, catalog.Screens, catalog.WorldArtwork, catalog.Sprites);
            compiled.BindLabels(catalog.Labels);
            for (int mask = 0; mask < 64; mask++)
            {
                var stations = new ushort[FileSelectMapRomData.AreaCount];
                for (int index = 0; index < stations.Length; index++) stations[index] = (ushort)((mask >> index) & 1);
                AssertTrue(native.Render(stations).SequenceEqual(compiled.Render(stations)), "compiled area order retains label layering and visibility for every area combination");
            }
        }
        Console.WriteLine("Map load metadata: all 34 native anchors, four visibility patterns/scroll trajectories, each saved-menu entry and 384 display-order frames pass with every bus access forbidden; unbound cartridge fallback is rejected.");

        void VerifyStationMenu(AreaId area, int station)
        {
            var saves = stationSaves;
            var snapshot = new SuperMetroidSaveSnapshot { Area = (ushort)area, SaveStation = (ushort)station, Health = 99, MaxHealth = 99 };
            snapshot.MapStationBytes[(int)area] = 1;
            snapshot.UsedSaveStationBytes[(int)area * 2 + station / 8] = (byte)(1 << (station % 8));
            saves.SaveSlot(0, snapshot);
            byte[] before = ReadSram();
            var slot = saves.ReadSlot(0);
            var native = new FileSelectMapMenuState(new CartridgeAudioState(), (slot ?? throw new InvalidOperationException("Verification requires slot.")), 0, catalog);
            var compiled = new FileSelectMapMenuState(new CartridgeAudioState(), slot, 0, catalog);
            // Area-specific expanding-window durations differ. Compare every
            // native phase and stop on its actual endpoint, with a bounded guard.
            for (int tick = 0; tick < 256 && compiled.Phase != FileSelectMapNavigationPhase.Room; tick++)
            {
                SnesButton input = tick == 48 ? SnesButton.Start : 0;
                native.Step(input); compiled.Step(input);
                AssertEqual(native.Phase, compiled.Phase, "each saved station retains entry timing");
                AssertEqual(ScrollState(Scroll(native)), ScrollState(Scroll(compiled)), "each saved station retains menu-owned scroll state");
            }
            AssertEqual(FileSelectMapNavigationPhase.Room, compiled.Phase, $"saved-station fixture {area}/{station} reaches room map");
            AssertTrue(native.Render().SequenceEqual(compiled.Render()), "every saved station has exact composed room-map position");
            AssertTrue(before.SequenceEqual(ReadSram()), "map entry leaves entire SRAM and progression unchanged");
        }
        byte[] ReadSram() => SuperMetroid.Core.Saves.SaveRamCodec.Encode(stationSaves, RetailPresentationFixture());
        static FileSelectMapScroll Scroll(FileSelectMapMenuState menu) => menu.scroll;
        static (Position16<MapPixels>, Position16<MapPixels>, Position16<MapPixels>, MapScrollDirection) ScrollState(FileSelectMapScroll value) =>
            (value.Scroll, value.Minimum, value.Maximum, value.Direction);
    }

    /// <summary>Derives a saved station's map pixel anchor directly from its native load record and room header.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for native map load-station anchors")]
    private static Position16<MapPixels> ReadNativeMapLoadAnchor(ISnesAddressSpace bus, AreaId area, int station)
    {
        // Independent literal native layout: no production LoadStationEntry or
        // CartridgeRoomHeader/state-selection call participates in this oracle.
        int list = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x80c4b5 + (int)area * 2);
        int record = 0x800000 | (list + station * 14);
        int room = 0x8f0000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record);
        AssertEqual((byte)area, bus.ReadCartridgeByte(room + 1), "valid saved-map station remains in its requested area");
        int worldX = SnesAddressMath.OffsetInBank(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 6) + 128 + RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 12));
        int worldY = SnesAddressMath.OffsetInBank(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 8) + RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 10));
        return new((ushort)((bus.ReadCartridgeByte(room + 2) + RoomScreenOf.Pixel(worldX)) << 3),
            (ushort)((bus.ReadCartridgeByte(room + 3) + RoomScreenOf.Pixel(worldY) + 1) << 3));
    }
}
