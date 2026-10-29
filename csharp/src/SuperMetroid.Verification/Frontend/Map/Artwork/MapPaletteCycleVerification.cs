using SuperMetroid.Core.Assets;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Frontend.Map.Artwork;

/// <summary>Checks installed map highlight palettes in file-select arrows and pause and map screens.</summary>
internal static class MapPaletteCycleVerification
{
    /// <summary>Compares native animation cadence and CGRAM, then checks override rebinding and visible palette isolation.</summary>
    internal static void VerifyMapPaletteCycleIntegration(ISnesAddressSpace bus, string stock, string overrides,
        AreaMapPresentation original, ImportedAreaMap[] rules)
    {
        var guard = NewMapDataGuard(bus, rules);
        var arrows = new FileSelectMapAnimations(original.Arrows);
        arrows.StepArrows(_ => true);
        var arrowOam = ScreenSpritesFixtures.Blank();
        arrowOam.BeginFrame(); arrows.DrawArrows(arrowOam, MenuSheetFixtures.MapSprites(original.Sprites)); arrowOam.FinishFrame();
        ushort nativeArrowPalette = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), MapAnimationNativeSources.AnimatedSpritePalette);
        AssertTrue(arrowOam.NextByteOffset > 0, "arrow palette test draws actual sprites");
        for (int offset = 0; offset < arrowOam.NextByteOffset; offset += 4)
            AssertEqual(LittleEndianBytes.HighByte(nativeArrowPalette), (byte)(arrowOam.LowTable[offset + 3] & 14), "each file-select arrow uses cartridge animated palette");
        var native = new MapPaletteAnimation();
        native.Bind(original.HighlightCycle);
        var installed = new MapPaletteAnimation();
        installed.Bind(original.HighlightCycle);
        AssertEqual(MapAnimationNativeSources.PaletteFrameCount, original.HighlightCycle.FrameCount, "imported native highlight frame count");
        var nativeColors = new ScreenPalette();
        var installedColors = new ScreenPalette();
        int loops = 0;
        for (int tick = 0; tick < 600; tick++)
        {
            bool looped = native.Step(nativeColors);
            if (looped) loops++;
            AssertEqual(looped, installed.Step(installedColors), "imported highlight requests loop sound on native tick");
            AssertTrue(new CgramPalettes(nativeColors).ImageEquals(new CgramPalettes(installedColors)), "all CGRAM colors match throughout imported highlight cycle");
        }
        AssertTrue(loops > 5, "highlight parity crosses multiple loop boundaries");

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<MapPaletteCycleDocument>(File.ReadAllBytes(Path.Combine(stock, MapPaletteCycleFormat.FileName)), options);
        var replacementDocument = document with { Frames = document.Frames.Select(f => f with
        {
            Colors = f.Colors.Select(c => new PaletteRgb5 { Red = 31 - c.Red, Green = 31 - c.Green, Blue = 31 - c.Blue }).ToArray()
        }).ToArray() };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, MapPaletteCycleFormat.FileName);
        using (var output = File.Create(replacement)) MapPaletteCycleFile.Write(output, replacementDocument);
        byte[] editedBytes = File.ReadAllBytes(replacement);
        var edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "palette edit invalidates selected content identity");

        var system = new SharedGameState();
        var pause = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(), System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        var nativePause = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(), System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        for (int tick = 0; tick < 60; tick++)
        {
            pause.Step(0, 0); nativePause.Step(0, 0);
            AssertTrue(pause.Render().SequenceEqual(nativePause.Render()), "stock imported highlight gives exact pause pixels through full cycle");
        }
        using var snapshot = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(snapshot, pause);
        snapshot.Position = 0;
        pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(snapshot);
        pause.BindFixtureArtwork(edited);
        bool cycleColorsChanged = false;
        for (int tick = 0; tick < 60; tick++)
        {
            pause.Step(0, 0); nativePause.Step(0, 0);
            var editedMemory = pause.CaptureRenderSnapshot().Paint;
            var stockMemory = nativePause.CaptureRenderSnapshot().Paint;
            cycleColorsChanged |= !editedMemory.Colors.Slice(ScreenColourPositions.Of(ScreenColourGroup.MapHighlight), 16)
                .SequenceEqual(stockMemory.Colors.Slice(ScreenColourPositions.Of(ScreenColourGroup.MapHighlight), 16));
            AssertTrue(editedMemory.Colors.Slice(240, 16).SequenceEqual(stockMemory.Colors.Slice(240, 16)),
                "highlight edit leaves the native palette-seven pause cursor colors unchanged");
            // Since #1251 the pause-map scroll arrows draw with OBJ palette 3 as on the cartridge,
            // so the highlight edit may recolor them. Every differing pixel must therefore show a
            // palette-3 color in both renders; nothing drawn from other palettes may change.
            var editedPalette3 = new HashSet<RgbaPixel>();
            var stockPalette3 = new HashSet<RgbaPixel>();
            for (int color = 0; color < 16; color++)
            {
                editedPalette3.Add(editedMemory.Colors[ScreenColourPositions.Of(ScreenColourGroup.MapHighlight) + color].ToRgbaPixel());
                stockPalette3.Add(stockMemory.Colors[ScreenColourPositions.Of(ScreenColourGroup.MapHighlight) + color].ToRgbaPixel());
            }
            var editedPixels = pause.Render();
            var stockPixels = nativePause.Render();
            for (int pixel = 0; pixel < editedPixels.Length; pixel++)
                if (editedPixels[pixel] != stockPixels[pixel])
                    AssertTrue(editedPalette3.Contains(editedPixels[pixel]) && stockPalette3.Contains(stockPixels[pixel]),
                        "palette-three highlight edit recolors only palette-three pixels, not the palette-seven pause cursor");
            AssertEqual(nativePause.MapScroll, pause.MapScroll, "palette-only edit preserves map navigation");
        }
        AssertTrue(cycleColorsChanged, "edited highlight colors animate in restored pause CGRAM with palette ROM blocked");
        pause.BindFixtureArtwork(original);
        for (int tick = 0; tick < 60; tick++) { pause.Step(0, 0); nativePause.Step(0, 0); }
        AssertTrue(pause.Render().SequenceEqual(nativePause.Render()), "rebind preserves timing and returns to exact stock animated pixels");

        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        var saved = new SuperMetroidSaveSnapshot { Area = (ushort)AreaId.Maridia, SaveStation = 0, Health = 99, MaxHealth = 99 };
        saved.MapStationBytes[(int)AreaId.Maridia] = 1;
        saved.UsedSaveStationBytes[(int)AreaId.Maridia * 2] = 1;
        saves.SaveSlot(0, saved);
        var slot = saves.ReadSlot(0);
        var nativeMenu = new FileSelectMapMenuState(new SuperMetroid.Core.Audio.AudioQueues(), (slot ?? throw new InvalidOperationException("Verification requires slot.")), 0, original, MenuSheetFixtures.For(original));
        var menu = new FileSelectMapMenuState(new SuperMetroid.Core.Audio.AudioQueues(), slot, 0, original, MenuSheetFixtures.For(original));
        for (int tick = 0; tick < 48; tick++) { nativeMenu.Step(0); menu.Step(0); }
        nativeMenu.Step(ControllerButton.Start);
        menu.Step(ControllerButton.Start);
        for (int tick = 0; tick < 54; tick++) { nativeMenu.Step(0); menu.Step(0); }
        AssertEqual(FileSelectMapNavigationPhase.Room, menu.Phase, "palette replacement test reaches file-select room map");
        menu.BindFixtureArtwork(edited);
        bool menuEdit = false;
        for (int tick = 0; tick < 60; tick++)
        {
            nativeMenu.Step(0); menu.Step(0);
            menuEdit |= !menu.Render().SequenceEqual(nativeMenu.Render());
            AssertEqual(nativeMenu.Phase, menu.Phase, "palette replacement does not change file-select phase");
        }
        AssertTrue(menuEdit, "edited palette visibly animates in installed file-select map");

        // Replacing with a shorter cycle must not index a saved, now nonexistent
        // frame. Binding retains the pending delay, then wraps at its boundary.
        using var shortJson = new MemoryStream();
        MapPaletteCycleFile.Write(shortJson, document with { Frames = [document.Frames[0] with { DurationTicks = 2 }] });
        shortJson.Position = 0;
        var shortCycle = MapPaletteCycleFile.Read(shortJson);
        installed.Reset(); installed.Step(installedColors); // Native frame one, delay three.
        installed.Bind(shortCycle);
        AssertTrue(!installed.Step(installedColors) && !installed.Step(installedColors), "shorter cycle preserves outstanding delay");
        AssertTrue(installed.Step(installedColors), "shorter cycle wraps saved out-of-range frame at next boundary");
        AssertTrue(!installed.Step(installedColors) && installed.Step(installedColors), "authored duration controls subsequent cycle ticks");

        string rebuilt = Path.Combine(overrides, "stock-rebuilt");
        SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, MapPresentationFiles.Load(rebuilt, overrides).ContentIdentity, "palette override survives re-extraction and reload");
        AssertTrue(editedBytes.SequenceEqual(File.ReadAllBytes(replacement)), "palette override never rewritten");
        foreach (string invalid in new[] { "null", "{}", "{\"version\":1,\"frames\":[]}", "{\"version\":1,\"frames\":null}" })
        {
            using var input = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid));
            AssertThrows<InvalidDataException>(() => MapPaletteCycleFile.Read(input), "invalid palette schema rejected");
        }
        foreach (int duration in new[] { 0, 255 })
        {
            var invalid = document with { Frames = [document.Frames[0] with { DurationTicks = duration }] };
            using var output = new MemoryStream();
            AssertThrows<InvalidDataException>(() => MapPaletteCycleFile.Write(output, invalid), "invalid palette duration rejected");
        }
        var badColors = document.Frames[0].Colors.ToArray();
        badColors[0] = badColors[0] with { Red = 32 };
        using (var output = new MemoryStream())
            AssertThrows<InvalidDataException>(() => MapPaletteCycleFile.Write(output, document with { Frames = [document.Frames[0] with { Colors = badColors }] }), "palette RGB overflow rejected");
        File.WriteAllText(replacement, "broken palette JSON");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "invalid palette override never falls back to stock");
        AssertEqual("broken palette JSON", File.ReadAllText(replacement), "invalid palette override retained for repair");
        Console.WriteLine("Map palette cycle: native colors/loop ticks, full pause pixels, edited restored animation, override preservation and strict validation pass.");
    }
}
