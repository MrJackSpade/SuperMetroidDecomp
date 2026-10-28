using SuperMetroid.Core.Assets;
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
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Checks extracted room graphics-set palettes and their installation into room CGRAM.</summary>
internal static class RoomStaticPalettesVerification
{
    /// <summary>Compares all distinct native palettes and checks room loading plus a single-color JSON edit.</summary>
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
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int source = RoomTilesets.Get(graphicsSet).PaletteAddress;
            string name = RoomStaticPaletteFormat.SourceFileName(source);
            if (compiled.ContainsKey(source)) continue;
            RoomStaticPalette palette = RoomStaticPaletteFile.Read(
                new MemoryStream(files[name], writable: false));
            byte[] native = RomDataReader.Decompress(bus, source);
            var expected = new SnesCgram();
            var actual = new SnesCgram();
            expected.LoadBytes(native.AsSpan(0, RoomStaticPaletteFormat.ByteCount));
            new CgramPalettes(actual).Background.LoadPortBytes(palette.PackedColours.Span);
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)),
                $"graphics-set palette ${source:X6} keeps all 128 native CGRAM colors");
            compiled.Add(source, palette);
        }
        AssertEqual(compiled.Count, files.Count,
            "one extracted room palette per distinct graphics-set color source");
        var catalog = new RoomStaticPalettes(compiled.ToDictionary(pair => SuperMetroid.Core.Rooms.RoomTilesetNative.PaletteSetAt(pair.Key), pair => pair.Value));
        foreach (ushort roomPointer in new ushort[] { 0x91f8, 0xdf8d })
        {
            SelectedRoom header = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, roomPointer);
            LoadedRoomAssets native = LoadFixtureRoomAssets(header);
            LoadedRoomAssets installed = LoadFixtureRoomAssets(header, paletteArt: catalog);
            var nativeCgram = new GameplayColours(new SpriteColourSets());
            var installedCgram = new GameplayColours(new SpriteColourSets());
            native.LoadGraphics(BgArtworkFixtures.For(new SnesVram()), nativeCgram.BackgroundSets);
            installed.LoadGraphics(BgArtworkFixtures.For(new SnesVram()), installedCgram.BackgroundSets);
            AssertTrue(new CgramPalettes(nativeCgram.AsPaletteColors()).ImageEquals(new CgramPalettes(installedCgram.AsPaletteColors())),
                $"room $8F:{roomPointer:X4} loads installed palette without source reads");
        }

        SelectedRoom landing = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0x91f8);
        int selectedSource = RoomTilesets.Get(landing.State.GraphicsSet).PaletteAddress;
        string selectedName = RoomStaticPaletteFormat.SourceFileName(selectedSource);
        JsonNode edited = JsonNode.Parse(files[selectedName])
            ?? throw new InvalidDataException("Extracted room palette JSON is empty.");
        JsonNode color = edited.Required("colors").Required(0);
        int originalRed = color.Required("red").GetValue<int>();
        color["red"] = originalRed ^ 1;
        RoomStaticPalette replacement = RoomStaticPaletteFile.Read(
            new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited)));
        var stockCgram = new SnesCgram();
        var editedCgram = new SnesCgram();
        new CgramPalettes(stockCgram).Background.LoadPortBytes(catalog.GetAt(selectedSource).PackedColours.Span);
        new CgramPalettes(editedCgram).Background.LoadPortBytes(replacement.PackedColours.Span);
        AssertTrue(stockCgram.Colors[0] != editedCgram.Colors[0] &&
            stockCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]
                .SequenceEqual(editedCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]),
            "editing one RGB5 component changes only its selected runtime CGRAM color");
        compiled[selectedSource] = replacement;
        LoadedRoomAssets editedRoom = LoadFixtureRoomAssets(landing,
            paletteArt: new RoomStaticPalettes(compiled.ToDictionary(pair => SuperMetroid.Core.Rooms.RoomTilesetNative.PaletteSetAt(pair.Key), pair => pair.Value)));
        var editedRoomCgram = new GameplayColours(new SpriteColourSets());
        editedRoom.LoadGraphics(BgArtworkFixtures.For(new SnesVram()), editedRoomCgram.BackgroundSets);
        AssertEqual(editedCgram.Colors[0], editedRoomCgram.Colors[0],
            "edited RGB5 room palette changes real room-loader CGRAM");

        color["red"] = 32;
        byte[] invalid = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited);
        AssertThrows<InvalidDataException>(() => RoomStaticPaletteFile.Read(
                new MemoryStream(invalid, writable: false)),
            "room palette rejects out-of-range RGB5 components");
        Console.WriteLine($"  Room static palettes: {files.Count} distinct palettes cover all " +
            $"{RoomTilesetDefinitions.Count} graphics sets; Landing Site/Ceres load without source reads, " +
            "and RGB5 edits reach room CGRAM.");
    }

    /// <summary>Runs the room static palette extraction and installation check.</summary>
    [VerificationCommand("--room-static-palettes")]
    internal static int RoomStaticPalettesCommand()
    {
        Suite(nameof(VerifyRoomStaticPaletteExtraction), () => VerifyRoomStaticPaletteExtraction());
        return 0;
    }
}
