using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyMapScreens(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var guard = new MapScreenReadGuard(bus);
        var system = new Bank80SystemState();
        system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
        for (int index = 0; index < MapScreenDefinitions.ZebesAreas; index++)
        {
            var native = new FileSelectAreaMapGraphics(bus, index);
            var installed = World(guard, original, index);
            byte[] usedNativeVram = native.Vram.Bytes.ToArray();
            // The world view never references the initial BG2 page. Installed
            // maps intentionally omit it; every other byte must remain exact.
            usedNativeVram.AsSpan(MenuPpuState.Bg2TilemapWord * 2, MapScreenDefinitions.PageBytes).Clear();
            AssertTrue(usedNativeVram.AsSpan().SequenceEqual(installed.Vram.Bytes), "stock world resources reproduce native VRAM except the unused initial BG2 template");
            foreach (bool backdropMath in new[] { false, true })
                AssertTrue(native.RenderBackgrounds(backdropMath).AsSpan().SequenceEqual(installed.RenderBackgrounds(backdropMath)), "all six world layers preserve native additive pixels");
            installed.SelectArea((index + 1) % MapScreenDefinitions.ZebesAreas);
            installed.SelectArea(index);
            AssertTrue(native.RenderBackgrounds().AsSpan().SequenceEqual(installed.RenderBackgrounds()), "reselection reloads authored world background without ROM reads");
            var area = (AreaId)index;
            var nativeRoom = new FileSelectRoomMapGraphics(bus, system, area);
            var installedRoom = new FileSelectRoomMapGraphics(guard, system, area, mapPresentation: original);
            AssertTrue(nativeRoom.Vram.Bytes.SequenceEqual(installedRoom.Vram.Bytes), "stock resolved frame/footer/area label matches full native VRAM");
            AssertTrue(nativeRoom.RenderFrameOnly().AsSpan().SequenceEqual(installedRoom.RenderFrameOnly()), "native frame-only transition pixels match");
            foreach (ushort scroll in new ushort[] { 0, 8, 127, 255 })
                AssertTrue(nativeRoom.RenderBackgrounds(scroll, scroll).AsSpan().SequenceEqual(installedRoom.RenderBackgrounds(scroll, scroll)), "fixed room frame remains exact while map scrolls");
        }
        VerifyInstalledFileSelectMenu(bus, guard, original, original, verifyCapturedRendering: true);
        Directory.CreateDirectory(overrides);
        string jsonPath = Path.Combine(overrides, MapScreenDefinitions.FileName);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<MapScreenDocument>(File.ReadAllBytes(Path.Combine(stock, MapScreenDefinitions.FileName)), options)!;
        foreach (var page in document.Pages)
        {
            var isolated = new Dictionary<string, MapPresentationCell[]>(document.Pages);
            isolated[page.Key] = page.Value.Select(cell => cell with { Palette = (cell.Palette + 1) % MapPresentationFormat.PaletteCount, FlipX = !cell.FlipX }).ToArray();
            using (var output = File.Create(jsonPath)) MapScreenPresentation.Write(output, document with { Pages = isolated });
            var oneEdit = AreaMapPresentationCatalog.Load(stock, overrides);
            bool room = page.Key.StartsWith("Room.", StringComparison.Ordinal);
            var area = page.Key == MapScreenDefinitions.WorldForeground ? AreaId.Crateria : Enum.Parse<AreaId>(page.Key.Split('.')[1]);
            Rgba32[] before = room ? new FileSelectRoomMapGraphics(guard, system, area, mapPresentation: original).RenderFrameOnly()
                : World(guard, original, (int)area).RenderBackgrounds();
            Rgba32[] after = room ? new FileSelectRoomMapGraphics(guard, system, area, mapPresentation: oneEdit).RenderFrameOnly()
                : World(guard, oneEdit, (int)area).RenderBackgrounds();
            AssertTrue(!before.AsSpan().SequenceEqual(after), $"independent {page.Key} JSON edit reaches its composed pixels");
        }
        var pages = document.Pages.ToDictionary(pair => pair.Key, pair => pair.Value.Select(cell => cell with
        { Palette = (cell.Palette + 1) % MapPresentationFormat.PaletteCount, FlipX = !cell.FlipX }).ToArray());
        using (var output = File.Create(jsonPath)) MapScreenPresentation.Write(output, document with { Pages = pages });
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity, "screen JSON changes catalog identity");
        var worldBefore = World(guard, original, (int)AreaId.Maridia);
        var worldAfter = World(guard, edited, (int)AreaId.Maridia);
        AssertTrue(!worldBefore.RenderBackgrounds().AsSpan().SequenceEqual(worldAfter.RenderBackgrounds()), "JSON world layer edits change composed pixels");
        var roomBefore = new FileSelectRoomMapGraphics(guard, system, AreaId.Maridia, mapPresentation: original);
        var roomAfter = new FileSelectRoomMapGraphics(guard, system, AreaId.Maridia, mapPresentation: edited);
        AssertTrue(!roomBefore.RenderFrameOnly().AsSpan().SequenceEqual(roomAfter.RenderFrameOnly()), "JSON room frame edits change transition pixels");
        using var captured = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(captured, worldAfter); captured.Position = 0;
        worldAfter = DebuggerObjectGraphSerializer.Deserialize<FileSelectAreaMapGraphics>(captured);
        worldAfter.BindScreens(original.Screens, original.WorldArtwork);
        AssertTrue(worldBefore.RenderBackgrounds().AsSpan().SequenceEqual(worldAfter.RenderBackgrounds()), "restored world replaces stale edited page without changing selected area");
        VerifyScreenMenuRebinding(bus, guard, original, edited);
        // Test each PNG independently: a combined edit must not mask a disconnected resource.
        File.WriteAllBytes(jsonPath, File.ReadAllBytes(Path.Combine(stock, MapScreenDefinitions.FileName)));
        foreach (var atlas in new[]
        {
            (WorldMapArtworkFormat.ForegroundFile, WorldMapArtworkFormat.ForegroundHeight, 16),
            (WorldMapArtworkFormat.BackgroundFile, WorldMapArtworkFormat.BackgroundHeight, 4)
        })
        {
            string path = Path.Combine(overrides, atlas.Item1);
            IndexedPngImage image;
            using (var input = File.OpenRead(Path.Combine(stock, atlas.Item1))) image = IndexedPng.Read(input, WorldMapArtworkFormat.Width, atlas.Item2);
            for (int pixel = 0; pixel < image.Pixels.Length; pixel++) image.Pixels[pixel] = (byte)((image.Pixels[pixel] + 1) % atlas.Item3);
            using (var output = File.Create(path)) IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            var artEdit = AreaMapPresentationCatalog.Load(stock, overrides);
            AssertTrue(artEdit.ContentIdentity != original.ContentIdentity, "each world PNG affects identity independently");
            var changedWorld = World(guard, artEdit, (int)AreaId.Maridia);
            AssertTrue(!worldBefore.RenderBackgrounds().AsSpan().SequenceEqual(changedWorld.RenderBackgrounds()), "each world PNG edit reaches composed world pixels independently");
            changedWorld.BindScreens(original.Screens, original.WorldArtwork);
            AssertTrue(worldBefore.RenderBackgrounds().AsSpan().SequenceEqual(changedWorld.RenderBackgrounds()), "world artwork rebind restores exact stock pixels");
            File.WriteAllText(path, "invalid PNG");
            AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "corrupt world PNG override cannot silently select stock");
            using (var output = File.Create(path)) IndexedPng.Write(output, 1, 1, [0], image.Palette);
            AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "wrong world PNG dimensions rejected");
            image.Pixels[0] = (byte)atlas.Item3;
            using (var output = File.Create(path)) IndexedPng.Write(output, image.Width, image.Height, image.Pixels, SnesGraphics.DiagnosticPalette(atlas.Item3 + 1));
            AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "world PNG index outside native depth rejected");
            File.WriteAllBytes(path, File.ReadAllBytes(Path.Combine(stock, atlas.Item1)));
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
                AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "stock resource hash failure remains visible with override present");
                File.Delete(path);
                AssertThrows<FileNotFoundException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "missing stock screen resource remains visible with override present");
            }
            finally { File.WriteAllBytes(path, bytes); }
        }
        var invalidPages = new Dictionary<string, MapPresentationCell[]>(document.Pages);
        invalidPages.Remove(MapScreenDefinitions.WorldForeground);
        AssertThrows<InvalidDataException>(() => MapScreenPresentation.Write(new MemoryStream(), document with { Pages = invalidPages }), "missing named screen page rejected");
        invalidPages = new(document.Pages);
        invalidPages[MapScreenDefinitions.WorldForeground] = [];
        AssertThrows<InvalidDataException>(() => MapScreenPresentation.Write(new MemoryStream(), document with { Pages = invalidPages }), "wrong page dimensions rejected");
        invalidPages[MapScreenDefinitions.WorldForeground] = document.Pages[MapScreenDefinitions.WorldForeground].ToArray();
        invalidPages[MapScreenDefinitions.WorldForeground][0] = invalidPages[MapScreenDefinitions.WorldForeground][0] with { TileRow = 999 };
        AssertThrows<InvalidDataException>(() => MapScreenPresentation.Write(new MemoryStream(), document with { Pages = invalidPages }), "out of atlas reference rejected");
        File.WriteAllText(jsonPath, "broken JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "corrupt screen override fails loudly");
        AssertEqual("broken JSON", File.ReadAllText(jsonPath), "invalid screen override never overwritten");
        Console.WriteLine("Map screen resources: six native world/room VRAM and pixel comparisons, guarded full menu, independent JSON/PNG edits, restored content and strict failures pass.");

        static FileSelectAreaMapGraphics World(ISnesAddressSpace addressSpace, AreaMapPresentationCatalog catalog, int area) =>
            new(addressSpace, area, catalog.Tiles, catalog.Palettes, catalog.Screens, catalog.WorldArtwork);
    }

    private static void VerifyScreenMenuRebinding(ISnesAddressSpace bus, ISnesAddressSpace guard, AreaMapPresentationCatalog original, AreaMapPresentationCatalog edited)
    {
        var saves = new SuperMetroidSaveRam(bus);
        var data = new SuperMetroidSaveSnapshot { Area = (ushort)AreaId.Maridia, SaveStation = 0, Health = 99, MaxHealth = 99 };
        data.MapStationBytes[(int)AreaId.Maridia] = 1;
        data.UsedSaveStationBytes[(int)AreaId.Maridia * 2] = 1;
        saves.SaveSlot(0, data);
        var slot = saves.ReadSlot(0)!;
        var control = new FileSelectMapMenuState(bus, new CartridgeAudioState(), slot, 0, original);
        var changed = new FileSelectMapMenuState(guard, new CartridgeAudioState(), slot, 0, edited);
        for (int tick = 0; tick < 104; tick++)
        {
            ushort input = tick == 48 ? (ushort)SnesButton.Start : (ushort)0;
            control.Step(input); changed.Step(input);
            AssertEqual(control.Phase, changed.Phase, "layout replacement never changes menu transition timing");
        }
        AssertTrue(!control.Render().AsSpan().SequenceEqual(changed.Render()), "normal installed menu displays edited room frame");
        using var state = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(state, changed); state.Position = 0;
        changed = DebuggerObjectGraphSerializer.Deserialize<FileSelectMapMenuState>(state);
        changed.BindMapPresentation(original);
        AssertTrue(control.Render().AsSpan().SequenceEqual(changed.Render()), "whole restored menu rebinds current frame pixels");
        for (int tick = 0; tick < 80; tick++)
        {
            ushort input = tick == 0 ? (ushort)SnesButton.Start : (ushort)0;
            control.Step(input); changed.Step(input);
            AssertEqual(control.Phase, changed.Phase, "restored menu retains load phase");
            AssertEqual(control.LoadRequested, changed.LoadRequested, "restored menu retains load handoff timing");
            AssertTrue(control.Render().AsSpan().SequenceEqual(changed.Render()), "restored menu retains exact load fade pixels");
        }
        AssertTrue(changed.LoadRequested, "screen fixture reaches load handoff");
        AssertTrue(saves.ReadSlot(0)!.ToSnapshot().MapStationBytes.AsSpan().SequenceEqual(data.MapStationBytes), "screen edits preserve saved map progression");
    }

    private sealed class MapScreenReadGuard : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace source;
        private readonly HashSet<int> forbidden = new();
        public MapScreenReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            Add(FileSelectMapRomData.InitialMenuBackground, MapScreenDefinitions.PageBytes);
            Add(FileSelectMapRomData.AreaForeground, MapScreenDefinitions.PageBytes);
            Add(FileSelectMapRomData.AreaBackgrounds, MapScreenDefinitions.PageBytes * MapScreenDefinitions.ZebesAreas);
            Add(WorldMapArtworkFormat.ForegroundSource, WorldMapArtworkFormat.ForegroundBytes);
            Add(WorldMapArtworkFormat.BackgroundSource, WorldMapArtworkFormat.BackgroundBytes);
            Add(FileSelectMapRomData.RoomFrame, 1600);
            Add(FileSelectMapRomData.RoomFrameFooter, 322);
            Add(FileSelectMapRomData.RoomLabelPointers, MapScreenDefinitions.ZebesAreas * 2);
            for (int area = 0; area < MapScreenDefinitions.ZebesAreas; area++)
                Add(FileSelectMapRomData.MenuObjectBank | RomDataReader.ReadWordFixedBank(source, FileSelectMapRomData.RoomLabelPointers + area * 2), 24);
            void Add(int start, int length) { for (int offset = 0; offset < length; offset++) forbidden.Add(start + offset); }
        }
        public byte ReadByte(int address) => forbidden.Contains(address)
            ? throw new InvalidOperationException($"Installed map screens read cartridge presentation at {address:X6}.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
