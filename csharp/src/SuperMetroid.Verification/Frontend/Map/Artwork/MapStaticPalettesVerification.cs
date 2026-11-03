using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Frontend.Map.Artwork;

/// <summary>Checks native pause and file-select map colors, visual overrides,
/// and palette rebinding through live map and pause presentation.</summary>
internal static class MapStaticPalettesVerification
{
    /// <summary>Compares stock palettes with the cartridge, then checks edited
    /// colors survive state restoration, stock replacement, and an ongoing fade.</summary>
    internal static void VerifyMapStaticPaletteIntegration(ISnesAddressSpace bus, string stock, string overrides,
        AreaMapPresentation original, ImportedAreaMap[] rules)
    {
        var guard = NewMapDataGuard(bus, rules);
        var palette = new SnesCgram();
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(palette, bus, MapSourceDefinitions.Artwork.PauseColors);
        MapStaticPalettes installed = InstalledAreaMapPresentation.PalettesOf(original);
        AssertTrue(palette.Colors.SequenceEqual(installed.Pause), "all static pause colors match cartridge");
        AssertEqual(palette.Colors[0], original.Backdrops.Pause, "the pause backdrop is the cartridge's backdrop colour");
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(palette, bus, MapSourceDefinitions.Menu.Colors);
        AssertTrue(palette.Colors.SequenceEqual(installed.FileSelect), "all static file-select colors match cartridge");
        AssertEqual(palette.Colors[0], original.Backdrops.FileSelect, "the file-select backdrop is the cartridge's backdrop colour");
        var nativeWorld = new FileSelectAreaMapGraphics(0, original.Backdrops, original.WorldArtwork, MenuSheetFixtures.For(original));
        var world = new FileSelectAreaMapGraphics(0, original.Backdrops, original.WorldArtwork, MenuSheetFixtures.For(original));
        for (int area = 0; area < FileSelectMapDefinitions.AreaCount; area++)
        {
            nativeWorld.SelectArea(area); world.SelectArea(area);
            AssertEqual(installed.World(AreaIds.At(area))[0], world.screen.Backdrop, "the selected area shows its world-map backdrop");
            AssertTrue(nativeWorld.RenderBackgrounds().SequenceEqual(world.RenderBackgrounds()), "all six stock world selections render identical pixels");
        }
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<MapStaticPalettesDocument>(File.ReadAllBytes(Path.Combine(stock, MapStaticPalettesFormat.FileName)), options);
        PaletteRgb5[] Invert(PaletteRgb5[] colors) => colors.Select(c => new PaletteRgb5 { Red = 31 - c.Red, Green = 31 - c.Green, Blue = 31 - c.Blue }).ToArray();
        var replacementDocument = document with { Pause = Invert(document.Pause), FileSelect = Invert(document.FileSelect),
            World = document.World.ToDictionary(pair => pair.Key, pair => Invert(pair.Value)) };
        Directory.CreateDirectory(overrides);
        string path = Path.Combine(overrides, MapStaticPalettesFormat.FileName);
        using (var output = File.Create(path)) MapStaticPalettesFile.Write(output, replacementDocument);
        byte[] replacementBytes = File.ReadAllBytes(path);
        var edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "static palette edit changes content identity");
        world.BindFixtureArtwork(edited);
        AssertEqual(FileSelectMapDefinitions.AreaCount - 1, world.SelectedArea, "palette bind preserves selected world area");
        AssertTrue(!nativeWorld.RenderBackgrounds().SequenceEqual(world.RenderBackgrounds()), "world palette edit changes rendered pixels");

        var system = new SharedGameState();
        system.Progress.ExploreEveryCell();
        var room = new FileSelectRoomMapGraphics(system, AreaId.Crateria, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
        var pause = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        var roomBefore = room.RenderBackgrounds(new(0, 0)).ToArray(); var pauseBefore = pause.Render();
        room.BindFixtureArtwork(edited); pause.BindFixtureArtwork(edited);
        AssertTrue(!roomBefore.SequenceEqual(room.RenderBackgrounds(new(0, 0))), "static palette edit reaches room-map pixels");
        AssertTrue(!pauseBefore.SequenceEqual(pause.Render()), "static palette edit reaches pause-map pixels");
        using var state = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(state, pause); state.Position = 0;
        pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(state);
        pause.BindFixtureArtwork(original); room.BindFixtureArtwork(original); world.BindFixtureArtwork(original);
        AssertTrue(pauseBefore.SequenceEqual(pause.Render()), "restored pause accepts current static colors without stale edited state");
        AssertTrue(roomBefore.SequenceEqual(room.RenderBackgrounds(new(0, 0))), "room map rebind restores original colors");
        AssertEqual(nativeWorld.screen.Backdrop, world.screen.Backdrop, "world rebind restores the original backdrop");
        AssertTrue(nativeWorld.RenderBackgrounds().SequenceEqual(world.RenderBackgrounds()), "world rebind restores the original pixels");

        string rebuilt = Path.Combine(overrides, "stock-rebuilt");
        AssertEqual(edited.ContentIdentity, MapStockReExtraction.ReExtractStock(bus, rebuilt, overrides).ContentIdentity, "static palette override survives stock replacement");
        AssertTrue(replacementBytes.SequenceEqual(File.ReadAllBytes(path)), "static palette overrides are not rewritten");
        var badColor = document.Pause.ToArray(); badColor[0] = badColor[0] with { Blue = -1 };
        foreach (var invalid in new[] { document with { Version = 0 }, document with { Pause = [] }, document with { Pause = badColor },
            document with { World = new Dictionary<string, PaletteRgb5[]>() } })
        {
            using var output = new MemoryStream();
            AssertThrows<InvalidDataException>(() => MapStaticPalettesFile.Write(output, invalid), "invalid static palette schema/color/area rejected");
        }
        File.WriteAllText(path, "{broken static colors");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "corrupt static palette override fails without fallback");
        AssertEqual("{broken static colors", File.ReadAllText(path), "corrupt static override retained for repair");
        Console.WriteLine("Static map palettes: all six native world selections, pause/file-select colors, visible edits, state/fade rebind and override validation pass.");
    }
}
