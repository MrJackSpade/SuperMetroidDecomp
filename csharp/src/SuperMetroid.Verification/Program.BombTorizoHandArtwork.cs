using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Confirm #1162's exact PLM-to-installed-art-to-NMI handoff, not a room sweep.</summary>
    private static void VerifyBombTorizoHandArtwork()
    {
        var importer = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        string directory = Path.Combine(Path.GetFullPath("csharp/test-temp"),
            "bomb-torizo-hand-artwork-" + Guid.NewGuid().ToString("N"));
        try
        {
            EnemyTileArtworkFiles.Extract(importer, directory, SupportedCartridge.Sha256);
            EnemyTileArtworkCatalog stock = EnemyTileArtworkFiles.Load(directory, null);
            TorizoInstructionTileSheetDefinition page = TorizoInstructionVramArtworkDefinitions.ChozoDebris;
            AssertTrue(stock.TryResolve(page.SourceAddress, page.ByteCount, out ReadOnlyMemory<byte> bytes),
                "installed Chozo debris page exists");
            for (int index = 0; index < bytes.Length; index++)
                AssertEqual(importer.ReadByte(page.SourceAddress + index), bytes.Span[index],
                    $"imported debris tile byte {index:X4} matches the pinned cartridge");
            VerifyBombTorizoHandPlm(stock);
            VerifyBombTorizoDebrisOverride(directory, stock, page);
            Console.WriteLine("Bomb Torizo hand artwork: 1,024 native tile bytes, exact production PLM/NMI DMA, " +
                "PNG override rebinding and missing-page rejection pass.");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    private static void VerifyBombTorizoDebrisOverride(string directory,
        EnemyTileArtworkCatalog stock, TorizoInstructionTileSheetDefinition page)
    {
        string overrides = Path.Combine(directory, "overrides");
        Directory.CreateDirectory(overrides);
        string pngPath = Path.Combine(directory, page.FileName);
        using (var input = File.OpenRead(pngPath))
        {
            int tiles = page.ByteCount / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(tiles, RoomCharacterAtlasFormat.TileColumns);
            var image = IndexedPng.Read(input, columns * 8, (tiles + columns - 1) / columns * 8);
            image.Pixels[0] ^= 1;
            using var output = File.Create(Path.Combine(overrides, page.FileName));
            IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        }
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrides);
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var runtime = new SuperMetroidRuntime(memory, initialPaletteArt: BombTorizoHandFixturePalette());
        runtime.Enemies.TileArtwork = stock;
        // Already-pending native-address descriptors must use the rebound current
        // PNG, not stale byte blobs captured with the original simulation graph.
        var queue = new VramWriteQueue();
        queue.Enqueue(checked((ushort)page.ByteCount), page.SourceAddress,
            BombTorizoHandPlmProgramDefinitions.DebrisDestinationWord);
        runtime.Enemies.TileArtwork = edited;
        queue.DrainTo(runtime.Vram, memory, runtime);
        AssertTrue(stock.TryResolve(page.SourceAddress, page.ByteCount, out ReadOnlyMemory<byte> original),
            "stock debris page remains installed");
        AssertEqual((byte)(original.Span[0] ^ 0x80),
            runtime.Vram.ReadByte(BombTorizoHandPlmProgramDefinitions.DebrisDestinationWord * 2),
            "edited debris PNG changes the queued planar pixel");
        for (int offset = 1; offset < page.ByteCount; offset++)
            AssertEqual(original.Span[offset], runtime.Vram.ReadByte(
                BombTorizoHandPlmProgramDefinitions.DebrisDestinationWord * 2 + offset),
                $"edited debris PNG preserves other byte {offset:X4}");
        File.Delete(pngPath);
        AssertThrows<FileNotFoundException>(() => EnemyTileArtworkFiles.Load(directory, null),
            "a missing required debris page is rejected at installation loading, not during play");
    }

    private static GameplayBasePaletteCatalog BombTorizoHandFixturePalette() =>
        GameplayBasePaletteCatalog.Load(new MemoryStream(GameplayBasePaletteCatalog.Write(
            new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
                Enumerable.Range(0, SnesCgram.ColorCount).Select(_ => new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 }).ToArray(),
                Enumerable.Range(0, GameplayBasePaletteFormat.SpriteColorCount)
                    .Select(_ => new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 }).ToArray()))));
}
