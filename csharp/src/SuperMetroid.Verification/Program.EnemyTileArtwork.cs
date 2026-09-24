using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEnemyTileArtwork()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Enemy tile artwork: private ROM absent; retail extraction skipped.");
            return;
        }
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        string directory = Path.Combine(Path.GetFullPath("csharp/test-temp"),
            "enemy-tiles-" + Guid.NewGuid().ToString("N"));
        try
        {
            EnemyTileArtworkFiles.Extract(bus, directory, SupportedCartridge.Sha256);
            EnemyTileArtworkCatalog stock = EnemyTileArtworkFiles.Load(directory, null);
            string[] files = Directory.GetFiles(directory, "enemy-????-tiles.png");
            AssertEqual(EnemyTileArtworkFormat.RetailDefinitionCount, files.Length,
                "one indexed PNG per distinct retail enemy graphics definition");
            foreach (string file in files)
            {
                ushort pointer = Convert.ToUInt16(Path.GetFileName(file).Substring(6, 4), 16);
                RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, pointer);
                int byteCount = definition.TileDataSize & 0x7fff;
                byte[] native = RomDataReader.ReadFixedBank(bus, definition.TileDataAddress, byteCount);
                var vram = new SnesVram();
                stock.LoadTo(pointer, byteCount, vram, 0);
                AssertTrue(vram.Bytes[..byteCount].SequenceEqual(native),
                    $"enemy ${pointer:X4} PNG preserves every native tile byte");
            }

            string editedFile = files[0];
            ushort editedPointer = Convert.ToUInt16(Path.GetFileName(editedFile).Substring(6, 4), 16);
            RoomEnemyDefinition editedDefinition = RoomEnemySystem.ReadDefinition(bus, editedPointer);
            int editedByteCount = editedDefinition.TileDataSize & 0x7fff;
            int tileCount = editedByteCount / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(tileCount, RoomCharacterAtlasFormat.TileColumns);
            int rows = (tileCount + columns - 1) / columns;
            using var stockPng = new MemoryStream(File.ReadAllBytes(editedFile), writable: false);
            IndexedPngImage image = IndexedPng.Read(stockPng, columns * 8, rows * 8);
            image.Pixels[0] ^= 1;
            string overrideDirectory = Path.Combine(directory, "overrides");
            Directory.CreateDirectory(overrideDirectory);
            string overridePath = Path.Combine(overrideDirectory, Path.GetFileName(editedFile));
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
            var stockVram = new SnesVram();
            var editedVram = new SnesVram();
            stock.LoadTo(editedPointer, editedByteCount, stockVram, 0);
            edited.LoadTo(editedPointer, editedByteCount, editedVram, 0);
            AssertEqual((byte)(stockVram.ReadByte(0) ^ 0x80), editedVram.ReadByte(0),
                "edited enemy PNG changes its first indexed pixel at the native tile location");
            AssertTrue(stockVram.Bytes[1..].SequenceEqual(editedVram.Bytes[1..]),
                "enemy PNG edit leaves all neighboring VRAM bytes unchanged");
            var reloadedVram = new SnesVram();
            EnemyTileArtworkFiles.Load(directory, overrideDirectory)
                .LoadTo(editedPointer, editedByteCount, reloadedVram, 0);
            AssertTrue(reloadedVram.Bytes.SequenceEqual(editedVram.Bytes),
                "enemy override survives content reload without modifying stock PNG");

            byte[] original = File.ReadAllBytes(editedFile);
            File.WriteAllBytes(editedFile, new byte[] { 0 });
            AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, null),
                "corrupt stock enemy artwork fails its manifest hash");
            File.WriteAllBytes(editedFile, original);
            File.WriteAllBytes(overridePath, new byte[] { 0 });
            AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
                "malformed enemy override fails loudly");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
        Console.WriteLine("  Enemy tile artwork: 122 retail sheets, exact bytes, pixel override, persistence, and invalid resources pass.");
    }
}
