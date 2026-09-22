using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Full stock import, live edit, repair and invalid-override contract for room art.</summary>
    private static void VerifyRoomArtworkInstallation(string sourceRom)
    {
        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "room-character-installation-" + Guid.NewGuid().ToString("N")));
        try
        {
            GameInstallation installed = GameAssetInstaller.Install(sourceRom, root);
            AssertTrue(File.Exists(Path.Combine(installed.RoomCharacterDirectory,
                    RoomCharacterArtworkFiles.ManifestFileName)),
                "room-character stock manifest is installed");
            RoomCharacterAtlasCatalog stock = installed.LoadRoomCharacters();
            SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
            CartridgeRoomHeader landing = CartridgeRoomHeader.Load(bus, 0x91f8);
            CartridgeRoomAssets native = CartridgeRoomAssets.Load(bus, landing);
            CartridgeRoomAssets installedRoom = CartridgeRoomAssets.Load(bus, landing, stock);
            AssertTrue(installedRoom.CreCharacters.AsSpan().SequenceEqual(native.CreCharacters) &&
                installedRoom.RoomCharacters.AsSpan().SequenceEqual(native.RoomCharacters),
                "installed stock room PNGs retain Landing Site character bytes");

            int source = native.Tileset.CharacterAddress;
            string name = RoomCharacterAtlasFormat.SourceFileName(source);
            int tileCount = native.RoomCharacters.Length / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
            int rows = (tileCount + columns - 1) / columns;
            IndexedPngImage image;
            using (var stockFile = File.OpenRead(Path.Combine(installed.RoomCharacterDirectory, name)))
                image = IndexedPng.Read(stockFile, columns * 8, rows * 8);
            image.Pixels[0] ^= 1;
            Directory.CreateDirectory(installed.RoomCharacterOverrideDirectory);
            string overridePath = Path.Combine(installed.RoomCharacterOverrideDirectory, name);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            RoomCharacterAtlasCatalog edited = installed.LoadRoomCharacters();
            CartridgeRoomAssets editedRoom = CartridgeRoomAssets.Load(bus, landing, edited);
            AssertTrue(!editedRoom.RoomCharacters.AsSpan().SequenceEqual(native.RoomCharacters),
                "installed PNG override changes the real room-loader output");
            AssertTrue(stock.Get(source).Transfer.Span.SequenceEqual(native.RoomCharacters),
                "room override does not mutate stock artwork already loaded");

            AssertTrue(File.Exists(Path.Combine(installed.RoomPaletteDirectory,
                    RoomStaticPaletteArtworkFiles.ManifestFileName)),
                "room-palette stock manifest is installed");
            RoomStaticPaletteCatalog stockPalettes = installed.LoadRoomPalettes();
            int paletteSource = native.Tileset.PaletteAddress;
            string paletteName = RoomStaticPaletteFormat.SourceFileName(paletteSource);
            string paletteStockPath = Path.Combine(installed.RoomPaletteDirectory, paletteName);
            JsonNode paletteDocument = JsonNode.Parse(File.ReadAllText(paletteStockPath))
                ?? throw new InvalidDataException("Installed room palette JSON is empty.");
            JsonNode firstColor = paletteDocument["colors"]![0]!;
            int stockRed = firstColor["red"]!.GetValue<int>();
            firstColor["red"] = stockRed ^ 1;
            Directory.CreateDirectory(installed.RoomPaletteOverrideDirectory);
            string paletteOverridePath = Path.Combine(installed.RoomPaletteOverrideDirectory, paletteName);
            File.WriteAllText(paletteOverridePath, paletteDocument.ToJsonString());
            RoomStaticPaletteCatalog editedPalettes = installed.LoadRoomPalettes();
            CartridgeRoomAssets editedPaletteRoom = CartridgeRoomAssets.Load(bus, landing,
                paletteArt: editedPalettes);
            var stockCgram = new SnesCgram();
            var editedCgram = new SnesCgram();
            native.LoadGraphics(new SnesVram(), stockCgram);
            editedPaletteRoom.LoadGraphics(new SnesVram(), editedCgram);
            AssertTrue(stockCgram.Colors[0] != editedCgram.Colors[0] &&
                stockCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]
                    .SequenceEqual(editedCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]),
                "installed RGB5 override changes only its room CGRAM color");
            var compiledStockCgram = new SnesCgram();
            stockPalettes.Get(paletteSource).LoadTo(compiledStockCgram);
            AssertTrue(stockCgram.Colors.SequenceEqual(compiledStockCgram.Colors),
                "installed stock room palette matches native room CGRAM");

            // A missing stock resource causes atomic re-extraction from the installed
            // cartridge; the edit remains outside the replaceable game directory.
            File.Delete(Path.Combine(installed.RoomCharacterDirectory, name));
            File.Delete(paletteStockPath);
            GameInstallation repaired = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Installed room-art repair lost its ROM.");
            AssertTrue(File.Exists(overridePath), "room-character override survives stock repair");
            AssertTrue(File.Exists(Path.Combine(repaired.RoomCharacterDirectory, name)),
                "room-character stock sheet is restored by repair");
            AssertTrue(File.Exists(paletteOverridePath) &&
                File.Exists(Path.Combine(repaired.RoomPaletteDirectory, paletteName)),
                "room-palette stock is restored while its override survives repair");
            CartridgeRoomAssets repairedEdited = CartridgeRoomAssets.Load(bus, landing,
                repaired.LoadRoomCharacters());
            AssertTrue(repairedEdited.RoomCharacters.AsSpan().SequenceEqual(editedRoom.RoomCharacters),
                "repaired installation retains the selected user room artwork");
            CartridgeRoomAssets repairedPaletteRoom = CartridgeRoomAssets.Load(bus, landing,
                paletteArt: repaired.LoadRoomPalettes());
            var repairedCgram = new SnesCgram();
            repairedPaletteRoom.LoadGraphics(new SnesVram(), repairedCgram);
            AssertTrue(repairedCgram.Colors.SequenceEqual(editedCgram.Colors),
                "repaired installation retains the selected user room palette");

            File.WriteAllBytes(overridePath, "invalid indexed PNG"u8.ToArray());
            AssertThrows<InvalidDataException>(() => repaired.LoadRoomCharacters(),
                "invalid room-art override fails loudly instead of reverting to stock");
            File.Delete(overridePath);
            File.WriteAllText(paletteOverridePath, "invalid RGB5 JSON");
            AssertThrows<InvalidDataException>(() => repaired.LoadRoomPalettes(),
                "invalid room-palette override fails loudly instead of reverting to stock");

            Console.WriteLine("  Room artwork installation: PNG and RGB5 stock import, live edits, " +
                "repair preservation and invalid-override rejection verified.");
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
