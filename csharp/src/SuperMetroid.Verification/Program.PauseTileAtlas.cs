using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyPauseTileAtlas(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var vram = new SnesVram(); original.PauseTiles.LoadTo(vram, PauseTileAtlasFormat.DestinationByte);
        AssertTrue(vram.Bytes.Slice(PauseTileAtlasFormat.DestinationByte, PauseTileAtlasFormat.ByteCount).SequenceEqual(
            RomDataReader.ReadFixedBank(bus, PauseTileAtlasFormat.SourceAddress, PauseTileAtlasFormat.ByteCount)), "pause UI PNG round-trips all native character bytes");
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseTileAtlasFormat.FileName);
        string path = Path.Combine(overrides, PauseTileAtlasFormat.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        IndexedPngImage image;
        using (var input = File.OpenRead(stockPath)) image = IndexedPng.Read(input, MapTileAtlasFormat.Width, MapTileAtlasFormat.Height);
        for (int pixel = 0; pixel < image.Pixels.Length; pixel++) image.Pixels[pixel] = (byte)((image.Pixels[pixel] + 1) % 16);
        using (var output = File.Create(path)) IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "pause-only PNG edit changes content identity");
        var firstMap = new SnesVram(); var editedMap = new SnesVram();
        original.Tiles.LoadTo(firstMap, 0); edited.Tiles.LoadTo(editedMap, 0);
        AssertTrue(firstMap.Bytes.SequenceEqual(editedMap.Bytes), "pause UI override does not alter shared map characters");
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.Maridia, AreaId.Tourian })
        {
            var system = new Bank80SystemState(); system.SetAreaMapAcquired(area);
            var samus = new SamusState { CollectedBeams = (ushort)SamusBeamFlags.Charge, EquippedBeams = (ushort)SamusBeamFlags.Charge };
            var nativeSamus = new SamusState { CollectedBeams = samus.CollectedBeams, EquippedBeams = samus.EquippedBeams };
            var native = new PauseMenuState(bus, nativeSamus, system, area, 10, 10);
            var pause = new PauseMenuState(new PauseArtworkReadGuard(bus), samus, system, area, 10, 10, mapPresentation: original);
            for (int tick = 0; tick < 72; tick++)
            {
                ushort pressed = tick == 2 ? (ushort)SnesButton.R : tick == 38 ? (ushort)SnesButton.L : (ushort)0;
                AssertEqual(native.Step(pressed, pressed), pause.Step(pressed, pressed), "pause PNG migration preserves unpause result");
                AssertEqual(native.ScreenMode, pause.ScreenMode, "pause PNG migration preserves page timing");
                AssertTrue(native.Render().AsSpan().SequenceEqual(pause.Render()), "pause map/equipment transitions match native pixels with artwork reads blocked");
            }
            pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
            for (int tick = 0; tick < 32; tick++) pause.Step(0, 0);
            AssertEqual(1, pause.ScreenMode, "artwork edit fixture reaches equipment page");
            var before = pause.Render();
            var selected = (pause.SelectedCategory, pause.SelectedItem, samus.EquippedBeams, samus.CollectedBeams);
            pause.BindMapPresentation(edited);
            var after = pause.Render();
            AssertTrue(!before.AsSpan().SequenceEqual(after), "pause UI PNG edit reaches actual equipment pixels immediately");
            AssertTrue(after.AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(pause.CaptureRenderSnapshot())), "edited pause artwork reaches captured rendering");
            AssertEqual(selected, (pause.SelectedCategory, pause.SelectedItem, samus.EquippedBeams, samus.CollectedBeams), "artwork rebind preserves selection and equipment");
            using var captured = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(captured, pause); captured.Position = 0;
            pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(captured);
            pause.BindMapPresentation(original);
            AssertTrue(before.AsSpan().SequenceEqual(pause.Render()), "restored pause uses current PNG without changing animation phase");
        }
        File.WriteAllText(path, "invalid PNG");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "corrupt pause override is not silently replaced");
        using (var output = File.Create(path)) IndexedPng.Write(output, 1, 1, [0], image.Palette);
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "pause PNG dimensions enforced");
        image.Pixels[0] = 16;
        using (var output = File.Create(path)) IndexedPng.Write(output, image.Width, image.Height, image.Pixels, SnesGraphics.DiagnosticPalette(17));
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

    private sealed class PauseArtworkReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => (uint)(address - PauseTileAtlasFormat.SourceAddress) < PauseTileAtlasFormat.ByteCount
            ? throw new InvalidOperationException($"Installed pause read UI artwork at {address:X6}.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
