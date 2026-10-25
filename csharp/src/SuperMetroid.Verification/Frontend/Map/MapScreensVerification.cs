using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.InstalledFileSelectMapVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed file-select world and room screens, artwork overrides, and menu rebinding against native presentation.</summary>
internal static class MapScreensVerification
{
    /// <summary>Compares six areas' VRAM and pixels, then verifies independent JSON and PNG edits, restored menus, and strict resource validation.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyMapScreens(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var guard = NewMapScreenGuard(bus);
        var system = new SharedGameState();
        system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
        for (int index = 0; index < MapScreenDefinitions.ZebesAreas; index++)
        {
            var native = World(bus, original, index);
            var installed = World(guard, original, index);
            byte[] usedNativeVram = MenuScreenFixtures.Vram(native.screen).Bytes.ToArray();
            AssertTrue(usedNativeVram.SequenceEqual(MenuScreenFixtures.Vram(installed.screen).Bytes),
                "stock world resources reproduce native VRAM");
            foreach (bool backdropMath in new[] { false, true })
                AssertTrue(native.RenderBackgrounds(backdropMath).SequenceEqual(installed.RenderBackgrounds(backdropMath)), "all six world layers preserve native additive pixels");
            installed.SelectArea((index + 1) % MapScreenDefinitions.ZebesAreas);
            installed.SelectArea(index);
            AssertTrue(native.RenderBackgrounds().SequenceEqual(installed.RenderBackgrounds()), "reselection reloads authored world background without ROM reads");
            var area = (AreaId)index;
            var nativeRoom = new FileSelectRoomMapGraphics(system, area,
                mapPresentation: original, sheets: MenuSheetFixtures.For(original));
            var installedRoom = new FileSelectRoomMapGraphics(system, area, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
            AssertTrue(MenuScreenFixtures.Vram(nativeRoom.screen).ImageEquals(MenuScreenFixtures.Vram(installedRoom.screen)), "stock resolved frame/footer/area label matches full native VRAM");
            AssertTrue(nativeRoom.RenderFrameOnly().SequenceEqual(installedRoom.RenderFrameOnly()), "native frame-only transition pixels match");
            foreach (ushort scroll in new ushort[] { 0, 8, 127, 255 })
                AssertTrue(nativeRoom.RenderBackgrounds(new(scroll, scroll)).SequenceEqual(installedRoom.RenderBackgrounds(new(scroll, scroll))), "fixed room frame remains exact while map scrolls");
        }
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(original, original, verifyCapturedRendering: true));
        Directory.CreateDirectory(overrides);
        string jsonPath = Path.Combine(overrides, MapScreenDefinitions.FileName);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<MapScreenDocument>(File.ReadAllBytes(Path.Combine(stock, MapScreenDefinitions.FileName)), options);
        foreach (MapScreenPage definition in MapScreenDefinitions.Pages())
        {
            string name = definition.Id.Name;
            var isolated = new Dictionary<string, MapPresentationCell[]>(document.Pages);
            isolated[name] = document.Pages[name].Select(cell => cell with { Palette = (cell.Palette + 1) % MapPresentationFormat.PaletteCount, FlipX = !cell.FlipX }).ToArray();
            using (var output = File.Create(jsonPath)) MapScreenPresentation.Write(output, document with { Pages = isolated });
            var oneEdit = MapPresentationFiles.Load(stock, overrides);
            bool room = definition.Id is MapScreenPageId.RoomFrame;
            AreaId area = definition.Id switch
            {
                MapScreenPageId.WorldForeground => AreaId.Crateria,
                MapScreenPageId.WorldBackground background => background.Area,
                MapScreenPageId.RoomFrame frame => frame.Area,
                _ => throw new InvalidOperationException($"Undefined map-screen page {name}."),
            };
            RgbaPixel[] before = room ? new FileSelectRoomMapGraphics(system, area, mapPresentation: original, sheets: MenuSheetFixtures.For(original)).RenderFrameOnly()
                : World(guard, original, (int)area).RenderBackgrounds();
            RgbaPixel[] after = room ? new FileSelectRoomMapGraphics(system, area, mapPresentation: oneEdit, sheets: MenuSheetFixtures.For(oneEdit)).RenderFrameOnly()
                : World(guard, oneEdit, (int)area).RenderBackgrounds();
            AssertTrue(!before.SequenceEqual(after), $"independent {name} JSON edit reaches its composed pixels");
        }
        var pages = document.Pages.ToDictionary(pair => pair.Key, pair => pair.Value.Select(cell => cell with
        { Palette = (cell.Palette + 1) % MapPresentationFormat.PaletteCount, FlipX = !cell.FlipX }).ToArray());
        using (var output = File.Create(jsonPath)) MapScreenPresentation.Write(output, document with { Pages = pages });
        var edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity, "screen JSON changes catalog identity");
        var worldBefore = World(guard, original, (int)AreaId.Maridia);
        var worldAfter = World(guard, edited, (int)AreaId.Maridia);
        AssertTrue(!worldBefore.RenderBackgrounds().SequenceEqual(worldAfter.RenderBackgrounds()), "JSON world layer edits change composed pixels");
        var roomBefore = new FileSelectRoomMapGraphics(system, AreaId.Maridia, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
        var roomAfter = new FileSelectRoomMapGraphics(system, AreaId.Maridia, mapPresentation: edited, sheets: MenuSheetFixtures.For(edited));
        AssertTrue(!roomBefore.RenderFrameOnly().SequenceEqual(roomAfter.RenderFrameOnly()), "JSON room frame edits change transition pixels");
        using var captured = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(captured, worldAfter); captured.Position = 0;
        worldAfter = DebuggerObjectGraphSerializer.Deserialize<FileSelectAreaMapGraphics>(captured);
        worldAfter.BindFixtureArtwork(original);
        AssertTrue(worldBefore.RenderBackgrounds().SequenceEqual(worldAfter.RenderBackgrounds()), "restored world replaces stale edited page without changing selected area");
        Suite(nameof(VerifyScreenMenuRebinding), () => VerifyScreenMenuRebinding(original, edited));
        // Test each PNG independently: a combined edit must not mask a disconnected resource.
        File.WriteAllBytes(jsonPath, File.ReadAllBytes(Path.Combine(stock, MapScreenDefinitions.FileName)));
        foreach (var atlas in new[]
        {
            new VerifyMapScreensCase(WorldMapArtworkFormat.ForegroundFile, WorldMapArtworkFormat.ForegroundHeight, 16),
            new VerifyMapScreensCase(WorldMapArtworkFormat.BackgroundFile, WorldMapArtworkFormat.BackgroundHeight, 4)
        })
        {
            string path = Path.Combine(overrides, atlas.First);
            IndexedPngImage image;
            using (var input = File.OpenRead(Path.Combine(stock, atlas.First))) image = IndexedPng.Read(input, new(WorldMapArtworkFormat.Width, checked((ushort)atlas.Second)));
            for (int pixel = 0; pixel < image.Pixels.Length; pixel++) image.Pixels[pixel] = (byte)((image.Pixels[pixel] + 1) % atlas.Third);
            using (var output = File.Create(path)) IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            var artEdit = MapPresentationFiles.Load(stock, overrides);
            AssertTrue(artEdit.ContentIdentity != original.ContentIdentity, "each world PNG affects identity independently");
            var changedWorld = World(guard, artEdit, (int)AreaId.Maridia);
            AssertTrue(!worldBefore.RenderBackgrounds().SequenceEqual(changedWorld.RenderBackgrounds()), "each world PNG edit reaches composed world pixels independently");
            changedWorld.BindFixtureArtwork(original);
            AssertTrue(worldBefore.RenderBackgrounds().SequenceEqual(changedWorld.RenderBackgrounds()), "world artwork rebind restores exact stock pixels");
            File.WriteAllText(path, "invalid PNG");
            AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "corrupt world PNG override cannot silently select stock");
            using (var output = File.Create(path)) IndexedPng.Write(output, new(1, 1), [0], image.Colours);
            AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "wrong world PNG dimensions rejected");
            image.Pixels[0] = (byte)atlas.Third;
            using (var output = File.Create(path)) IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, SnesGraphics.DiagnosticPalette(atlas.Third + 1));
            AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "world PNG index outside native depth rejected");
            File.WriteAllBytes(path, File.ReadAllBytes(Path.Combine(stock, atlas.First)));
        }
        // Preserve the original bytes around invalid-resource tests; all paths are
        // this fixture's isolated generated stock, never an installed player's files.
        foreach (string name in new[] { MapScreenDefinitions.FileName, WorldMapArtworkFormat.ForegroundFile, WorldMapArtworkFormat.BackgroundFile })
        {
            string path = Path.Combine(stock, name);
            byte[] bytes = File.ReadAllBytes(path);
            try
            {
                File.WriteAllText(path, "corrupt stock");
                AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "stock resource hash failure remains visible with override present");
                File.Delete(path);
                AssertThrows<FileNotFoundException>(() => MapPresentationFiles.Load(stock, overrides), "missing stock screen resource remains visible with override present");
            }
            finally { File.WriteAllBytes(path, bytes); }
        }
        var invalidPages = new Dictionary<string, MapPresentationCell[]>(document.Pages);
        invalidPages.Remove(MapScreenDefinitions.WorldForeground.Name);
        AssertThrows<InvalidDataException>(() => MapScreenPresentation.Write(new MemoryStream(), document with { Pages = invalidPages }), "missing named screen page rejected");
        invalidPages = new(document.Pages);
        invalidPages[MapScreenDefinitions.WorldForeground.Name] = [];
        AssertThrows<InvalidDataException>(() => MapScreenPresentation.Write(new MemoryStream(), document with { Pages = invalidPages }), "wrong page dimensions rejected");
        invalidPages[MapScreenDefinitions.WorldForeground.Name] = document.Pages[MapScreenDefinitions.WorldForeground.Name].ToArray();
        invalidPages[MapScreenDefinitions.WorldForeground.Name][0] = invalidPages[MapScreenDefinitions.WorldForeground.Name][0] with { TileRow = 999 };
        AssertThrows<InvalidDataException>(() => MapScreenPresentation.Write(new MemoryStream(), document with { Pages = invalidPages }), "out of atlas reference rejected");
        File.WriteAllText(jsonPath, "broken JSON");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "corrupt screen override fails loudly");
        AssertEqual("broken JSON", File.ReadAllText(jsonPath), "invalid screen override never overwritten");
        Console.WriteLine("Map screen resources: six native world/room VRAM and pixel comparisons, guarded full menu, independent JSON/PNG edits, restored content and strict failures pass.");

        static FileSelectAreaMapGraphics World(ISnesAddressSpace addressSpace, AreaMapPresentationCatalog catalog, int area) =>
            new(area, catalog.Tiles, catalog.Palettes, catalog.Screens,
                catalog.WorldArtwork, catalog.Sprites, MenuSheetFixtures.For(catalog));
    }

    /// <summary>Runs file-select transition and load phases with edited screens, then restores and rebinds the menu to stock art.</summary>
    private static void VerifyScreenMenuRebinding(AreaMapPresentationCatalog original, AreaMapPresentationCatalog edited)
    {
        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        var data = new SuperMetroidSaveSnapshot { Area = (ushort)AreaId.Maridia, SaveStation = 0, Health = 99, MaxHealth = 99 };
        data.MapStationBytes[(int)AreaId.Maridia] = 1;
        data.UsedSaveStationBytes[(int)AreaId.Maridia * 2] = 1;
        saves.SaveSlot(0, data);
        var slot = saves.ReadSlot(0);
        var control = new FileSelectMapMenuState(new AudioQueues(), (slot ?? throw new InvalidOperationException("Verification requires slot.")), 0, original, MenuSheetFixtures.For(original));
        var changed = new FileSelectMapMenuState(new AudioQueues(), slot, 0, edited, MenuSheetFixtures.For(edited));
        for (int tick = 0; tick < 104; tick++)
        {
            ControllerButton input = tick == 48 ? ControllerButton.Start : 0;
            control.Step(input); changed.Step(input);
            AssertEqual(control.Phase, changed.Phase, "layout replacement never changes menu transition timing");
        }
        AssertTrue(!control.Render().SequenceEqual(changed.Render()), "normal installed menu displays edited room frame");
        using var state = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(state, changed); state.Position = 0;
        changed = DebuggerObjectGraphSerializer.Deserialize<FileSelectMapMenuState>(state);
        changed.BindFixtureArtwork(original);
        AssertTrue(control.Render().SequenceEqual(changed.Render()), "whole restored menu rebinds current frame pixels");
        for (int tick = 0; tick < 80; tick++)
        {
            ControllerButton input = tick == 0 ? ControllerButton.Start : 0;
            control.Step(input); changed.Step(input);
            AssertEqual(control.Phase, changed.Phase, "restored menu retains load phase");
            AssertEqual(control.LoadRequested, changed.LoadRequested, "restored menu retains load handoff timing");
            AssertTrue(control.Render().SequenceEqual(changed.Render()), "restored menu retains exact load fade pixels");
        }
        AssertTrue(changed.LoadRequested, "screen fixture reaches load handoff");
        AssertTrue((saves.ReadSlot(0) ?? throw new InvalidOperationException("Verification requires saves.ReadSlot(0).")).ToSnapshot().MapStationBytes.SequenceEqual(data.MapStationBytes), "screen edits preserve saved map progression");
    }

    /// <summary>Forbids runtime cartridge reads across all migrated world pages, room frames, labels, and map character sheets.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle reading the native room-label pointers to forbid every map-screen presentation source")]
    private static MutableCartridgeReadGuard NewMapScreenGuard(ISnesAddressSpace source)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        var forbidden = new HashSet<int>();
        Add(MapSourceDefinitions.Menu.Background, MapScreenDefinitions.PageBytes);
        Add(MapSourceDefinitions.FileSelectMap.AreaForeground, MapScreenDefinitions.PageBytes);
        Add(MapSourceDefinitions.FileSelectMap.AreaBackgrounds, MapScreenDefinitions.PageBytes * MapScreenDefinitions.ZebesAreas);
        Add(MapSourceDefinitions.Menu.ForegroundCharacters, WorldMapArtworkFormat.ForegroundBytes);
        Add(MapSourceDefinitions.Menu.BackgroundCharacters, WorldMapArtworkFormat.BackgroundBytes);
        Add(MapSourceDefinitions.FileSelectMap.RoomFrame, 1600);
        Add(MapSourceDefinitions.FileSelectMap.RoomFrameFooter, 322);
        Add(MapSourceDefinitions.FileSelectMap.AreaLabelPointers, MapScreenDefinitions.ZebesAreas * 2);
        for (int area = 0; area < MapScreenDefinitions.ZebesAreas; area++)
            Add(MapSourceDefinitions.Menu.ObjectBank | RomDataReader.ReadWordFixedBank(cartridge, MapSourceDefinitions.FileSelectMap.AreaLabelPointers + area * 2), 24);
        void Add(int start, int length) { for (int offset = 0; offset < length; offset++) forbidden.Add(start + offset); }
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(forbidden.Contains, address => $"Installed map screens read cartridge presentation at {address:X6}.");
        return guard;
    }
}
