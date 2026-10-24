using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.InstalledFileSelectMapVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks file-select save marker eligibility, animation, and editable map positions.</summary>
internal static class MapSaveMarkersVerification
{
    /// <summary>Compares all area slots with native marker presence and used-station label rules.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the native save/elevator marker slots per area")]
    internal static void VerifySaveMarkerEligibility(ISnesAddressSpace bus)
    {
        var expectedIds = new List<string>();
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), (int)FrontendSourceDefinitions.SavePointMapPointers + area * 2);
            int nativeMask = 0;
            var expectedIndices = new List<int>();
            for (int index = 0; index < MapSaveMarkerDefinitions.SlotsPerArea; index++)
            {
                ushort x = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.MenuObjectBank | (pointer + index * 4));
                AssertTrue(x != ushort.MaxValue, "all sixteen native save/elevator slots precede the terminator");
                bool usable = x != ushort.MaxValue - 1;
                AssertEqual(usable, MapSaveMarkerDefinitions.IsUsable((AreaId)area, index),
                    "compiled active placement agrees with original marker presence");
                if (usable)
                {
                    nativeMask |= 1 << index;
                    expectedIndices.Add(index);
                    string id = $"{(AreaId)area}.Save.{index}";
                    expectedIds.Add(id);
                    AssertEqual(id, MapSaveMarkerDefinitions.Id((AreaId)area, index), "stable marker identity");
                }
                else
                    AssertThrows<InvalidDataException>(() => MapSaveMarkerDefinitions.Id((AreaId)area, index),
                        "native unused marker remains rejected");
            }
            AssertTrue(expectedIndices.SequenceEqual(MapSaveMarkerDefinitions.Indices((AreaId)area)),
                "lazy enumeration preserves native ascending station order");
            for (int mask = 0; mask <= ushort.MaxValue; mask++)
                AssertEqual((mask & nativeMask) != 0, MapSaveMarkerDefinitions.HasUsedMarker((AreaId)area, (ushort)mask),
                    "every used-station mask retains native area-label eligibility");
            foreach (int invalid in new[] { int.MinValue, -1, 16, 256, int.MaxValue })
            {
                AssertThrows<ArgumentOutOfRangeException>(() => MapSaveMarkerDefinitions.IsUsable((AreaId)area, invalid),
                    "station bounds precede byte narrowing");
                AssertThrows<ArgumentOutOfRangeException>(() => MapSaveMarkerDefinitions.Id((AreaId)area, invalid),
                    "ID station bounds remain rejected");
            }
        }
        AssertEqual(34, expectedIds.Count, "all original marker identities");
        AssertTrue(expectedIds.SequenceEqual(MapSaveMarkerDefinitions.AllIds()), "full marker identity enumeration order");
        foreach (AreaId invalid in new[] { AreaId.Ceres, (AreaId)7, (AreaId)255 })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = MapSaveMarkerDefinitions.Indices(invalid); },
                "invalid area rejects without enumerating the lazy sequence");
            AssertThrows<ArgumentOutOfRangeException>(() => MapSaveMarkerDefinitions.HasUsedMarker(invalid, 0),
                "invalid area rejects even an empty used mask");
            try
            {
                MapSaveMarkerDefinitions.Id(invalid, -1);
                throw new InvalidOperationException("Invalid marker area/index accepted.");
            }
            catch (ArgumentOutOfRangeException error)
            {
                AssertEqual("area", error.ParamName, "area validation retains precedence over index validation");
            }
        }
    }

    /// <summary>Checks marker OAM, edited Maridia position, snapshot rebind, and load handoff timing.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyMapSaveMarkers(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var guard = NewSaveMarkerGuard(bus);
        Suite(nameof(VerifySaveMarkerEligibility), () => VerifySaveMarkerEligibility(bus));
        int valid = 0;
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        for (int index = 0; index < MapSaveMarkerDefinitions.SlotsPerArea; index++)
        {
            var typedArea = (AreaId)area;
            if (!MapSaveMarkerDefinitions.Indices(typedArea).Contains(index))
            {
                AssertThrows<InvalidDataException>(() => new FileSelectStationMarker(typedArea, index, original.SaveMarkers), "cartridge rejects unused marker index");
                AssertThrows<InvalidDataException>(() => new FileSelectStationMarker(typedArea, index, original.SaveMarkers), "compiled definition rejects same unused marker index");
                continue;
            }
            var native = new FileSelectStationMarker(typedArea, index,
                original.SaveMarkers);
            var installed = new FileSelectStationMarker(typedArea, index, original.SaveMarkers);
            AssertEqual(native.MapPosition, installed.MapPosition, "stock selected marker coordinates");
            for (int frame = 0; frame < 128; frame++)
            {
                native.Step(); installed.Step();
                AssertTrue(Draw(native).SequenceEqual(Draw(installed)), "stock marker animation/backing OAM");
            }
            valid++;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(AreaId.Ceres, 0, original.SaveMarkers), "Ceres marker rejected");
        foreach (int invalid in new[] { -1, MapSaveMarkerDefinitions.SlotsPerArea })
            AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(AreaId.Crateria, invalid, original.SaveMarkers), "out-of-range marker rejected");
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(original, original));
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<MapSaveMarkerDocument>(File.ReadAllBytes(Path.Combine(stock, MapSaveMarkerFormat.FileName)), options);
        var points = new Dictionary<string, Position16<MapPixels>>(document.Markers);
        points["Maridia.Save.0"] = points["Maridia.Save.0"] with { X = (ushort)(points["Maridia.Save.0"].X + 8) };
        Directory.CreateDirectory(overrides);
        string path = Path.Combine(overrides, MapSaveMarkerFormat.FileName);
        using (var output = File.Create(path)) MapSaveMarkerLayout.Write(output, document with { Markers = points });
        byte[] bytes = File.ReadAllBytes(path);
        var edited = AreaMapPresentationFiles.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity, "save marker edit changes content identity");
        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        var snapshot = new SuperMetroidSaveSnapshot { Area = (ushort)AreaId.Maridia, SaveStation = 0, Health = 99, MaxHealth = 99 };
        snapshot.MapStationBytes[(int)AreaId.Maridia] = 1;
        snapshot.UsedSaveStationBytes[(int)AreaId.Maridia * 2] = 1;
        saves.SaveSlot(0, snapshot);
        var slot = saves.ReadSlot(0);
        var control = new FileSelectMapMenuState(new CartridgeAudioState(), (slot ?? throw new InvalidOperationException("Verification requires slot.")), 0, original, MenuSheetFixtures.For(original));
        var menu = new FileSelectMapMenuState(new CartridgeAudioState(), slot, 0, edited, MenuSheetFixtures.For(edited));
        for (int frame = 0; frame < 104; frame++)
        {
            SnesButton input = frame == 48 ? SnesButton.Start : 0;
            control.Step(input); menu.Step(input);
            AssertEqual(control.Phase, menu.Phase, "cosmetic marker edit preserves entry/navigation timing");
        }
        AssertEqual(FileSelectMapNavigationPhase.Room, menu.Phase, "edited marker fixture reaches room map");
        AssertTrue(!control.Render().SequenceEqual(menu.Render()), "selected save marker edit changes actual room-map pixels");
        using var captured = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(captured, menu); captured.Position = 0;
        menu = DebuggerObjectGraphSerializer.Deserialize<FileSelectMapMenuState>(captured);
        menu.BindFixtureArtwork(original);
        AssertTrue(control.Render().SequenceEqual(menu.Render()), "restored marker rebind preserves scroll/animation and restores current stock position");
        for (int frame = 0; frame < 80; frame++)
        {
            SnesButton input = frame == 0 ? SnesButton.Start : 0;
            control.Step(input); menu.Step(input);
            AssertEqual(control.Phase, menu.Phase, "marker rebind preserves load phase");
            AssertEqual(control.LoadRequested, menu.LoadRequested, "marker rebind preserves load handoff timing");
            AssertTrue(control.Render().SequenceEqual(menu.Render()), "marker rebind preserves animation through load fade");
        }
        AssertTrue(menu.LoadRequested, "fixture actually requests loading");
        AssertEqual((ushort)AreaId.Maridia, slot.Area, "layout does not alter selected save area");
        AssertEqual(0, slot.SaveStation, "layout does not alter selected load station");
        string rebuilt = Path.Combine(overrides, "rebuilt");
        SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, AreaMapPresentationFiles.Load(rebuilt, overrides).ContentIdentity, "save marker override survives stock replacement");
        AssertTrue(bytes.SequenceEqual(File.ReadAllBytes(path)), "save marker override bytes preserved");
        foreach (var bad in new[] { document with { Version = 2 }, document with { Markers = new() },
            document with { Markers = points.ToDictionary(pair => pair.Key, pair => pair.Value with { X = ushort.MaxValue }) } })
            AssertThrows<InvalidDataException>(() => MapSaveMarkerLayout.Write(new MemoryStream(), bad), "invalid save marker layout rejected");
        File.WriteAllText(path, "{bad save markers");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationFiles.Load(stock, overrides), "bad save marker override cannot fall back");
        AssertEqual("{bad save markers", File.ReadAllText(path), "bad save marker override preserved");
        string rebuiltPath = Path.Combine(rebuilt, MapSaveMarkerFormat.FileName);
        File.Delete(rebuiltPath);
        AssertThrows<FileNotFoundException>(() => AreaMapPresentationFiles.Load(rebuilt, null), "missing stock save markers rejected");
        File.WriteAllText(rebuiltPath, "bad stock");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationFiles.Load(rebuilt, null), "corrupt stock save markers rejected");
        Console.WriteLine($"Save markers: {valid} coordinates/128-tick animations, unused indices, transitions with save-coordinate ROM reads blocked, edited pixels and restored load timing pass.");

        byte[] Draw(FileSelectStationMarker marker)
        {
            var oam = ScreenSpritesFixtures.Blank(); oam.BeginFrame();
            marker.Draw(oam, new(24, 8), MenuSheetFixtures.MapSprites(original.Sprites));
            oam.FinalizeFrame();
            return oam.LowTable.ConcatToArray(oam.HighTable);
        }
    }

    /// <summary>Blocks runtime reads of native area pointer and save-marker coordinate tables.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle walking the native selected-save marker coordinate lists to forbid them")]
    private static MutableCartridgeReadGuard NewSaveMarkerGuard(ISnesAddressSpace source)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        var blocked = new HashSet<int>();
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            int entry = (int)FrontendSourceDefinitions.SavePointMapPointers + area * 2;
            int list = RomDataReader.ReadWordFixedBank(cartridge, entry);
            blocked.Add(entry); blocked.Add(entry + 1);
            for (int offset = 0; offset < MapSaveMarkerDefinitions.SlotsPerArea * 4; offset++)
                blocked.Add(FileSelectMapRomData.MenuObjectBank | (list + offset));
        }
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(blocked.Contains, _ => "Installed selected-save marker read cartridge coordinate table.");
        return guard;
    }
}
