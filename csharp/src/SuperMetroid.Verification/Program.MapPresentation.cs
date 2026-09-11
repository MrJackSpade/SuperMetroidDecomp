using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMapPresentation()
    {
        var rules = new PresentationMapRules();
        var cell = new MapPresentationCell { TileColumn = 17, TileRow = 2, Palette = 3,
            Priority = true, FlipX = true, FlipY = false };
        var document = new MapPresentationDocument { Version = 1, Area = "Crateria",
            Cells = Enumerable.Repeat(cell, 64 * 32).ToArray() };
        string Json(MapPresentationDocument value) => JsonSerializer.Serialize(value,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AreaMapPresentationAsset Load(string text) => AreaMapPresentationAsset.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(text)), rules);
        var map = Load(Json(document));
        AssertEqual((ushort)0x6c51, map.GetTile(5, 6).Raw, "map JSON compiles only presentation attributes");
        AssertTrue(map.RevealsCellAbove(5, 6), "replaced slope art retains its compiled reveal rule");
        AssertTrue(!map.IsDiscoverable(4, 6), "new artwork cannot create exploration cells");

        var system = new Bank80SystemState();
        byte[] projected = AreaMapTilemapBuilder.Build(map, system, MapTileWords.PauseBlank, MapRevealMode.Secret);
        ushort Word(int x, int y) => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(
            projected.AsSpan(AreaMapLayout.GetTilemapWordIndex(x, y) * 2));
        AssertEqual(map.GetTile(5, 6).Raw, Word(5, 6), "shared pause/file-select projection uses editable art");
        AssertEqual(MapTileWords.PauseBlank.Raw, Word(4, 6), "secret reveal uses rules, not painted cells");
        AssertTrue(!system.IsMapTileExplored(AreaId.Crateria, 5, 6), "projection never mutates exploration");

        var hud = new HudState();
        hud.Initialize(new TestAddressSpace(), HudSnapshot.CeresDebug);
        hud.UpdateMinimap(new ForbiddenMapBus(), system, AreaId.Crateria, 5, 5, 16, 16, 128, 128, 8,
            MapRevealMode.Secret, map);
        AssertEqual(map.GetTile(5, 6).ForHud(true).Raw, hud.Tiles[60], "live minimap consumes edited cells without cartridge reads");
        AssertTrue(system.IsMapTileExplored(AreaId.Crateria, 5, 5), "art replacement preserves slope corner exploration");

        var edited = document with { Cells = document.Cells.Select(c => c with { TileColumn = 19 }).ToArray() };
        var reloaded = Load(Json(edited));
        hud.UpdateMinimap(new ForbiddenMapBus(), system, AreaId.Crateria, 5, 5, 16, 16, 128, 128, 8,
            MapRevealMode.Secret, reloaded);
        AssertEqual(reloaded.GetTile(5, 6).ForHud(true).Raw, hud.Tiles[60], "reloaded edit changes live minimap without ROM patch");
        AssertThrows<InvalidDataException>(() => Load(Json(document with { Version = 2 })), "reject unsupported map schema");
        AssertThrows<InvalidDataException>(() => Load(Json(document with { Area = "Norfair" })), "reject wrong area");
        AssertThrows<InvalidDataException>(() => Load(Json(document with { Cells = [cell] })), "reject incomplete layout");
        AssertThrows<InvalidDataException>(() => Load(Json(document).Replace("\"tileColumn\":17", "\"tileColumn\":32")), "reject invalid atlas cell");
        AssertThrows<InvalidDataException>(() => Load(Json(document).Replace("\"version\":1", "\"version\":1,\"revealEverything\":true")), "reject editable gameplay commands");
        if (File.Exists("Super Metroid.smc"))
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
            foreach (AreaId area in Enum.GetValues<AreaId>())
            {
                var stock = AreaMapRomData.Load(bus, area);
                using var json = new MemoryStream();
                AreaMapPresentationAsset.Write(json, stock);
                json.Position = 0;
                var restored = AreaMapPresentationAsset.Load(json, stock);
                for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
                for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
                {
                    AssertEqual(stock.GetTile(x, y).Raw, restored.GetTile(x, y).Raw, "stock map JSON retains tile/attributes");
                    AssertEqual(stock.IsDiscoverable(x, y), restored.IsDiscoverable(x, y), "stock discovery rules survive import");
                    AssertEqual(stock.RevealsCellAbove(x, y), restored.RevealsCellAbove(x, y), "stock slope rules survive import");
                }
            }
            Console.WriteLine("Map presentation: all 14,336 retail cell words round-trip exactly.");
            VerifyMapPresentationCatalog(bus);
        }
        else Console.WriteLine("Map presentation: retail round-trip skipped; source ROM unavailable.");
        Console.WriteLine("Map presentation: JSON edits reach minimap/shared projection; independent exploration and strict validation pass.");
    }

    private sealed class PresentationMapRules : IAreaMapView
    {
        public AreaId Area => AreaId.Crateria;
        public MapTileWord GetTile(int x, int y) => MapTileWords.PauseBlank;
        public bool IsDiscoverable(int x, int y) => x == 5 && y is 5 or 6;
        public bool IsRevealedByMapStation(int x, int y) => x == 5 && y == 6;
        public bool RevealsCellAbove(int x, int y) => x == 5 && y == 6;
    }

    private sealed class ForbiddenMapBus : ISnesAddressSpace
    {
        public byte ReadByte(int address) => throw new InvalidOperationException($"Unexpected map ROM read {address:X6}.");
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected map bus write.");
    }

    private static void VerifyMapPresentationCatalog(ISnesAddressSpace bus)
    {
        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp", "map-catalog-" + Guid.NewGuid().ToString("N")));
        string stock = Path.Combine(root, "game", "maps"), repaired = Path.Combine(root, "replacement-stock"), overrides = Path.Combine(root, "overrides", "maps");
        var rules = Enum.GetValues<AreaId>().ToDictionary(area => area, area => AreaMapRomData.Load(bus, area));
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, stock, "test-provenance");
        IAreaMapView Rules(AreaId area) => rules[area];
        var original = AreaMapPresentationCatalog.Load(stock, overrides, Rules);
        var reopened = AreaMapPresentationCatalog.Load(stock, overrides, Rules);
        AssertEqual(original.ContentIdentity, reopened.ContentIdentity, "map catalog identity stable across reload");
        Directory.CreateDirectory(overrides);
        string name = AreaMapCatalogFormat.FileName(AreaId.Crateria);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<MapPresentationDocument>(File.ReadAllText(Path.Combine(stock, name)), options)!;
        document.Cells[0] = document.Cells[0] with { TileColumn = (document.Cells[0].TileColumn + 1) % MapPresentationFormat.AtlasColumns };
        string replacement = Path.Combine(overrides, name);
        File.WriteAllText(replacement, JsonSerializer.Serialize(document, options));
        byte[] editedBytes = File.ReadAllBytes(replacement);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides, Rules);
        VerifyLiveMapCatalog(bus, original, edited, rules.Values.ToArray());
        AssertTrue(edited.ContentIdentity != original.ContentIdentity, "map override changes content identity");
        AssertTrue(edited.Get(AreaId.Crateria).GetTile(0, 0) != original.Get(AreaId.Crateria).GetTile(0, 0), "catalog prefers valid override");
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, repaired, "test-provenance");
        var afterRepair = AreaMapPresentationCatalog.Load(repaired, overrides, Rules);
        AssertEqual(edited.ContentIdentity, afterRepair.ContentIdentity, "re-extracted stock preserves selected override");
        AssertTrue(editedBytes.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)), "stock extraction and reload never rewrite override bytes");
        AssertThrows<IOException>(() => SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, stock, "test-provenance"), "stock importer refuses overwrite");
        File.WriteAllText(replacement, "{ broken JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides, Rules), "corrupt override fails instead of selecting stock");
        AssertEqual("{ broken JSON", File.ReadAllText(replacement), "invalid override retained for user repair");
        File.WriteAllBytes(replacement, editedBytes);
        File.WriteAllText(Path.Combine(stock, name), "corrupt stock");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides, Rules), "stock integrity failure remains visible with override present");
        Console.WriteLine("Map catalog: deterministic reload, override precedence/identity, stock replacement preservation and corruption errors pass.");
    }

    private static void VerifyLiveMapCatalog(ISnesAddressSpace bus, AreaMapPresentationCatalog original,
        AreaMapPresentationCatalog edited, AreaMapCartridgeData[] rules)
    {
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(bus);
        using var withoutContent = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(withoutContent, game);
        game.BindMapPresentation(original);
        using var withContent = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(withContent, game);
        AssertTrue(withoutContent.ToArray().AsSpan().SequenceEqual(withContent.ToArray()), "map resources do not enter debugger graph");
        withContent.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Frontend.SuperMetroidGame>(withContent);
        AssertTrue(restored.MapPresentationIdentity is null, "restored graph requires host content rebind");
        restored.BindMapPresentation(edited);
        AssertEqual(edited.ContentIdentity, restored.MapPresentationIdentity, "state rebind uses current override identity");

        var guard = new MapDataGuard(bus, rules);
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(guard) { MapPresentation = edited };
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        AssertEqual(edited.Get(AreaId.Ceres).GetTile(runtime.Hud.MinimapCenterX, runtime.Hud.MinimapCenterY).CharacterIndex,
            (ushort)(runtime.Hud.Tiles[60] & 0x3ff), "Ceres room-entry runtime consumes bound catalog without map ROM reads");
        var room = runtime.ActiveRoom!;
        var pause = new SuperMetroid.Core.Frontend.PauseMenuState(guard, runtime.Samus!, runtime.System,
            room.AreaIndex, room.MapX, room.MapY, mapPresentation: edited);
        ushort x = pause.MapHorizontalScroll, y = pause.MapVerticalScroll;
        pause.BindMapPresentation(original);
        AssertEqual(x, pause.MapHorizontalScroll, "pause content rebind retains horizontal scroll");
        AssertEqual(y, pause.MapVerticalScroll, "pause content rebind retains vertical scroll");
        _ = pause.Render();
        Console.WriteLine("Live map catalog: Ceres room-entry/pause reject map ROM access; debugger rebind excludes stale content and preserves scroll.");
    }

    private sealed class MapDataGuard(ISnesAddressSpace source, AreaMapCartridgeData[] maps) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if ((address >= AreaMapRomData.TilemapPointerTable && address < AreaMapRomData.TilemapPointerTable + AreaIds.RetailCount * 3) ||
                (address >= AreaMapRomData.StationRevealMaskPointerTable && address < AreaMapRomData.StationRevealMaskPointerTable + AreaIds.RetailCount * 2) ||
                maps.Any(map => (address >= map.TilemapAddress && address < map.TilemapAddress + AreaMapRomData.TilemapByteCount) ||
                    (address >= map.StationRevealMaskAddress && address < map.StationRevealMaskAddress + AreaMapRomData.StationRevealMaskByteCount)))
                throw new InvalidOperationException($"Live presentation read map ROM at {address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
