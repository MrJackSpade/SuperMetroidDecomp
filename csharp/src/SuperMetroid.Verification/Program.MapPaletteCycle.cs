using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyMapPaletteCycleIntegration(ISnesAddressSpace bus, string stock, string overrides,
        AreaMapPresentationCatalog original, AreaMapCartridgeData[] rules)
    {
        var guard = new MapDataGuard(bus, rules);
        var arrows = new FileSelectMapAnimations(bus);
        arrows.StepArrows(_ => true);
        var arrowOam = new OamBuffer();
        arrowOam.BeginFrame(); arrows.DrawArrows(arrowOam);
        ushort nativeArrowPalette = SuperMetroid.Core.Rom.RomDataReader.ReadWordFixedBank(bus, MapAnimationRomData.AnimatedSpritePalette);
        AssertTrue(arrowOam.NextByteOffset > 0, "arrow palette test draws actual sprites");
        for (int offset = 0; offset < arrowOam.NextByteOffset; offset += 4)
            AssertEqual((byte)(nativeArrowPalette >> 8), (byte)(arrowOam.LowTable[offset + 3] & 14), "each file-select arrow uses cartridge animated palette");
        var native = new MapPaletteAnimation(bus);
        var installed = new MapPaletteAnimation(new ForbiddenMapBus());
        installed.Bind(original.HighlightCycle);
        AssertEqual(MapAnimationRomData.PaletteFrameCount, original.HighlightCycle.FrameCount, "imported native highlight frame count");
        var nativeColors = new SnesCgram();
        var installedColors = new SnesCgram();
        int loops = 0;
        for (int tick = 0; tick < 600; tick++)
        {
            bool looped = native.Step(nativeColors);
            if (looped) loops++;
            AssertEqual(looped, installed.Step(installedColors), "imported highlight requests loop sound on native tick");
            AssertTrue(nativeColors.Colors.SequenceEqual(installedColors.Colors), "all CGRAM colors match throughout imported highlight cycle");
        }
        AssertTrue(loops > 5, "highlight parity crosses multiple loop boundaries");

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<MapPaletteCycleDocument>(File.ReadAllBytes(Path.Combine(stock, MapPaletteCycleFormat.FileName)), options)!;
        var replacementDocument = document with { Frames = document.Frames.Select(f => f with
        {
            Colors = f.Colors.Select(c => new PaletteRgb5 { Red = 31 - c.Red, Green = 31 - c.Green, Blue = 31 - c.Blue }).ToArray()
        }).ToArray() };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, MapPaletteCycleFormat.FileName);
        using (var output = File.Create(replacement)) MapPaletteCycle.Write(output, replacementDocument);
        byte[] editedBytes = File.ReadAllBytes(replacement);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "palette edit invalidates selected content identity");

        var system = new Bank80SystemState();
        var pause = new PauseMenuState(guard, new SamusState(), system, AreaId.Crateria, 0, 0, mapPresentation: original);
        var nativePause = new PauseMenuState(bus, new SamusState(), system, AreaId.Crateria, 0, 0);
        for (int tick = 0; tick < 60; tick++)
        {
            pause.Step(0, 0); nativePause.Step(0, 0);
            AssertTrue(pause.Render().AsSpan().SequenceEqual(nativePause.Render()), "stock imported highlight gives exact pause pixels through full cycle");
        }
        using var snapshot = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(snapshot, pause);
        snapshot.Position = 0;
        pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(snapshot);
        pause.BindMapPresentation(edited);
        bool visibleEdit = false;
        for (int tick = 0; tick < 60; tick++)
        {
            pause.Step(0, 0); nativePause.Step(0, 0);
            visibleEdit |= !pause.Render().AsSpan().SequenceEqual(nativePause.Render());
            AssertEqual(nativePause.MapHorizontalScroll, pause.MapHorizontalScroll, "palette-only edit preserves map horizontal navigation");
            AssertEqual(nativePause.MapVerticalScroll, pause.MapVerticalScroll, "palette-only edit preserves map vertical navigation");
        }
        AssertTrue(visibleEdit, "edited colors visibly animate in restored pause menu with palette ROM blocked");
        pause.BindMapPresentation(original);
        for (int tick = 0; tick < 60; tick++) { pause.Step(0, 0); nativePause.Step(0, 0); }
        AssertTrue(pause.Render().AsSpan().SequenceEqual(nativePause.Render()), "rebind preserves timing and returns to exact stock animated pixels");

        var saves = new SuperMetroidSaveRam(bus);
        var saved = new SuperMetroidSaveSnapshot { Area = (ushort)AreaId.Maridia, SaveStation = 0, Health = 99, MaxHealth = 99 };
        saved.MapStationBytes[(int)AreaId.Maridia] = 1;
        saved.UsedSaveStationBytes[(int)AreaId.Maridia * 2] = 1;
        saves.SaveSlot(0, saved);
        var slot = saves.ReadSlot(0)!;
        var nativeMenu = new FileSelectMapMenuState(bus, new SuperMetroid.Core.Audio.CartridgeAudioState(), slot, 0);
        var menu = new FileSelectMapMenuState(guard, new SuperMetroid.Core.Audio.CartridgeAudioState(), slot, 0, original);
        for (int tick = 0; tick < 48; tick++) { nativeMenu.Step(0); menu.Step(0); }
        nativeMenu.Step((ushort)SuperMetroid.Core.Input.SnesButton.Start);
        menu.Step((ushort)SuperMetroid.Core.Input.SnesButton.Start);
        for (int tick = 0; tick < 54; tick++) { nativeMenu.Step(0); menu.Step(0); }
        AssertEqual(FileSelectMapNavigationPhase.Room, menu.Phase, "palette replacement test reaches file-select room map");
        menu.BindMapPresentation(edited);
        bool menuEdit = false;
        for (int tick = 0; tick < 60; tick++)
        {
            nativeMenu.Step(0); menu.Step(0);
            menuEdit |= !menu.Render().AsSpan().SequenceEqual(nativeMenu.Render());
            AssertEqual(nativeMenu.Phase, menu.Phase, "palette replacement does not change file-select phase");
        }
        AssertTrue(menuEdit, "edited palette visibly animates in installed file-select map");

        // Replacing with a shorter cycle must not index a saved, now nonexistent
        // frame. Binding retains the pending delay, then wraps at its boundary.
        using var shortJson = new MemoryStream();
        MapPaletteCycle.Write(shortJson, document with { Frames = [document.Frames[0] with { DurationTicks = 2 }] });
        shortJson.Position = 0;
        var shortCycle = MapPaletteCycle.Load(shortJson);
        installed.Reset(); installed.Step(installedColors); // Native frame one, delay three.
        installed.Bind(shortCycle);
        AssertTrue(!installed.Step(installedColors) && !installed.Step(installedColors), "shorter cycle preserves outstanding delay");
        AssertTrue(installed.Step(installedColors), "shorter cycle wraps saved out-of-range frame at next boundary");
        AssertTrue(!installed.Step(installedColors) && installed.Step(installedColors), "authored duration controls subsequent cycle ticks");

        string rebuilt = Path.Combine(overrides, "stock-rebuilt");
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, AreaMapPresentationCatalog.Load(rebuilt, overrides).ContentIdentity, "palette override survives re-extraction and reload");
        AssertTrue(editedBytes.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)), "palette override never rewritten");
        foreach (string invalid in new[] { "null", "{}", "{\"version\":1,\"frames\":[]}", "{\"version\":1,\"frames\":null}" })
        {
            using var input = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid));
            AssertThrows<InvalidDataException>(() => MapPaletteCycle.Load(input), "invalid palette schema rejected");
        }
        foreach (int duration in new[] { 0, 255 })
        {
            var invalid = document with { Frames = [document.Frames[0] with { DurationTicks = duration }] };
            using var output = new MemoryStream();
            AssertThrows<InvalidDataException>(() => MapPaletteCycle.Write(output, invalid), "invalid palette duration rejected");
        }
        var badColors = document.Frames[0].Colors.ToArray();
        badColors[0] = badColors[0] with { Red = 32 };
        using (var output = new MemoryStream())
            AssertThrows<InvalidDataException>(() => MapPaletteCycle.Write(output, document with { Frames = [document.Frames[0] with { Colors = badColors }] }), "palette RGB overflow rejected");
        File.WriteAllText(replacement, "broken palette JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "invalid palette override never falls back to stock");
        AssertEqual("broken palette JSON", File.ReadAllText(replacement), "invalid palette override retained for repair");
        Console.WriteLine("Map palette cycle: native colors/loop ticks, full pause pixels, edited restored animation, override preservation and strict validation pass.");
    }
}
