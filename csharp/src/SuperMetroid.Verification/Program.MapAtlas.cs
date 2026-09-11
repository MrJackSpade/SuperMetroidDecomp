using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyMapAtlasIntegration(ISnesAddressSpace bus, string stock, string overrides,
        AreaMapPresentationCatalog original, AreaMapCartridgeData[] rules)
    {
        byte[] source = RomDataReader.ReadFixedBank(bus, MapTileAtlasFormat.SourceAddress, MapTileAtlasFormat.ByteCount);
        var vram = new SnesVram();
        original.Tiles.LoadTo(vram, 0);
        for (int i = 0; i < source.Length; i++) AssertEqual(source[i], vram.ReadByte(i), "stock PNG recompiles exact map tile bytes");
        var guard = new MapDataGuard(bus, rules);
        var system = new Bank80SystemState();
        system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
        var menu = new FileSelectRoomMapGraphics(guard, system, AreaId.Crateria, mapPresentation: original);
        var samus = new SamusState();
        var pause = new PauseMenuState(guard, samus, system, AreaId.Crateria, 0, 0, mapPresentation: original);
        var nativePause = new PauseMenuState(bus, samus, system, AreaId.Crateria, 0, 0);
        Rgba32[] pauseBefore = pause.Render(), menuBefore = menu.RenderBackgrounds(0, 0);
        AssertTrue(pauseBefore.AsSpan().SequenceEqual(nativePause.Render()), "installed stock atlas gives exact pause pixels with artwork ROM blocked");
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, MapTileAtlasFormat.FileName);
        IndexedPngImage image;
        using (var input = File.OpenRead(Path.Combine(stock, MapTileAtlasFormat.FileName)))
            image = IndexedPng.Read(input, MapTileAtlasFormat.Width, MapTileAtlasFormat.Height);
        for (int i = 0; i < image.Pixels.Length; i++) image.Pixels[i] = (byte)((image.Pixels[i] + 1) % MapTileAtlasFormat.ColorCount);
        using (var output = File.Create(replacement)) IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        byte[] replacementBytes = File.ReadAllBytes(replacement);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "PNG edit changes selected catalog identity");
        edited.Tiles.LoadTo(vram, 0);
        byte[] planar = Enumerable.Range(0, source.Length).Select(vram.ReadByte).ToArray();
        byte[] decoded = SnesGraphics.DecodePlanarTiles(planar, 4, MapTileAtlasFormat.TileColumns, out _, out _);
        AssertTrue(decoded.AsSpan().SequenceEqual(image.Pixels), "every edited PNG pixel reaches compiled tile data");
        menu.BindMapPresentation(edited);
        pause.BindMapPresentation(edited);
        AssertTrue(!menuBefore.AsSpan().SequenceEqual(menu.RenderBackgrounds(0, 0)), "PNG artwork changes rendered file-select map");
        AssertTrue(!pauseBefore.AsSpan().SequenceEqual(pause.Render()), "PNG artwork changes rendered pause map");
        bool InnerMapChanged(Rgba32[] before, Rgba32[] after) => Enumerable.Range(64, 96)
            .Any(y => Enumerable.Range(64, 128).Any(x => before[y * 256 + x] != after[y * 256 + x]));
        AssertTrue(InnerMapChanged(menuBefore, menu.RenderBackgrounds(0, 0)), "file-select artwork change reaches map interior, not only frame chrome");
        AssertTrue(InnerMapChanged(pauseBefore, pause.Render()), "pause artwork change reaches map interior, not only frame chrome");
        using var state = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(state, menu);
        state.Position = 0;
        menu = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<FileSelectRoomMapGraphics>(state);
        menu.BindMapPresentation(original);
        pause.BindMapPresentation(original);
        AssertTrue(menuBefore.AsSpan().SequenceEqual(menu.RenderBackgrounds(0, 0)), "restored menu replaces stale modified atlas pixels");
        AssertTrue(pauseBefore.AsSpan().SequenceEqual(pause.Render()), "pause rebind restores exact stock atlas pixels");
        string rebuilt = Path.Combine(overrides, "stock-rebuilt");
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, AreaMapPresentationCatalog.Load(rebuilt, overrides).ContentIdentity, "PNG override survives stock re-extraction");
        AssertTrue(replacementBytes.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)), "PNG override bytes never rewritten");
        File.WriteAllText(replacement, "invalid PNG");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "bad PNG override fails without stock fallback");
        AssertEqual("invalid PNG", File.ReadAllText(replacement), "bad PNG override retained for repair");
        using (var output = File.Create(replacement)) IndexedPng.Write(output, 1, 1, [0], image.Palette);
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "wrong native atlas dimensions rejected");
        File.WriteAllBytes(replacement, replacementBytes);
        var tooManyColors = SnesGraphics.DiagnosticPalette(17);
        image.Pixels[0] = 16;
        using (var output = File.Create(replacement)) IndexedPng.Write(output, image.Width, image.Height, image.Pixels, tooManyColors);
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "PNG index outside 4-bpp atlas rejected");
        File.WriteAllBytes(replacement, replacementBytes);
        string rebuiltAtlas = Path.Combine(rebuilt, MapTileAtlasFormat.FileName);
        File.Delete(rebuiltAtlas);
        AssertThrows<FileNotFoundException>(() => AreaMapPresentationCatalog.Load(rebuilt, overrides), "missing stock atlas rejected even with valid override");
        Console.WriteLine("Map PNG atlas: exact stock bytes/pause pixels; PNG edits reach both rendered menus, rebind and survive stock replacement.");
    }
}
