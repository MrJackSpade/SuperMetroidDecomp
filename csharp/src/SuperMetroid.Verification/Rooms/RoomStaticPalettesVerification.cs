using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;

/// <summary>Checks extracted room graphics-set palettes and their installation into room CGRAM.</summary>
internal static class RoomStaticPalettesVerification
{
    /// <summary>Compares all distinct native palettes and checks room loading plus a single-color JSON edit.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for compressed room base palettes")]
    internal static void VerifyRoomStaticPaletteExtraction()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Room static palettes: private ROM absent; retail audit skipped.");
            return;
        }

        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
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
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)),
                $"graphics-set palette ${source:X6} keeps all 128 native CGRAM colors");
            compiled.Add(source, palette);
        }
        AssertEqual(compiled.Count, files.Count,
            "one extracted room palette per distinct graphics-set color source");
        var catalog = new RoomStaticPaletteCatalog(compiled);
        foreach (ushort roomPointer in new ushort[] { 0x91f8, 0xdf8d })
        {
            CartridgeRoomHeader header = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(bus, roomPointer);
            int source = RoomTilesetDefinitions.Get(header.State.GraphicsSet).PaletteAddress;
            CartridgeRoomAssets native = LoadFixtureRoomAssets(bus, header);
            CartridgeRoomAssets installed = LoadFixtureRoomAssets(
                NewRoomBasePaletteReadGuard(bus, source), header, paletteArt: catalog);
            var nativeCgram = new SnesCgram();
            var installedCgram = new SnesCgram();
            native.LoadGraphics(new SnesVram(), nativeCgram);
            installed.LoadGraphics(new SnesVram(), installedCgram);
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                $"room $8F:{roomPointer:X4} loads installed palette without source reads");
        }

        CartridgeRoomHeader landing = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(bus, 0x91f8);
        int selectedSource = RoomTilesetDefinitions.Get(landing.State.GraphicsSet).PaletteAddress;
        string selectedName = RoomStaticPaletteFormat.SourceFileName(selectedSource);
        JsonNode edited = JsonNode.Parse(files[selectedName])
            ?? throw new InvalidDataException("Extracted room palette JSON is empty.");
        JsonNode color = edited.Required("colors").Required(0);
        int originalRed = color.Required("red").GetValue<int>();
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
        CartridgeRoomAssets editedRoom = LoadFixtureRoomAssets(
            NewRoomBasePaletteReadGuard(bus, selectedSource), landing,
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

    /// <summary>Blocks a compressed palette source while its installed room graphics load.</summary>
    private static CartridgeReadGuard NewRoomBasePaletteReadGuard(ISnesAddressSpace source, int paletteSource) =>
        new CartridgeReadGuard(source).Forbid(address => address == paletteSource,
            address => $"Installed room loader reread base palette source ${address:X6}.");

    /// <summary>Runs the room static palette extraction and installation check.</summary>
    [VerificationCommand("--room-static-palettes")]
    internal static int RoomStaticPalettesCommand()
    {
        Suite(nameof(VerifyRoomStaticPaletteExtraction), () => VerifyRoomStaticPaletteExtraction());
        return 0;
    }
}
