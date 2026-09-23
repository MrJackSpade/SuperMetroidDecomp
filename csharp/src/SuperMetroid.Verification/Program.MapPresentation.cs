using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMapPresentation()
    {
        VerifyIndexedPng();
        VerifyQueuedVramAssets();
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
        var original = new SuperMetroid.AssetExtraction.GameInstallation(root).LoadMaps();
        var reopened = AreaMapPresentationCatalog.Load(stock, overrides);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            var actual = original.Get(area);
            AssertEqual(rules[area].GetTile(x, y), actual.GetTile(x, y), "ROM-free catalog retains stock cells");
            AssertEqual(rules[area].IsDiscoverable(x, y), actual.IsDiscoverable(x, y), "ROM-free stock discovery parity");
            AssertEqual(rules[area].IsRevealedByMapStation(x, y), actual.IsRevealedByMapStation(x, y), "ROM-free station mask parity");
            AssertEqual(rules[area].RevealsCellAbove(x, y), actual.RevealsCellAbove(x, y), "ROM-free stock slope parity");
        }
        AssertEqual(original.ContentIdentity, reopened.ContentIdentity, "map catalog identity stable across reload");
        Directory.CreateDirectory(overrides);
        VerifyTitleGraphicsOverride(bus, stock, overrides, original);
        VerifyTitlePaletteOverride(bus, stock, overrides, original);
        VerifyTitleGradientOverride(bus, stock, overrides, original);
        VerifyRoomPaletteFxOverride(bus, stock, overrides, original);
        VerifyRoomFxAnimatedTileArtworkOverride(bus, stock, overrides, original);
        VerifyRoomFxLayer3TilemapOverride(stock, overrides, original);
        VerifyRoomFxPaletteBlendOverride(stock, overrides, original);
        VerifyPowerBombFixedColorOverride(stock, overrides, original);
        VerifySamusVisorColorOverride(stock, overrides, original, bus);
        VerifySamusHurtColorOverride(stock, overrides, original, bus);
        VerifyMotherBrainHealthPaletteOverride(bus, stock, overrides, original);
        VerifyMotherBrainRainbowPaletteOverride(bus, stock, overrides, original);
        string name = AreaMapCatalogFormat.FileName(AreaId.Crateria);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<MapPresentationDocument>(File.ReadAllText(Path.Combine(stock, name)), options)!;
        document.Cells[0] = document.Cells[0] with { TileColumn = (document.Cells[0].TileColumn + 1) % MapPresentationFormat.AtlasColumns };
        int visibleCell = Enumerable.Range(0, document.Cells.Length).First(i => rules[AreaId.Crateria].IsDiscoverable(i % 64, i / 64));
        document.Cells[visibleCell] = document.Cells[visibleCell] with { Palette = (document.Cells[visibleCell].Palette + 1) % 8 };
        string replacement = Path.Combine(overrides, name);
        File.WriteAllText(replacement, JsonSerializer.Serialize(document, options));
        byte[] editedBytes = File.ReadAllBytes(replacement);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            AssertEqual(original.Get(area).IsDiscoverable(x, y), edited.Get(area).IsDiscoverable(x, y), "override cannot alter discovery");
            AssertEqual(original.Get(area).IsRevealedByMapStation(x, y), edited.Get(area).IsRevealedByMapStation(x, y), "override cannot alter station reveal");
            AssertEqual(original.Get(area).RevealsCellAbove(x, y), edited.Get(area).RevealsCellAbove(x, y), "override cannot alter slope exploration");
        }
        VerifyLiveMapCatalog(bus, original, edited, rules.Values.ToArray());
        AssertTrue(edited.ContentIdentity != original.ContentIdentity, "map override changes content identity");
        AssertTrue(edited.Get(AreaId.Crateria).GetTile(0, 0) != original.Get(AreaId.Crateria).GetTile(0, 0), "catalog prefers valid override");
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, repaired, "test-provenance");
        var afterRepair = AreaMapPresentationCatalog.Load(repaired, overrides);
        AssertEqual(edited.ContentIdentity, afterRepair.ContentIdentity, "re-extracted stock preserves selected override");
        AssertTrue(editedBytes.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)), "stock extraction and reload never rewrite override bytes");
        VerifyBundledMapMaskValidation(repaired);
        VerifyMapAtlasIntegration(bus, stock, Path.Combine(root, "atlas-overrides"), original, rules.Values.ToArray());
        VerifyHudAtlasIntegration(bus, stock, Path.Combine(root, "hud-overrides"), original, rules.Values.ToArray());
        VerifyMapPaletteCycleIntegration(bus, stock, Path.Combine(root, "cycle-overrides"), original, rules.Values.ToArray());
        VerifyMapStaticPaletteIntegration(bus, stock, Path.Combine(root, "palette-overrides"), original, rules.Values.ToArray());
        VerifyWorldMapLabels(bus, stock, Path.Combine(root, "label-overrides"), original);
        VerifyMapStationLayout(bus, stock, Path.Combine(root, "station-overrides"), original);
        VerifyMapLandmarks(bus, stock, Path.Combine(root, "landmark-overrides"), original);
        VerifyMapSaveMarkers(bus, stock, Path.Combine(root, "save-marker-overrides"), original);
        VerifyMapArrows(bus, stock, Path.Combine(root, "arrow-overrides"), original);
        VerifyMapScreens(bus, stock, Path.Combine(root, "screen-overrides"), original);
        VerifyMapSprites(bus, stock, Path.Combine(root, "sprite-overrides"), original);
        VerifyPauseTileAtlas(bus, stock, Path.Combine(root, "pause-art-overrides"), original);
        VerifyCompiledPauseEquipmentRules(bus, original);
        VerifyPauseBackdrops(bus, stock, Path.Combine(root, "pause-backdrop-overrides"), original);
        VerifyPauseWireframes(bus, stock, Path.Combine(root, "pause-wireframe-overrides"), original);
        VerifyPauseSelectors(bus, stock, Path.Combine(root, "pause-selector-overrides"), original);
        VerifyPauseReserveTankAssets(bus, stock, Path.Combine(root, "pause-reserve-tank-overrides"), original);
        VerifyPauseReserveUiAssets(bus, stock, Path.Combine(root, "pause-reserve-ui-overrides"), original);
        VerifyPauseEquipmentBaseAssets(bus, stock, Path.Combine(root, "pause-equipment-base-overrides"), original);
        VerifyPauseEquipmentLabelAssets(bus, stock, Path.Combine(root, "pause-equipment-label-overrides"), original);
        VerifyEscapeTimerPresentationAssets(bus, stock, Path.Combine(root, "escape-timer-overrides"), original);
        VerifyGameplayHudPresentationAssets(bus, stock, Path.Combine(root, "gameplay-hud-overrides"), original);
        VerifyGameOverPresentationAssets(bus, stock, Path.Combine(root, "game-over-overrides"), original);
        VerifyGameOptionsPresentationAssets(bus, stock, Path.Combine(root, "game-options-overrides"), original);
        VerifyFileSelectPresentationAssets(bus, stock, Path.Combine(root, "file-select-overrides"), original);
        VerifyGameplayMessagePanelAssets(bus, stock, Path.Combine(root, "gameplay-message-panel-overrides"), original);
        VerifyGameplayMessageNoticeAssets(bus, stock, Path.Combine(root, "gameplay-message-notice-overrides"), original);
        VerifyEscapeTypewriterAssets(bus, stock, Path.Combine(root, "escape-typewriter-overrides"), original);
        VerifyIntroNarrationAssets(bus, stock, Path.Combine(root, "intro-narration-overrides"), original);
        VerifyIntroFontAssets(bus, stock, Path.Combine(root, "intro-font-overrides"), original);
        VerifyEndingTextAssets(bus, stock, Path.Combine(root, "ending-text-overrides"), original);
        VerifyEndingFontAssets(bus, stock, Path.Combine(root, "ending-font-overrides"), original);
        VerifyStaffCreditsAssets(stock, Path.Combine(root, "staff-credits-overrides"), original);
        // Stronger than composing individual range guards: no bus read or write
        // is permitted anywhere in this complete installed saved-map lifecycle.
        VerifyInstalledFileSelectMenu(bus, new ForbiddenMapBus(), original, original, verifyCapturedRendering: true);
        VerifyMapLoadAnchors(bus, original);
        VerifyCompiledMapScrollControls(bus, original);
        AssertThrows<IOException>(() => SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, stock, "test-provenance"), "stock importer refuses overwrite");
        File.WriteAllText(replacement, "{ broken JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "corrupt override fails instead of selecting stock");
        AssertEqual("{ broken JSON", File.ReadAllText(replacement), "invalid override retained for user repair");
        File.WriteAllBytes(replacement, editedBytes);
        File.WriteAllText(Path.Combine(stock, name), "corrupt stock");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "stock integrity failure remains visible with override present");
        Console.WriteLine("Map catalog: deterministic reload, override precedence/identity, stock replacement preservation and corruption errors pass.");
    }

    private sealed class PaletteReadForbiddenBus : ISnesAddressSpace
    {
        public Dictionary<int, byte> Writes { get; } = new();
        public byte ReadByte(int address) => throw new InvalidOperationException(
            $"Installed Mother Brain palette unexpectedly read cartridge address ${address:X6}.");
        public void WriteByte(int address, byte value)
        {
            if (address is not (MotherBrainDrainedPaletteRomData.TrailingWordWram or
                MotherBrainDrainedPaletteRomData.TrailingWordWram + 1))
                throw new InvalidOperationException($"Unexpected Mother Brain palette write ${address:X6}.");
            Writes[address] = value;
        }
    }

    private static void VerifyMotherBrainRainbowPaletteOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string source = Path.Combine(stock, MotherBrainRainbowPaletteFormat.FileName);
        MotherBrainRainbowPaletteDocument document =
            JsonSerializer.Deserialize<MotherBrainRainbowPaletteDocument>(
                File.ReadAllBytes(source), MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted Mother Brain rainbow palette is null.");
        for (int frame = 0; frame < MotherBrainRainbowPaletteFormat.RainbowFrameCount; frame++)
        {
            var native = new SnesCgram();
            var installed = new SnesCgram();
            int pointer = MotherBrainRainbowPaletteRomData.PointerTable + frame * sizeof(ushort);
            int palette = MotherBrainRainbowPaletteRomData.SourceBank |
                bus.ReadByte(pointer) | bus.ReadByte(pointer + 1) << 8;
            native.LoadFromBus(bus, palette, MotherBrainRainbowPaletteRomData.ColorCount,
                MotherBrainRainbowPaletteRomData.BodyColor);
            native.LoadFromBus(bus, palette, MotherBrainRainbowPaletteRomData.ColorCount,
                MotherBrainRainbowPaletteRomData.BrainColor);
            native.LoadFromBus(bus, palette + MotherBrainRainbowPaletteRomData.ColorCount * sizeof(ushort),
                MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.SecondaryColor);
            original.MotherBrainRainbowPalette.ApplyRainbow(installed, frame);
            AssertTrue(native.Colors.SequenceEqual(installed.Colors),
                $"Mother Brain rainbow frame {frame} matches all native CGRAM colors");
        }

        foreach (bool draining in new[] { true, false })
        for (int frame = 0; frame < MotherBrainRainbowPaletteFormat.GreyFrameCount; frame++)
        {
            var native = new SnesCgram();
            var installed = new SnesCgram();
            // Revival writes thirteen colors and deliberately leaves the last two from
            // the preceding phase. Seed both CGRAM images to catch an overlong copy.
            native.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + 13, 0x1234);
            native.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + 14, 0x1235);
            native.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + 13, 0x2234);
            native.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + 14, 0x2235);
            installed.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + 13, 0x1234);
            installed.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + 14, 0x1235);
            installed.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + 13, 0x2234);
            installed.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + 14, 0x2235);
            int table = draining ? MotherBrainDrainedPaletteRomData.ToGreyTable :
                MotherBrainDrainedPaletteRomData.FromGreyTable;
            int count = draining ? MotherBrainDrainedPaletteRomData.DrainedColors :
                MotherBrainDrainedPaletteRomData.RevivalColors;
            int pointer = table + frame * sizeof(ushort);
            int palette = MotherBrainRainbowPaletteRomData.SourceBank |
                bus.ReadByte(pointer) | bus.ReadByte(pointer + 1) << 8;
            native.LoadFromBus(bus, palette, count, MotherBrainRainbowPaletteRomData.BodyColor);
            native.LoadFromBus(bus, palette, count, MotherBrainRainbowPaletteRomData.BrainColor);
            native.LoadFromBus(bus, palette + count * sizeof(ushort),
                MotherBrainDrainedPaletteRomData.BackLegCount,
                MotherBrainDrainedPaletteRomData.BackLegColor);
            var guard = new PaletteReadForbiddenBus();
            if (draining) original.MotherBrainRainbowPalette.ApplyToGrey(guard, installed, frame);
            else original.MotherBrainRainbowPalette.ApplyFromGrey(guard, installed, frame);
            AssertTrue(native.Colors.SequenceEqual(installed.Colors),
                $"Mother Brain {(draining ? "drain" : "revival")} frame {frame} retains exact native CGRAM and preserved colors");
            int tail = palette + (count + MotherBrainDrainedPaletteRomData.BackLegCount) * sizeof(ushort);
            AssertEqual(bus.ReadByte(tail), guard.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram],
                "grey transition publishes native trailing WRAM low byte");
            AssertEqual(bus.ReadByte(tail + 1), guard.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram + 1],
                "grey transition publishes native trailing WRAM high byte");
            AssertEqual(2, guard.Writes.Count, "grey transition writes only its two native WRAM bytes");
        }

        var nativeNormal = new SnesCgram();
        var installedNormal = new SnesCgram();
        nativeNormal.LoadFromBus(bus, MotherBrainRainbowPaletteRomData.NormalBrainSource,
            MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.BodyColor);
        nativeNormal.LoadFromBus(bus, MotherBrainRainbowPaletteRomData.NormalBrainSource,
            MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.BrainColor);
        nativeNormal.LoadFromBus(bus, MotherBrainRainbowPaletteRomData.NormalSecondarySource,
            MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.SecondaryColor);
        original.MotherBrainRainbowPalette.ApplyNormal(installedNormal);
        AssertTrue(nativeNormal.Colors.SequenceEqual(installedNormal.Colors),
            "Mother Brain normal restoration matches both fixed native sources");

        PaletteRgb5 beam = document.Rainbow[0].Body[0];
        document.Rainbow[0].Body[0] = beam with { Red = beam.Red == 31 ? 30 : beam.Red + 1 };
        PaletteRgb5 drain = document.ToGrey[0].Body[0];
        document.ToGrey[0].Body[0] = drain with { Blue = drain.Blue == 31 ? 30 : drain.Blue + 1 };
        PaletteRgb5 authoredTail = document.ToGrey[0].TrailingColor!;
        document.ToGrey[0] = document.ToGrey[0] with
        {
            TrailingColor = authoredTail with
            {
                Green = authoredTail.Green == 31 ? 30 : authoredTail.Green + 1,
            },
        };
        PaletteRgb5 revive = document.FromGrey[0].BackLegs[0];
        document.FromGrey[0].BackLegs[0] = revive with { Green = revive.Green == 31 ? 30 : revive.Green + 1 };
        PaletteRgb5 normal = document.Normal.Body[0];
        document.Normal.Body[0] = normal with { Red = normal.Red == 31 ? 30 : normal.Red + 1 };
        string replacement = Path.Combine(overrides, MotherBrainRainbowPaletteFormat.FileName);
        using (var stream = File.Create(replacement))
            MotherBrainRainbowPalettePresentation.Write(stream, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Mother Brain rainbow edit changes installed-content identity");
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus)
        {
            MapPresentation = edited,
        };
        var stockOutput = new SnesCgram();
        var editedOutput = new SnesCgram();
        original.MotherBrainRainbowPalette.ApplyRainbow(stockOutput, 0);
        runtime.Enemies.MotherBrainRainbowColors!.ApplyRainbow(editedOutput, 0);
        AssertTrue(stockOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor] !=
            editedOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor],
            "installed rainbow override reaches the room-enemy binding");
        stockOutput.Clear();
        editedOutput.Clear();
        var stockTailBus = new PaletteReadForbiddenBus();
        var editedTailBus = new PaletteReadForbiddenBus();
        original.MotherBrainRainbowPalette.ApplyToGrey(stockTailBus, stockOutput, 0);
        runtime.Enemies.MotherBrainRainbowColors.ApplyToGrey(editedTailBus, editedOutput, 0);
        AssertTrue(stockOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor] !=
            editedOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor],
            "drain override reaches the native body destination");
        AssertTrue(stockTailBus.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram] !=
            editedTailBus.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram] ||
            stockTailBus.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram + 1] !=
            editedTailBus.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram + 1],
            "drain trailing-color override reaches native WRAM publication");
        stockOutput.Clear();
        editedOutput.Clear();
        original.MotherBrainRainbowPalette.ApplyFromGrey(new PaletteReadForbiddenBus(), stockOutput, 0);
        runtime.Enemies.MotherBrainRainbowColors.ApplyFromGrey(new PaletteReadForbiddenBus(), editedOutput, 0);
        AssertTrue(stockOutput.Colors[MotherBrainDrainedPaletteRomData.BackLegColor] !=
            editedOutput.Colors[MotherBrainDrainedPaletteRomData.BackLegColor],
            "revival override reaches the native rear-leg destination");
        stockOutput.Clear();
        editedOutput.Clear();
        original.MotherBrainRainbowPalette.ApplyNormal(stockOutput);
        runtime.Enemies.MotherBrainRainbowColors.ApplyNormal(editedOutput);
        AssertTrue(stockOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor] !=
            editedOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor],
            "normal-restoration override reaches the native body destination");

        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Rainbow = document.Rainbow.Take(9).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject truncated Mother Brain rainbow loop");
        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                ToGrey = document.ToGrey.Select((item, index) => index == 0 ?
                    item with { TrailingColor = null } : item).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject missing Mother Brain grey trailing color");
        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Normal = document.Normal with { Body = document.Normal.Body.Take(14).ToArray() },
            }, MapPresentationFormat.JsonOptions))), "reject truncated Mother Brain normal palette");
        File.Delete(replacement);
        AssertEqual(original.ContentIdentity, AreaMapPresentationCatalog.Load(stock, overrides).ContentIdentity,
            "removing Mother Brain rainbow override restores installed-content identity");
        Console.WriteLine("Mother Brain rainbow palette: 10 beam, 16 grey, and normal native outputs; ROM-free copies/WRAM tail, five live edits and strict validation pass.");
    }

    private static void VerifyMotherBrainHealthPaletteOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string source = Path.Combine(stock, MotherBrainHealthPaletteFormat.FileName);
        MotherBrainHealthPaletteDocument document =
            JsonSerializer.Deserialize<MotherBrainHealthPaletteDocument>(
                File.ReadAllBytes(source), MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted Mother Brain health palette is null.");
        foreach (var (health, state) in new (ushort, int)[]
            { (36000, 0), (9000, 0), (8999, 1), (5400, 1), (5399, 2), (1800, 2), (1799, 3), (0, 3) })
        {
            var native = new SnesCgram();
            var installed = new SnesCgram();
            MotherBrainHealthPalette.Apply(bus, native, health);
            MotherBrainHealthPalette.Apply(new ForbiddenMapBus(), installed, health,
                original.MotherBrainHealthPalette);
            AssertTrue(native.Colors.SequenceEqual(installed.Colors),
                $"Mother Brain health {health} stock colors exactly match the native three-copy output");
            AssertEqual((ushort)0, installed.Colors[MotherBrainRainbowPaletteRomData.BrainColor - 1],
                "Mother Brain health palette leaves transparent color untouched");
            AssertEqual((ushort)0, installed.Colors[MotherBrainRainbowPaletteRomData.SecondaryColor +
                MotherBrainRainbowPaletteRomData.ColorCount],
                "Mother Brain health palette leaves adjacent colors untouched");
            AssertEqual(state, health >= MotherBrainHealthPaletteRomData.FirstThreshold ? 0 :
                health >= MotherBrainHealthPaletteRomData.SecondThreshold ? 1 :
                health >= MotherBrainHealthPaletteRomData.FinalThreshold ? 2 : 3,
                "native health thresholds still choose the four damage states");
        }

        PaletteRgb5 body = document.Body[2][0];
        document.Body[2][0] = body with { Red = body.Red == 31 ? 30 : body.Red + 1 };
        PaletteRgb5 leg = document.BackLegs[2][0];
        document.BackLegs[2][0] = leg with { Blue = leg.Blue == 31 ? 30 : leg.Blue + 1 };
        string replacement = Path.Combine(overrides, MotherBrainHealthPaletteFormat.FileName);
        using (var stream = File.Create(replacement))
            MotherBrainHealthPalettePresentation.Write(stream, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Mother Brain health palette override changes installed-content identity");
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus)
        {
            MapPresentation = edited,
        };
        var output = new SnesCgram();
        var stockOutput = new SnesCgram();
        MotherBrainHealthPalette.Apply(new ForbiddenMapBus(), stockOutput, 5399,
            original.MotherBrainHealthPalette);
        MotherBrainHealthPalette.Apply(new ForbiddenMapBus(), output, 5399,
            runtime.Enemies.MotherBrainHealthColors);
        AssertTrue(output.Colors[MotherBrainRainbowPaletteRomData.BodyColor] !=
            stockOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor],
            "installed Mother Brain body edit reaches the live enemy binding");
        AssertTrue(output.Colors[MotherBrainRainbowPaletteRomData.SecondaryColor] !=
            stockOutput.Colors[MotherBrainRainbowPaletteRomData.SecondaryColor],
            "installed Mother Brain leg edit reaches the live enemy binding");
        AssertEqual(output.Colors[MotherBrainRainbowPaletteRomData.BodyColor],
            output.Colors[MotherBrainRainbowPaletteRomData.BrainColor],
            "body and brain still share the native palette");

        AssertThrows<InvalidDataException>(() => MotherBrainHealthPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Version = MotherBrainHealthPaletteFormat.Version + 1,
            }, MapPresentationFormat.JsonOptions))), "reject unsupported Mother Brain palette version");
        AssertThrows<InvalidDataException>(() => MotherBrainHealthPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Body = document.Body.Take(3).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject incomplete Mother Brain damage states");
        AssertThrows<InvalidDataException>(() => MotherBrainHealthPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                BackLegs = document.BackLegs.Select((colors, index) => index == 0 ?
                    colors.Take(14).ToArray() : colors).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject truncated Mother Brain rear-leg palette");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing Mother Brain health palette override restores installed-content identity");
        Console.WriteLine("Mother Brain health palette: 8 threshold boundaries, ROM-free stock copies, live body/leg edits, schema checks and content identity pass.");
    }

    private static void VerifyRoomPaletteFxOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string source = Path.Combine(stock, RoomPaletteFxPresentationFormat.FileName);
        RoomPaletteFxPresentationDocument document =
            JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(
                File.ReadAllBytes(source),
                MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted room palette-FX document is null.");
        PaletteRgb5 color = document.NorfairForegroundAndHeatPhase[0][0];
        document.NorfairForegroundAndHeatPhase[0][0] = color with
        {
            Red = color.Red == 31 ? 30 : color.Red + 1,
        };
        PaletteRgb5 waterfall = document.MaridiaBackgroundWaterfalls[0][0];
        document.MaridiaBackgroundWaterfalls[0][0] = waterfall with
        {
            Blue = waterfall.Blue == 31 ? 30 : waterfall.Blue + 1,
        };
        PaletteRgb5 greenLight = document.WreckedShipGreenLights[0][0];
        document.WreckedShipGreenLights[0][0] = greenLight with
        {
            Green = greenLight.Green == 31 ? 30 : greenLight.Green + 1,
        };
        PaletteRgb5 redBrinstar = document.RedBrinstarBackgroundGlow[0][0];
        document.RedBrinstarBackgroundGlow[0][0] = redBrinstar with
        {
            Red = redBrinstar.Red == 31 ? 30 : redBrinstar.Red + 1,
        };
        PaletteRgb5 tourian = document.TourianGlow[0][0];
        document.TourianGlow[0][0] = tourian with
        {
            Blue = tourian.Blue == 31 ? 30 : tourian.Blue + 1,
        };
        PaletteRgb5 blueSpore = document.BrinstarBlueSpores[0][0];
        document.BrinstarBlueSpores[0][0] = blueSpore with
        {
            Green = blueSpore.Green == 31 ? 30 : blueSpore.Green + 1,
        };
        PaletteRgb5 bombTorizo = document.BombTorizoBelly[0][0];
        document.BombTorizoBelly[0][0] = bombTorizo with
        {
            Red = bombTorizo.Red == 31 ? 30 : bombTorizo.Red + 1,
        };
        PaletteRgb5 goldenTorizo = document.GoldenTorizoBelly[0][0];
        document.GoldenTorizoBelly[0][0] = goldenTorizo with
        {
            Blue = goldenTorizo.Blue == 31 ? 30 : goldenTorizo.Blue + 1,
        };
        PaletteRgb5 statueGrey = document.TourianStatueGrey[0][0];
        document.TourianStatueGrey[0][0] = statueGrey with
        {
            Green = statueGrey.Green == 31 ? 30 : statueGrey.Green + 1,
        };
        PaletteRgb5 surfaceLightning = document.CrateriaSurfaceLightning[0][0];
        document.CrateriaSurfaceLightning[0][0] = surfaceLightning with
        {
            Blue = surfaceLightning.Blue == 31 ? 30 : surfaceLightning.Blue + 1,
        };
        PaletteRgb5 darkLightning = document.CrateriaUnusedDarkLightning[0][0];
        document.CrateriaUnusedDarkLightning[0][0] = darkLightning with
        {
            Red = darkLightning.Red == 31 ? 30 : darkLightning.Red + 1,
        };
        PaletteRgb5 gunshipLight = document.CeresGunshipEngineLights[0][0];
        document.CeresGunshipEngineLights[0][0] = gunshipLight with
        {
            Green = gunshipLight.Green == 31 ? 30 : gunshipLight.Green + 1,
        };
        PaletteRgb5 navigationLight = document.CeresNavigationLights[0][0];
        document.CeresNavigationLights[0][0] = navigationLight with
        {
            Blue = navigationLight.Blue == 31 ? 30 : navigationLight.Blue + 1,
        };
        PaletteRgb5 fadeInColor = document.PlanetZebesTextFadeIn[0][0];
        document.PlanetZebesTextFadeIn[0][0] = fadeInColor with
        {
            Red = fadeInColor.Red == 31 ? 30 : fadeInColor.Red + 1,
        };
        PaletteRgb5 fadeOutColor = document.PlanetZebesTextFadeOut[0][0];
        document.PlanetZebesTextFadeOut[0][0] = fadeOutColor with
        {
            Green = fadeOutColor.Green == 31 ? 30 : fadeOutColor.Green + 1,
        };
        PaletteRgb5 motherBrainLight = document.OldMotherBrainBackgroundLights[0][0];
        document.OldMotherBrainBackgroundLights[0][0] = motherBrainLight with
        {
            Blue = motherBrainLight.Blue == 31 ? 30 : motherBrainLight.Blue + 1,
        };
        PaletteRgb5 gunshipGlow = document.CinematicGunshipGlow[0][0];
        document.CinematicGunshipGlow[0][0] = gunshipGlow with
        {
            Red = gunshipGlow.Red == 31 ? 30 : gunshipGlow.Red + 1,
        };
        PaletteRgb5 zebesFade = document.ExplodingZebesFade[0][0];
        document.ExplodingZebesFade[0][0] = zebesFade with
        {
            Green = zebesFade.Green == 31 ? 30 : zebesFade.Green + 1,
        };
        PaletteRgb5 unusedFade = document.UnusedCinematicFade[0][0];
        document.UnusedCinematicFade[0][0] = unusedFade with
        {
            Blue = unusedFade.Blue == 31 ? 30 : unusedFade.Blue + 1,
        };
        PaletteRgb5 titleLogo = document.TitleLogoFade[0][0];
        document.TitleLogoFade[0][0] = titleLogo with
        {
            Red = titleLogo.Red == 31 ? 30 : titleLogo.Red + 1,
        };
        PaletteRgb5 nintendoFade = document.NintendoSharedFade[0][0];
        document.NintendoSharedFade[0][0] = nintendoFade with
        {
            Green = nintendoFade.Green == 31 ? 30 : nintendoFade.Green + 1,
        };
        ChangeRed(document.ZebesExplosionForeground);
        ChangeRed(document.ZebesExplosionFinale);
        ChangeRed(document.ZebesExplosionWhiteout);
        ChangeRed(document.ZebesExplosionAfterglow);
        ChangeRed(document.ZebesExplosionLava);
        ChangeRed(document.ZebesExplosionCrust);
        ChangeRed(document.ZebesExplosionGreyClouds);
        ChangeRed(document.ZebesExplosionGunship);
        ChangeRed(document.SamusLoadingPowerSuit);
        ChangeRed(document.SamusLoadingVariaSuit);
        ChangeRed(document.SamusLoadingGravitySuit);
        ChangeRed(document.PostCreditsIconGlare);
        ChangeRed(document.TourianEscapeShutter);
        ChangeRed(document.TourianEscapeBackground);
        ChangeRed(document.TourianEscapeSharedRedFlash);
        ChangeRed(document.OldTourianEscapeRedFlash);
        ChangeRed(document.OldTourianEscapeOrangeRailings);
        ChangeRed(document.OldTourianEscapeYellowPanels);
        ChangeRed(document.UpperCrateriaEscapeRedFlash);
        ChangeRed(document.CrateriaEscapeYellowLightning);
        ChangeRed(document.CrateriaEscapeCreBlockPixel);
        ChangeRed(document.BeaconFlashing);

        string replacement = Path.Combine(overrides, RoomPaletteFxPresentationFormat.FileName);
        using (var stream = File.Create(replacement))
            RoomPaletteFxPresentation.Write(stream, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "room palette-FX override changes installed-content identity");

        NorfairEnvironmentalPaletteFxProgramDefinition definition =
            NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All.Single(item =>
                item.Owner == NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase);
        AssertOverride(definition.DefinitionPointer, definition.ColorByteIndex,
            "selected Norfair");

        MaridiaEnvironmentalPaletteFxProgramDefinition waterfallDefinition =
            MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All.Single(item =>
                item.Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls);
        AssertOverride(waterfallDefinition.DefinitionPointer,
            waterfallDefinition.ColorByteIndex, "selected Maridia");
        AssertOverride(
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.PoweredDefinition,
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Wrecked Ship");
        AssertOverride(
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Red Brinstar");
        AssertOverride(
            TourianGlowPaletteFxProgramMechanicsDefinitions.LiveDefinitionPointer,
            TourianGlowPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Tourian");
        BrinstarBlueSporePaletteFxProgramDefinition blueSporeDefinition =
            BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.All.Single(item =>
                item.Owner == BrinstarBlueSporePaletteOwner.StandardRooms);
        AssertOverride(blueSporeDefinition.DefinitionPointer,
            BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Brinstar blue-spore");
        foreach (TorizoBellyPaletteFxProgramDefinition torizoDefinition in
                 TorizoBellyPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(torizoDefinition.DefinitionPointer,
                TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
                $"{torizoDefinition.Owner} belly");
        }
        TourianStatueGreyPaletteFxProgramDefinition statueDefinition =
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.All[0];
        AssertOverride(statueDefinition.DefinitionPointer,
            statueDefinition.ColorByteIndex, "Tourian statue grey-out");
        foreach (CrateriaLightningPaletteFxProgramDefinition lightningDefinition in
                 CrateriaLightningPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(
                lightningDefinition.DefinitionPointer,
                lightningDefinition.ColorByteIndex,
                $"Crateria {lightningDefinition.Owner}");
        }
        AssertOverride(
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineDefinitionPointer,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions.GunshipEngineColorIndex,
            "Ceres gunship engine");
        AssertOverride(
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .SpriteNavigationLightsDefinitionPointer,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .SpriteNavigationLightsColorIndex,
            "sprite Ceres navigation lights");
        AssertOverride(
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .BackgroundNavigationLightsDefinitionPointer,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .BackgroundNavigationLightsColorIndex,
            "background Ceres navigation lights");
        foreach (PlanetZebesTextPaletteFxProgramDefinition fadeDefinition in
                 PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(
                fadeDefinition.DefinitionPointer,
                fadeDefinition.ColorByteIndex,
                $"PLANET ZEBES {fadeDefinition.Owner}");
        }
        foreach (CinematicGlowPaletteFxProgramDefinition glowDefinition in
                 CinematicGlowPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(
                glowDefinition.DefinitionPointer,
                glowDefinition.ColorByteIndex,
                $"cinematic glow {glowDefinition.Owner}");
        }
        AssertOverride(
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "exploding Zebes fade");
        AssertOverride(
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "unused cinematic fade");
        AssertOverride(
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "title-logo fade");
        foreach (NintendoLogoFadePaletteFxProgramDefinition nintendoDefinition in
                 NintendoLogoFadePaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(
                nintendoDefinition.DefinitionPointer,
                nintendoDefinition.ColorByteIndex,
                $"Nintendo shared fade {nintendoDefinition.Owner}");
        }
        AssertOverride(
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Zebes explosion foreground");
        AssertOverride(
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Zebes explosion finale");
        foreach (ZebesExplosionWhiteoutPaletteFxProgramDefinition whiteoutDefinition in
                 ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.All)
        {
            ushort colorByteIndex = whiteoutDefinition.Owner switch
            {
                ZebesExplosionWhiteoutPaletteFxProgramOwner.WideExplosionBackground =>
                    ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                        .WideExplosionBackgroundColorByteIndex,
                ZebesExplosionWhiteoutPaletteFxProgramOwner.SpaceWhiteout =>
                    ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                        .SpaceWhiteoutColorByteIndex,
                _ => throw new InvalidOperationException(
                    $"Unknown whiteout owner {whiteoutDefinition.Owner}."),
            };
            AssertOverride(whiteoutDefinition.DefinitionPointer, colorByteIndex,
                $"Zebes explosion whiteout {whiteoutDefinition.Owner}");
        }
        foreach (ZebesExplosionAmbientPaletteFxProgramDefinition ambientDefinition in
                 ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(ambientDefinition.DefinitionPointer,
                ambientDefinition.ColorByteIndex,
                $"Zebes explosion ambient {ambientDefinition.Owner}");
        }
        foreach (ZebesExplosionLayerFadePaletteFxProgramDefinition layerDefinition in
                 ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(layerDefinition.DefinitionPointer, layerDefinition.ColorByteIndex,
                $"Zebes explosion layer fade {layerDefinition.Owner}");
        }
        AssertOverride(
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Zebes explosion gunship");
        foreach (SamusLoadingSuitPaletteFxProgramDefinition suitDefinition in
                 SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(suitDefinition.DefinitionPointer,
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
                $"Samus loading {suitDefinition.Owner}");
        }
        AssertOverride(
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "post-credits icon glare");
        foreach (TourianEscapeRedFlashPaletteFxProgramDefinition flashDefinition in
                 TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(flashDefinition.DefinitionPointer, flashDefinition.ColorByteIndex,
                $"Tourian escape {flashDefinition.Owner}");
        }
        foreach (TourianEscapeSharedRedFlashPaletteFxProgramDefinition sharedDefinition in
                 TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(sharedDefinition.DefinitionPointer, sharedDefinition.ColorByteIndex,
                $"Tourian escape shared {sharedDefinition.Owner}");
        }
        AssertOverride(
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "old Tourian escape red flash");
        foreach (OldTourianEscapeAccentPaletteFxProgramDefinition accentDefinition in
                 OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(accentDefinition.DefinitionPointer, accentDefinition.ColorByteIndex,
                $"old Tourian escape {accentDefinition.Owner}");
        }
        AssertOverride(
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "upper Crateria escape red flash");
        foreach (CrateriaEscapeLightningPaletteFxProgramDefinition lightningDefinition in
                 CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(lightningDefinition.DefinitionPointer,
                lightningDefinition.ColorByteIndex,
                $"Crateria escape {lightningDefinition.Owner}");
        }
        AssertOverride(
            BeaconPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            BeaconPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Crateria and Brinstar beacon flash");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing room palette-FX override restores installed-content identity");
        Console.WriteLine(
            "Room palette-FX override: content identity and forty-seven live palette " +
            "CGRAM outputs " +
            "change immediately, then restore exactly.");

        static void ChangeRed(PaletteRgb5[][] frames)
        {
            PaletteRgb5 color = frames[0][0];
            frames[0][0] = color with
            {
                Red = color.Red == 31 ? 30 : color.Red + 1,
            };
        }

        void AssertOverride(ushort definitionPointer, ushort colorByteIndex,
            string description)
        {
            var stockRuntime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus)
            {
                MapPresentation = original,
            };
            var editedRuntime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus)
            {
                MapPresentation = edited,
            };
            stockRuntime.RoomPaletteFx.SpawnDefinition(bus, definitionPointer, 0);
            editedRuntime.RoomPaletteFx.SpawnDefinition(bus, definitionPointer, 0);
            stockRuntime.RoomPaletteFx.Step(bus, stockRuntime.Cgram, 0, 0, false, false);
            editedRuntime.RoomPaletteFx.Step(bus, editedRuntime.Cgram, 0, 0, false, false);
            AssertTrue(
                stockRuntime.Cgram.Colors[colorByteIndex / sizeof(ushort)] !=
                editedRuntime.Cgram.Colors[colorByteIndex / sizeof(ushort)],
                $"runtime catalog binding consumes {description} palette-FX override");
        }
    }

    private static void VerifyTitleGradientOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string source = Path.Combine(stock, TitleGradientFormat.FileName);
        TitleGradientDocument document = JsonSerializer.Deserialize<TitleGradientDocument>(
            File.ReadAllBytes(source),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted title gradient document is null.");
        foreach (TitleGradientVariant variant in document.Variants)
        {
            TitleGradientScanline line = variant.Lines[0];
            variant.Lines[0] = line with { Red = line.Red == 31 ? 30 : line.Red + 1 };
        }

        string replacement = Path.Combine(overrides, TitleGradientFormat.FileName);
        using (var stream = File.Create(replacement))
            TitleGradientPresentation.Write(stream, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "title gradient override changes installed-content identity");
        (TitleGradientLine[] rendered, ushort selectedZoom) = CaptureInstalledTitleGradient(bus, edited.TitleGradient);
        AssertTrue(rendered.AsSpan().SequenceEqual(edited.TitleGradient.Resolve(selectedZoom)),
            "production title consumes selected gradient override");
        AssertTrue(!rendered.AsSpan().SequenceEqual(original.TitleGradient.Resolve(selectedZoom)),
            "gradient override visibly changes production title output");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing title gradient override restores installed-content identity");
        Console.WriteLine("Title gradient override: content identity and production title output change immediately, then restore exactly.");
    }

    private static void VerifyTitlePaletteOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string source = Path.Combine(stock, TitlePaletteFormat.FileName);
        TitlePaletteDocument document = JsonSerializer.Deserialize<TitlePaletteDocument>(
            File.ReadAllBytes(source),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted title palette document is null.");
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5 color = document.Colors[index];
            document.Colors[index] = color with { Red = color.Red == 31 ? 30 : color.Red + 1 };
        }
        PaletteRgb5 ambient = document.BabyMetroidTubeLight[0][0];
        document.BabyMetroidTubeLight[0][0] = ambient with
        {
            Blue = ambient.Blue == 31 ? 30 : ambient.Blue + 1,
        };

        string replacement = Path.Combine(overrides, TitlePaletteFormat.FileName);
        using (var stream = File.Create(replacement))
            TitlePalettePresentation.Write(stream, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "title palette override changes installed-content identity");
        var stockTitle = new TitleSequenceState(bus, titlePalettePresentation: original.TitlePalette);
        var editedTitle = new TitleSequenceState(bus, titlePalettePresentation: edited.TitlePalette);
        AssertTrue(editedTitle.PaletteColors.SequenceEqual(edited.TitlePalette.Colors),
            "production title consumes selected palette override");
        stockTitle.Step(0);
        editedTitle.Step(0);
        TitleScreenAmbientPaletteFxProgramDefinition tube =
            TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All.Single(
                definition => definition.Owner ==
                    TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight);
        AssertTrue(stockTitle.PaletteColors[tube.ColorByteIndex / 2] !=
            editedTitle.PaletteColors[tube.ColorByteIndex / 2],
            "production title consumes selected ambient palette override");
        AssertTrue(!stockTitle.Render().AsSpan().SequenceEqual(editedTitle.Render()),
            "palette override visibly changes production title output");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing title palette override restores installed-content identity");
        Console.WriteLine("Title palette override: content identity and production title output change immediately, then restore exactly.");
    }

    private static void VerifyTitleGraphicsOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string name = TitleGraphicsFormat.Mode7TilesFile;
        IndexedPngImage image;
        using (var input = File.OpenRead(Path.Combine(stock, name)))
            image = IndexedPng.Read(input, TitleGraphicsFormat.Mode7Width, TitleGraphicsFormat.Mode7Height);
        byte[] pixels = image.Pixels.Select(value => unchecked((byte)(value + 1))).ToArray();
        string replacement = Path.Combine(overrides, name);
        using (var output = File.Create(replacement))
            IndexedPng.Write(output, image.Width, image.Height, pixels, image.Palette);

        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "title graphics override changes installed-content identity");
        var stockTitle = new TitleSequenceState(bus,
            titleGradientPresentation: original.TitleGradient,
            titlePalettePresentation: original.TitlePalette,
            titleGraphicsPresentation: original.TitleGraphics);
        var editedTitle = new TitleSequenceState(bus,
            titleGradientPresentation: edited.TitleGradient,
            titlePalettePresentation: edited.TitlePalette,
            titleGraphicsPresentation: edited.TitleGraphics);
        stockTitle.Step((ushort)SuperMetroid.Core.Input.SnesButton.Start);
        editedTitle.Step((ushort)SuperMetroid.Core.Input.SnesButton.Start);
        for (int frame = 0; frame < 16; frame++)
        {
            stockTitle.Step(0);
            editedTitle.Step(0);
        }
        AssertTrue(!stockTitle.Render().AsSpan().SequenceEqual(editedTitle.Render()),
            "Mode 7 PNG override visibly changes production title output");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing title graphics override restores installed-content identity");
        Console.WriteLine("Title graphics override: content identity and production title output change immediately, then restore exactly.");
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
        VerifyFileSelectMapCatalog(bus, guard, original, edited);
        VerifyInstalledFileSelectMenu(bus, guard, original, edited);
        Console.WriteLine("Live map catalog: Ceres room-entry/pause reject map ROM access; debugger rebind excludes stale content and preserves scroll.");
    }

    private static void VerifyFileSelectMapCatalog(ISnesAddressSpace bus, ISnesAddressSpace guard,
        AreaMapPresentationCatalog original, AreaMapPresentationCatalog edited)
    {
        var system = new Bank80SystemState();
        system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
        var stock = new SuperMetroid.Core.Frontend.FileSelectRoomMapGraphics(bus, system, AreaId.Crateria);
        var installed = new SuperMetroid.Core.Frontend.FileSelectRoomMapGraphics(guard, system, AreaId.Crateria,
            mapPresentation: original);
        AssertTrue(stock.RenderBackgrounds(0, 0).AsSpan().SequenceEqual(installed.RenderBackgrounds(0, 0)),
            "file-select installed stock renders identical pixels without map ROM reads");
        installed.BindMapPresentation(edited);
        byte[] expected = AreaMapTilemapBuilder.Build(edited.Get(AreaId.Crateria), system, MapTileWords.FileSelectUndownloadedBlank);
        byte[] unchanged = AreaMapTilemapBuilder.Build(original.Get(AreaId.Crateria), system, MapTileWords.FileSelectUndownloadedBlank);
        AssertTrue(!expected.AsSpan().SequenceEqual(unchanged), "override changes a visible file-select cell");
        for (int i = 0; i < expected.Length / 2; i++)
            AssertEqual(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(expected.AsSpan(i * 2)),
                installed.Vram.ReadWord(SuperMetroid.Core.Frontend.MenuPpuState.Bg1TilemapWord + i), "file-select rebind writes exact edited tilemap");
        using var snapshot = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(snapshot, installed);
        snapshot.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Frontend.FileSelectRoomMapGraphics>(snapshot);
        restored.BindMapPresentation(original);
        AssertTrue(stock.RenderBackgrounds(0, 0).AsSpan().SequenceEqual(restored.RenderBackgrounds(0, 0)),
            "restored file-select graphics accept current stock without stale override pixels");
        Console.WriteLine("File-select map: exact stock pixels, edited BG1 words, and graphics-state rebind pass.");
    }

    private sealed class MapDataGuard(ISnesAddressSpace source, AreaMapCartridgeData[] maps) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if ((address >= MapStaticPalettesRomData.PausePalette && address < MapStaticPalettesRomData.PausePalette + SnesCgram.ByteCount) ||
                (address >= SuperMetroid.Core.Frontend.FileSelectMapRomData.EntryPalette && address < SuperMetroid.Core.Frontend.FileSelectMapRomData.EntryPalette + SnesCgram.ByteCount) ||
                (address >= SuperMetroid.Core.Frontend.FileSelectMapRomData.PaletteColors && address < MapStaticPalettesRomData.WorldPaletteDataEnd))
                throw new InvalidOperationException($"Live presentation read static map palette ROM at {address:X6}.");
            if ((address >= SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteTiming &&
                address <= SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteTiming + SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteFrameCount * SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteTimingStride) ||
                (address >= SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteColors &&
                address < SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteColors + SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteFrameCount * MapPaletteCycleFormat.ColorCount * 2))
                throw new InvalidOperationException($"Live presentation read map palette-cycle ROM at {address:X6}.");
            if (address >= HudTileAtlasFormat.SourceAddress && address < HudTileAtlasFormat.SourceAddress + HudTileAtlasFormat.TransferByteCount)
                throw new InvalidOperationException($"Live presentation read HUD atlas ROM at {address:X6}.");
            if (address >= MapTileAtlasFormat.SourceAddress && address < MapTileAtlasFormat.SourceAddress + MapTileAtlasFormat.ByteCount)
                throw new InvalidOperationException($"Live presentation read map atlas ROM at {address:X6}.");
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
