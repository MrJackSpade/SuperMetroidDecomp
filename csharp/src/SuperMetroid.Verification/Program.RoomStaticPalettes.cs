using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRoomStaticPaletteExtraction()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Room static palettes: private ROM absent; retail audit skipped.");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        IReadOnlyDictionary<string, byte[]> files = RoomStaticPaletteExtractor.Extract(bus);
        var compiled = new Dictionary<int, RoomStaticPalette>();
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int source = RoomTilesetDefinitions.Get(graphicsSet).PaletteAddress;
            string name = RoomStaticPaletteFormat.SourceFileName(source);
            if (compiled.ContainsKey(source)) continue;
            RoomStaticPalette palette = RoomStaticPalette.Load(
                new MemoryStream(files[name], writable: false));
            byte[] native = RomDataReader.Decompress(bus, source);
            var expected = new SnesCgram();
            var actual = new SnesCgram();
            expected.LoadBytes(native.AsSpan(0, RoomAssetRomData.GraphicsLayout.BackgroundPaletteByteCount));
            palette.LoadTo(actual);
            AssertTrue(expected.Colors.SequenceEqual(actual.Colors),
                $"graphics-set palette ${source:X6} keeps all 128 native CGRAM colors");
            compiled.Add(source, palette);
        }
        AssertEqual(compiled.Count, files.Count,
            "one extracted room palette per distinct graphics-set color source");
        var catalog = new RoomStaticPaletteCatalog(compiled);
        foreach (ushort roomPointer in new ushort[] { 0x91f8, 0xdf8d })
        {
            CartridgeRoomHeader header = CartridgeRoomHeader.Load(bus, roomPointer);
            int source = RoomTilesetDefinitions.Get(header.State.GraphicsSet).PaletteAddress;
            CartridgeRoomAssets native = CartridgeRoomAssets.Load(bus, header);
            CartridgeRoomAssets installed = CartridgeRoomAssets.Load(
                new RoomBasePaletteReadGuard(bus, source), header, paletteArt: catalog);
            var nativeCgram = new SnesCgram();
            var installedCgram = new SnesCgram();
            native.LoadGraphics(new SnesVram(), nativeCgram);
            installed.LoadGraphics(new SnesVram(), installedCgram);
            AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                $"room $8F:{roomPointer:X4} loads installed palette without source reads");
        }

        CartridgeRoomHeader landing = CartridgeRoomHeader.Load(bus, 0x91f8);
        int selectedSource = RoomTilesetDefinitions.Get(landing.State.GraphicsSet).PaletteAddress;
        string selectedName = RoomStaticPaletteFormat.SourceFileName(selectedSource);
        JsonNode edited = JsonNode.Parse(files[selectedName])
            ?? throw new InvalidDataException("Extracted room palette JSON is empty.");
        JsonNode color = edited["colors"]![0]!;
        int originalRed = color["red"]!.GetValue<int>();
        color["red"] = originalRed ^ 1;
        RoomStaticPalette replacement = RoomStaticPalette.Load(
            new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited)));
        var stockCgram = new SnesCgram();
        var editedCgram = new SnesCgram();
        catalog.Get(selectedSource).LoadTo(stockCgram);
        replacement.LoadTo(editedCgram);
        AssertTrue(stockCgram.Colors[0] != editedCgram.Colors[0] &&
            stockCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]
                .SequenceEqual(editedCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]),
            "editing one RGB5 component changes only its selected runtime CGRAM color");
        compiled[selectedSource] = replacement;
        CartridgeRoomAssets editedRoom = CartridgeRoomAssets.Load(
            new RoomBasePaletteReadGuard(bus, selectedSource), landing,
            paletteArt: new RoomStaticPaletteCatalog(compiled));
        var editedRoomCgram = new SnesCgram();
        editedRoom.LoadGraphics(new SnesVram(), editedRoomCgram);
        AssertEqual(editedCgram.Colors[0], editedRoomCgram.Colors[0],
            "edited RGB5 room palette changes real room-loader CGRAM");

        color["red"] = 32;
        byte[] invalid = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited);
        AssertThrows<InvalidDataException>(() => RoomStaticPalette.Load(
                new MemoryStream(invalid, writable: false)),
            "room palette rejects out-of-range RGB5 components");
        Console.WriteLine($"  Room static palettes: {files.Count} distinct palettes cover all " +
            $"{RoomTilesetDefinitions.Count} graphics sets; Landing Site/Ceres load without source reads, " +
            "and RGB5 edits reach room CGRAM.");
    }

    private sealed class RoomBasePaletteReadGuard(ISnesAddressSpace source, int paletteSource)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address == paletteSource)
                throw new InvalidOperationException(
                    $"Installed room loader reread base palette source ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
