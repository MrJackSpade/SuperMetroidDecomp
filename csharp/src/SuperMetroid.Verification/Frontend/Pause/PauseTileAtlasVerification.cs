using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Diagnostics.SaveStates;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks pause UI tile artwork, page rendering, and PNG override isolation against native output.</summary>
internal static class PauseTileAtlasVerification
{
    /// <summary>Compares stock VRAM bytes, three-area menu transitions, live and captured edits, restore rebinding, and malformed resources.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause UI tile artwork")]
    internal static void VerifyPauseTileAtlas(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var vram = new SnesVram(); original.PauseTiles.LoadTo(new VramStore(vram).All.Slice(PauseTileAtlasFormat.DestinationByte / sizeof(ushort), MapTileAtlasFormat.ByteCount / sizeof(ushort)));
        AssertTrue(vram.Bytes.Slice(PauseTileAtlasFormat.DestinationByte, PauseTileAtlasFormat.ByteCount).SequenceEqual(
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), PauseTileAtlasFormat.SourceAddress, PauseTileAtlasFormat.ByteCount)), "pause UI PNG round-trips all native character bytes");
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseTileAtlasFormat.FileName);
        string path = Path.Combine(overrides, PauseTileAtlasFormat.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        IndexedPngImage image;
        using (var input = File.OpenRead(stockPath)) image = IndexedPng.Read(input, new(MapTileAtlasFormat.Width, MapTileAtlasFormat.Height));
        for (int pixel = 0; pixel < image.Pixels.Length; pixel++) image.Pixels[pixel] = (byte)((image.Pixels[pixel] + 1) % 16);
        using (var output = File.Create(path)) IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "pause-only PNG edit changes content identity");
        var firstMap = new SnesVram(); var editedMap = new SnesVram();
        original.Tiles.LoadTo(new VramStore(firstMap).All.Slice(0 / sizeof(ushort), MapTileAtlasFormat.ByteCount / sizeof(ushort))); edited.Tiles.LoadTo(new VramStore(editedMap).All.Slice(0 / sizeof(ushort), MapTileAtlasFormat.ByteCount / sizeof(ushort)));
        AssertTrue(firstMap.ImageEquals(editedMap), "pause UI override does not alter shared map characters");
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.Maridia, AreaId.Tourian })
        {
            var system = new Bank80SystemState(); system.SetAreaMapAcquired(area);
            var samus = new SamusState { CollectedBeams = SamusBeamFlags.Charge, EquippedBeams = SamusBeamFlags.Charge };
            var nativeSamus = new SamusState { CollectedBeams = samus.CollectedBeams, EquippedBeams = samus.EquippedBeams };
            var native = new PauseMenuState(new PauseMenuEntry { Samus = nativeSamus, System = system, AreaIndex = area, RoomMapPosition = new(10, 10), MapPresentation = original });
            var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = system, AreaIndex = area, RoomMapPosition = new(10, 10), MapPresentation = original });
            int toMap = 2 + PausePageTransitionNativeLength + 1;
            for (int tick = 0; tick < toMap + PausePageTransitionNativeLength + 2; tick++)
            {
                SnesButton pressed = tick == 2 ? SnesButton.R : tick == toMap ? SnesButton.L : 0;
                AssertEqual(native.Step(pressed, pressed), pause.Step(pressed, pressed), "pause PNG migration preserves unpause result");
                AssertEqual(native.ScreenMode, pause.ScreenMode, "pause PNG migration preserves page timing");
                AssertTrue(native.Render().SequenceEqual(pause.Render()), "pause map/equipment transitions match native pixels with artwork reads blocked");
            }
            AssertEqual(PauseScreenMode.Map, pause.ScreenMode, "pause PNG fixture returns to the map page");
            EnterPauseEquipment(pause);
            AssertEqual(PauseScreenMode.Equipment, pause.ScreenMode, "artwork edit fixture reaches equipment page");
            var before = pause.Render();
            var selected = (pause.SelectedCategory, pause.SelectedItem, samus.EquippedBeams, samus.CollectedBeams);
            pause.BindMapPresentation(edited);
            var after = pause.Render();
            AssertTrue(!before.SequenceEqual(after), "pause UI PNG edit reaches actual equipment pixels immediately");
            AssertTrue(after.SequenceEqual(RenderForComparison(pause.CaptureRenderSnapshot())), "edited pause artwork reaches captured rendering");
            AssertEqual(selected, (pause.SelectedCategory, pause.SelectedItem, samus.EquippedBeams, samus.CollectedBeams), "artwork rebind preserves selection and equipment");
            using var captured = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(captured, pause); captured.Position = 0;
            pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(captured);
            pause.BindMapPresentation(original);
            AssertTrue(before.SequenceEqual(pause.Render()), "restored pause uses current PNG without changing animation phase");
        }
        File.WriteAllText(path, "invalid PNG");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "corrupt pause override is not silently replaced");
        using (var output = File.Create(path)) IndexedPng.Write(output, new(1, 1), [0], image.Palette);
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "pause PNG dimensions enforced");
        image.Pixels[0] = 16;
        using (var output = File.Create(path)) IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, SnesGraphics.DiagnosticPalette(17));
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "pause PNG indexes limited to native 4bpp range");
        File.WriteAllBytes(path, stockBytes);
        // This is isolated generated fixture stock, never an installed player file.
        try
        {
            File.Delete(stockPath);
            AssertThrows<IOException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "override does not hide missing stock provenance");
            File.WriteAllText(stockPath, "corrupt stock");
            AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "override does not hide corrupted stock provenance");
        }
        finally { File.WriteAllBytes(stockPath, stockBytes); }
        Console.WriteLine("Pause UI atlas: exact bytes, three-area map/equipment transitions, immediate edited pixels, capture/restore, equipment isolation and strict resource failures pass.");
    }

}
