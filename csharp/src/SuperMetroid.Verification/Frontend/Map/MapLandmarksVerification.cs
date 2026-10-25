using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed map landmarks against native OAM and edited file-select and pause pixels.</summary>
internal static class MapLandmarksVerification
{
    /// <summary>Checks boss and elevator visibility across area state, then validates layout overrides and rebinds.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyMapLandmarks(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var guard = NewLandmarkGuard(bus);
        var iconGuard = NewLandmarkGuard(bus, blockGunship: true);
        int cases = 0;
        foreach (AreaId area in Enum.GetValues<AreaId>())
        foreach (bool downloaded in new[] { false, true })
        for (int bits = 0; bits <= byte.MaxValue; bits++)
        {
            var system = new Bank80SystemState();
            // SRAM preserves all eight bits, including unnamed ones; use its raw
            // restore boundary rather than passing unnamed flags to the gameplay API.
            byte[] bossBytes = new byte[Bank80SystemState.AreaCount];
            bossBytes[(int)area] = (byte)bits;
            system.LoadBossBytes(bossBytes);
            if (downloaded) system.SetAreaMapAcquired(area);
            var native = new FileSelectMapIcons(system, area);
            var installed = new FileSelectMapIcons(system, area);
            native.BindLandmarks(original.Landmarks);
            native.BindSprites(MenuSheetFixtures.MapSprites(original.Sprites));
            installed.BindLandmarks(original.Landmarks);
            installed.BindSprites(MenuSheetFixtures.MapSprites(original.Sprites));
            AssertTrue(Draw(native, area).SequenceEqual(Draw(installed, area)), $"landmark OAM {area}/{bits}/{downloaded}");
            AssertEqual((byte)bits, system.GetBossBitsRaw(area), "landmark drawing preserves boss state");
            AssertEqual(downloaded, system.HasAreaMap(area), "landmark drawing preserves map acquisition");
            cases++;
        }
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<MapLandmarkDocument>(File.ReadAllBytes(Path.Combine(stock, MapLandmarkFormat.FileName)), options);
        var points = document.Markers.ToDictionary(pair => pair.Key, pair => pair.Value with { X = (ushort)(pair.Value.X + 8) });
        Directory.CreateDirectory(overrides);
        string path = Path.Combine(overrides, MapLandmarkFormat.FileName);
        using (var stream = File.Create(path)) MapLandmarkLayout.Write(stream, document with { Markers = points });
        byte[] bytes = File.ReadAllBytes(path);
        var edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "landmark edit changes content identity");
        // An undiscovered living boss remains invisible even after its visual is moved.
        var unknown = new Bank80SystemState();
        var hidden = new FileSelectMapIcons(unknown, AreaId.WreckedShip);
        hidden.BindSprites(MenuSheetFixtures.MapSprites(original.Sprites));
        hidden.BindLandmarks(original.Landmarks); byte[] hiddenBefore = Draw(hidden, AreaId.WreckedShip);
        hidden.BindLandmarks(edited.Landmarks);
        AssertTrue(hiddenBefore.SequenceEqual(Draw(hidden, AreaId.WreckedShip)), "landmark edit cannot reveal living boss/elevators without map download");
        // Observe all families in real renderers: Wrecked Ship has boss/elevators;
        // Crateria has the independent gunship marker. Ceres boss data is covered above.
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.WreckedShip })
        {
            var system = new Bank80SystemState(); system.SetAreaMapAcquired(area); system.SetBossBits(area, (BossBits)1);
            system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
            var marker = new FileSelectStationMarker(area, 0, original.SaveMarkers);
            var native = new FileSelectRoomMapGraphics(system, area,
                mapPresentation: original, sheets: MenuSheetFixtures.For(original));
            var room = new FileSelectRoomMapGraphics(system, area, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
            var before = native.Render(new(0, 0), marker);
            AssertTrue(before.SequenceEqual(room.Render(new(0, 0), marker)), "full file-select landmark stock pixel parity");
            room.BindFixtureArtwork(edited);
            AssertTrue(!before.SequenceEqual(room.Render(new(0, 0), marker)), "edited landmarks change file-select pixels");
            using var capture = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(capture, room); capture.Position = 0;
            room = DebuggerObjectGraphSerializer.Deserialize<FileSelectRoomMapGraphics>(capture);
            room.BindFixtureArtwork(original);
            AssertTrue(before.SequenceEqual(room.Render(new(0, 0), marker)), "restored file-select uses current landmark content");
            if (area != AreaId.WreckedShip) continue;
            var pauseNative = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(), System = system, AreaIndex = area, RoomMapPosition = new(19, 20), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
            var pause = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(), System = system, AreaIndex = area, RoomMapPosition = new(19, 20), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
            var pauseBefore = pauseNative.Render();
            AssertTrue(pauseBefore.SequenceEqual(pause.Render()), "pause stock boss pixels with ROM coordinates blocked");
            pause.BindFixtureArtwork(edited);
            AssertTrue(!pauseBefore.SequenceEqual(pause.Render()), "boss layout edit changes pause pixels");
            pause.BindFixtureArtwork(original);
            AssertTrue(pauseBefore.SequenceEqual(pause.Render()), "pause boss layout rebind restores pixels");
        }
        string rebuilt = Path.Combine(overrides, "rebuilt");
        SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, MapPresentationFiles.Load(rebuilt, overrides).ContentIdentity, "landmarks survive stock replacement");
        AssertTrue(bytes.SequenceEqual(File.ReadAllBytes(path)), "landmark override bytes preserved");
        foreach (var bad in new[] { document with { Version = 0 }, document with { Markers = new() },
            document with { Markers = points.ToDictionary(pair => pair.Key, pair => pair.Value with { Y = ushort.MaxValue }) } })
            AssertThrows<InvalidDataException>(() => MapLandmarkLayout.Write(new MemoryStream(), bad), "invalid landmark schema/identity/position rejected");
        File.WriteAllText(path, "{bad landmarks");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "bad landmark override cannot fall back");
        AssertEqual("{bad landmarks", File.ReadAllText(path), "bad landmark override preserved for repair");
        string rebuiltPath = Path.Combine(rebuilt, MapLandmarkFormat.FileName);
        File.Delete(rebuiltPath);
        AssertThrows<FileNotFoundException>(() => MapPresentationFiles.Load(rebuilt, null), "missing stock landmarks rejected");
        File.WriteAllText(rebuiltPath, "corrupt landmarks");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(rebuilt, null), "corrupt stock landmarks rejected");
        Console.WriteLine($"Map landmarks: {cases} boss/download states match native OAM; pause/file-select edited pixels, restored rebind and override validation pass.");

        static byte[] Draw(FileSelectMapIcons icons, AreaId area)
        {
            var oam = ScreenSpritesFixtures.Blank(); oam.BeginFrame(); icons.DrawBossMarkers(oam, new(24, 8));
            if (area != AreaId.Ceres) icons.DrawAfterMarker(oam, new(24, 8));
            oam.FinalizeFrame(); return oam.LowTable.ConcatToArray(oam.HighTable);
        }
    }

    /// <summary>Blocks native landmark coordinate reads after their installed presentation is bound.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle walking the native boss, elevator and gunship landmark lists to forbid them")]
    private static MutableCartridgeReadGuard NewLandmarkGuard(ISnesAddressSpace source, bool blockGunship = false)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        var blocked = new HashSet<int>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            AddList(MapSourceDefinitions.Landmarks.BossLists, area, 4);
            if (area != AreaId.Ceres) AddList(MapSourceDefinitions.Landmarks.ElevatorLists, area, 6);
        }
        // Full room rendering still reads selected-save coordinates. The direct
        // icon comparison can additionally block the shared gunship coordinate.
        if (blockGunship)
        {
            int table = (int)FrontendSourceDefinitions.SavePointMapPointers;
            int pointer = RomDataReader.ReadWordFixedBank(cartridge, table);
            blocked.Add(table); blocked.Add(table + 1);
            for (int i = 0; i < 4; i++) blocked.Add(MapSourceDefinitions.Menu.ObjectBank | (pointer + i));
        }
        void AddList(int table, AreaId area, int stride)
        {
            int entry = table + AreaIds.ToIndex(area) * 2;
            blocked.Add(entry); blocked.Add(entry + 1);
            int list = RomDataReader.ReadWordFixedBank(cartridge, entry);
            if (list == 0) return;
            for (int record = 0; ; record++)
            {
                int address = MapSourceDefinitions.Menu.ObjectBank | (list + record * stride);
                blocked.Add(address); blocked.Add(address + 1);
                if (RomDataReader.ReadWordFixedBank(cartridge, address) == ushort.MaxValue) break;
                for (int i = 2; i < stride; i++) blocked.Add(address + i);
            }
        }
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(blocked.Contains, _ => "Installed landmarks read boss/elevator ROM definitions.");
        return guard;
    }
}
