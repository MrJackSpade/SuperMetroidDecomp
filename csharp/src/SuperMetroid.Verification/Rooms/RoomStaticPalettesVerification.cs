using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Checks extracted room graphics-set palettes and their installation into room CGRAM.</summary>
internal static class RoomStaticPalettesVerification
{
    /// <summary>Compares all distinct native palettes and checks room loading and RGB5 component rejection.</summary>
    internal static void VerifyRoomStaticPaletteExtraction()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Room static palettes: private ROM absent; retail audit skipped.");
            return;
        }

        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        IReadOnlyDictionary<string, byte[]> files = RoomStaticPaletteExtractor.Extract(bus);
        var compiled = new Dictionary<int, RoomStaticPalette>();
        for (byte graphicsSet = 0; graphicsSet < RoomGraphicsSetCodec.Count; graphicsSet++)
        {
            int source = RoomTilesets.Get(RoomGraphicsSetCodec.Decode(graphicsSet)).PaletteAddress;
            string name = RoomStaticPaletteFormat.SourceFileName(source);
            if (compiled.ContainsKey(source)) continue;
            RoomStaticPalette palette = RoomStaticPaletteFile.Read(
                new MemoryStream(files[name], writable: false));
            byte[] native = RomDataReader.Decompress(bus, source);
            var expected = new SnesCgram();
            var actual = new SnesCgram();
            expected.LoadBytes(native.AsSpan(0, RoomStaticPaletteFormat.ByteCount));
            new CgramPalettes(actual).Background.LoadPortBytes(Packed(RoomStaticPaletteImage.Colours(palette)));
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)),
                $"graphics-set palette ${source:X6} keeps all 128 native CGRAM colors");
            compiled.Add(source, palette);
        }
        AssertEqual(compiled.Count, files.Count,
            "one extracted room palette per distinct graphics-set color source");

        // `colours` packed as little-endian colour words, two bytes each, as the cartridge's room load transfers them.
        static byte[] Packed(RgbColor[] colours)
        {
            var packed = new byte[colours.Length * ColourWordCodec.ByteCount];
            for (int index = 0; index < colours.Length; index++)
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(packed.AsSpan(index * ColourWordCodec.ByteCount), colours[index].ToWord());
            return packed;
        }
        foreach (ushort roomPointer in new ushort[] { 0x91f8, 0xdf8d })
        {
            SelectedRoom header = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, roomPointer);
            // The room loads with source reads blocked; its pictures carry the palette's colours, which only the import
            // reads, so the load colours nothing (#1512).
            LoadedRoomAssets installed = LoadFixtureRoomAssets(header);
            AssertEqual(RoomTilesets.Get(header.State.GraphicsSet), installed.Tileset,
                $"room $8F:{roomPointer:X4} loads installed palette without source reads");
        }

        SelectedRoom landing = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0x91f8);
        int selectedSource = RoomTilesets.Get(landing.State.GraphicsSet).PaletteAddress;
        string selectedName = RoomStaticPaletteFormat.SourceFileName(selectedSource);
        JsonNode malformed = JsonNode.Parse(files[selectedName])
            ?? throw new InvalidDataException("Extracted room palette JSON is empty.");
        malformed.Required("colors").Required(0)["red"] = 32;
        byte[] invalid = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(malformed);
        AssertThrows<InvalidDataException>(() => RoomStaticPaletteFile.Read(
                new MemoryStream(invalid, writable: false)),
            "room palette rejects out-of-range RGB5 components");
        Console.WriteLine($"  Room static palettes: {files.Count} distinct palettes cover all " +
            $"{RoomGraphicsSetCodec.Count} graphics sets; Landing Site/Ceres load without source reads, " +
            "and out-of-range RGB5 components are rejected.");
    }

    /// <summary>Runs the room static palette extraction and installation check.</summary>
    [VerificationCommand("--room-static-palettes")]
    internal static int RoomStaticPalettesCommand()
    {
        Suite(nameof(VerifyRoomStaticPaletteExtraction), () => VerifyRoomStaticPaletteExtraction());
        return 0;
    }
}
