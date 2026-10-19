using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Map;
using SuperMetroid.Diagnostics.SaveStates;

namespace SuperMetroid.Verification.Frontend.Map.Artwork;

/// <summary>Checks native pause and file-select map colors, visual overrides,
/// and palette rebinding through live map and pause presentation.</summary>
internal static class MapStaticPalettesVerification
{
    /// <summary>Compares stock palettes with the cartridge, then checks edited
    /// colors survive state restoration, stock replacement, and an ongoing fade.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the installed static map palettes")]
    internal static void VerifyMapStaticPaletteIntegration(ISnesAddressSpace bus, string stock, string overrides,
        AreaMapPresentationCatalog original, AreaMapCartridgeData[] rules)
    {
        var guard = NewMapDataGuard(bus, rules);
        var palette = new SnesCgram();
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(palette, bus, MapStaticPalettesRomData.PausePalette);
        AssertTrue(palette.Colors.SequenceEqual(original.Palettes.Pause), "all static pause colors match cartridge");
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(palette, bus, FileSelectMapRomData.EntryPalette);
        AssertTrue(palette.Colors.SequenceEqual(original.Palettes.FileSelect), "all static file-select colors match cartridge");
        var nativeWorld = new FileSelectAreaMapGraphics(0, original.Tiles,
            original.Palettes, original.Screens, original.WorldArtwork, original.Sprites);
        var world = new FileSelectAreaMapGraphics(0, original.Tiles,
            original.Palettes, original.Screens, original.WorldArtwork, original.Sprites);
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            nativeWorld.SelectArea(area); world.SelectArea(area);
            AssertTrue(new CgramPalettes(nativeWorld.screen.Palette).ImageEquals(new CgramPalettes(world.screen.Palette)), "named world-selection palette matches native copy programs");
            AssertTrue(nativeWorld.RenderBackgrounds().SequenceEqual(world.RenderBackgrounds()), "all six stock world selections render identical pixels");
        }
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<MapStaticPalettesDocument>(File.ReadAllBytes(Path.Combine(stock, MapStaticPalettesFormat.FileName)), options);
        PaletteRgb5[] Invert(PaletteRgb5[] colors) => colors.Select(c => new PaletteRgb5 { Red = 31 - c.Red, Green = 31 - c.Green, Blue = 31 - c.Blue }).ToArray();
        var replacementDocument = document with { Pause = Invert(document.Pause), FileSelect = Invert(document.FileSelect),
            World = document.World.ToDictionary(pair => pair.Key, pair => Invert(pair.Value)) };
        Directory.CreateDirectory(overrides);
        string path = Path.Combine(overrides, MapStaticPalettesFormat.FileName);
        using (var output = File.Create(path)) MapStaticPalettes.Write(output, replacementDocument);
        byte[] replacementBytes = File.ReadAllBytes(path);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "static palette edit changes content identity");
        world.BindMapPresentation(edited);
        AssertEqual(FileSelectMapRomData.AreaCount - 1, world.SelectedArea, "palette bind preserves selected world area");
        AssertTrue(!nativeWorld.RenderBackgrounds().SequenceEqual(world.RenderBackgrounds()), "world palette edit changes rendered pixels");

        var system = new Bank80SystemState();
        system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
        var room = new FileSelectRoomMapGraphics(system, AreaId.Crateria, mapPresentation: original);
        var pause = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(), System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original });
        var roomBefore = room.RenderBackgrounds(new(0, 0)).ToArray(); var pauseBefore = pause.Render();
        room.BindMapPresentation(edited); pause.BindMapPresentation(edited);
        AssertTrue(!roomBefore.SequenceEqual(room.RenderBackgrounds(new(0, 0))), "static palette edit reaches room-map pixels");
        AssertTrue(!pauseBefore.SequenceEqual(pause.Render()), "static palette edit reaches pause-map pixels");
        using var state = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(state, pause); state.Position = 0;
        pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(state);
        pause.BindMapPresentation(original); room.BindMapPresentation(original); world.BindMapPresentation(original);
        AssertTrue(pauseBefore.SequenceEqual(pause.Render()), "restored pause accepts current static colors without stale edited state");
        AssertTrue(roomBefore.SequenceEqual(room.RenderBackgrounds(new(0, 0))), "room map rebind restores original colors");
        AssertTrue(new CgramPalettes(nativeWorld.screen.Palette).ImageEquals(new CgramPalettes(world.screen.Palette)), "world rebind restores complete original colors");

        var entry = new FileSelectMapEntry(original.Palettes);
        var controlEntry = new FileSelectMapEntry(original.Palettes);
        for (int tick = 0; tick < 5; tick++) { entry.Step(); controlEntry.Step(); }
        entry.BindPalettes(edited.Palettes);
        for (int tick = 0; tick < 48; tick++)
        {
            entry.Step(); controlEntry.Step();
            AssertEqual(controlEntry.Phase, entry.Phase, "palette replacement does not restart entry fade or reveal");
        }
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            AssertEqual(color is 14 or 30 ? Bgr555.Black : edited.Palettes.FileSelect[color], entry.Cgram.Colors[color], "ongoing fade reaches newly bound palette target");

        string rebuilt = Path.Combine(overrides, "stock-rebuilt");
        SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, AreaMapPresentationCatalog.Load(rebuilt, overrides).ContentIdentity, "static palette override survives stock replacement");
        AssertTrue(replacementBytes.SequenceEqual(File.ReadAllBytes(path)), "static palette overrides are not rewritten");
        var badColor = document.Pause.ToArray(); badColor[0] = badColor[0] with { Blue = -1 };
        foreach (var invalid in new[] { document with { Version = 0 }, document with { Pause = [] }, document with { Pause = badColor },
            document with { World = new Dictionary<string, PaletteRgb5[]>() } })
        {
            using var output = new MemoryStream();
            AssertThrows<InvalidDataException>(() => MapStaticPalettes.Write(output, invalid), "invalid static palette schema/color/area rejected");
        }
        File.WriteAllText(path, "{broken static colors");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "corrupt static palette override fails without fallback");
        AssertEqual("{broken static colors", File.ReadAllText(path), "corrupt static override retained for repair");
        Console.WriteLine("Static map palettes: all six native world selections, pause/file-select colors, visible edits, state/fade rebind and override validation pass.");
    }
}
