using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed map atlas bytes, editable PNG pixels, and both map-menu render paths.</summary>
internal static class MapAtlasVerification
{
    /// <summary>Compares stock tile bytes with ROM, then verifies override pixels, identity, rebind, and invalid asset handling.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the installed map tile atlas")]
    internal static void VerifyMapAtlasIntegration(ISnesAddressSpace bus, string stock, string overrides,
        AreaMapPresentationCatalog original, AreaMapCartridgeData[] rules)
    {
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapTileAtlasFormat.SourceAddress, MapTileAtlasFormat.ByteCount);
        var vram = new SnesVram();
        original.Tiles.LoadTo(new VramRegion(vram, PpuMemoryLayout.Vram).Slice(0 / sizeof(ushort), MapTileAtlasFormat.ByteCount / sizeof(ushort)));
        for (int i = 0; i < source.Length; i++) AssertEqual(source[i], vram.ReadByte(i), "stock PNG recompiles exact map tile bytes");
        var guard = NewMapDataGuard(bus, rules);
        var system = new Bank80SystemState();
        system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
        var menu = new FileSelectRoomMapGraphics(system, AreaId.Crateria, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
        var samus = new SamusState();
        var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        var nativePause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        Rgba32[] pauseBefore = pause.Render(), menuBefore = menu.RenderBackgrounds(new(0, 0)).ToArray();
        AssertTrue(pauseBefore.SequenceEqual(nativePause.Render()), "installed stock atlas gives exact pause pixels with artwork ROM blocked");
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, MapTileAtlasFormat.FileName);
        IndexedPngImage image;
        using (var input = File.OpenRead(Path.Combine(stock, MapTileAtlasFormat.FileName)))
            image = IndexedPng.Read(input, new(MapTileAtlasFormat.Width, MapTileAtlasFormat.Height));
        for (int i = 0; i < image.Pixels.Length; i++) image.Pixels[i] = (byte)((image.Pixels[i] + 1) % MapTileAtlasFormat.ColorCount);
        using (var output = File.Create(replacement)) IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        byte[] replacementBytes = File.ReadAllBytes(replacement);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "PNG edit changes selected catalog identity");
        edited.Tiles.LoadTo(new VramRegion(vram, PpuMemoryLayout.Vram).Slice(0 / sizeof(ushort), MapTileAtlasFormat.ByteCount / sizeof(ushort)));
        byte[] planar = Enumerable.Range(0, source.Length).Select(vram.ReadByte).ToArray();
        byte[] decoded = SnesGraphics.DecodePlanarTiles(planar, 4, MapTileAtlasFormat.TileColumns, out _);
        AssertTrue(decoded.SequenceEqual(image.Pixels), "every edited PNG pixel reaches compiled tile data");
        menu.BindFixtureArtwork(edited);
        pause.BindFixtureArtwork(edited);
        AssertTrue(!menuBefore.SequenceEqual(menu.RenderBackgrounds(new(0, 0))), "PNG artwork changes rendered file-select map");
        AssertTrue(!pauseBefore.SequenceEqual(pause.Render()), "PNG artwork changes rendered pause map");
        bool InnerMapChanged(Rgba32[] before, Rgba32[] after) => Enumerable.Range(64, 96)
            .Any(y => Enumerable.Range(64, 128).Any(x => before[y * 256 + x] != after[y * 256 + x]));
        AssertTrue(InnerMapChanged(menuBefore, menu.RenderBackgrounds(new(0, 0))), "file-select artwork change reaches map interior, not only frame chrome");
        AssertTrue(InnerMapChanged(pauseBefore, pause.Render()), "pause artwork change reaches map interior, not only frame chrome");
        using var state = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(state, menu);
        state.Position = 0;
        menu = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<FileSelectRoomMapGraphics>(state);
        menu.BindFixtureArtwork(original);
        pause.BindFixtureArtwork(original);
        AssertTrue(menuBefore.SequenceEqual(menu.RenderBackgrounds(new(0, 0))), "restored menu replaces stale modified atlas pixels");
        AssertTrue(pauseBefore.SequenceEqual(pause.Render()), "pause rebind restores exact stock atlas pixels");
        string rebuilt = Path.Combine(overrides, "stock-rebuilt");
        SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, AreaMapPresentationCatalog.Load(rebuilt, overrides).ContentIdentity, "PNG override survives stock re-extraction");
        AssertTrue(replacementBytes.SequenceEqual(File.ReadAllBytes(replacement)), "PNG override bytes never rewritten");
        File.WriteAllText(replacement, "invalid PNG");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "bad PNG override fails without stock fallback");
        AssertEqual("invalid PNG", File.ReadAllText(replacement), "bad PNG override retained for repair");
        using (var output = File.Create(replacement)) IndexedPng.Write(output, new(1, 1), [0], image.Colours);
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "wrong native atlas dimensions rejected");
        File.WriteAllBytes(replacement, replacementBytes);
        var tooManyColors = SnesGraphics.DiagnosticPalette(17);
        image.Pixels[0] = 16;
        using (var output = File.Create(replacement)) IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, tooManyColors);
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "PNG index outside 4-bpp atlas rejected");
        File.WriteAllBytes(replacement, replacementBytes);
        string rebuiltAtlas = Path.Combine(rebuilt, MapTileAtlasFormat.FileName);
        File.Delete(rebuiltAtlas);
        AssertThrows<FileNotFoundException>(() => AreaMapPresentationCatalog.Load(rebuilt, overrides), "missing stock atlas rejected even with valid override");
        Console.WriteLine("Map PNG atlas: exact stock bytes/pause pixels; PNG edits reach both rendered menus, rebind and survive stock replacement.");
    }
}
