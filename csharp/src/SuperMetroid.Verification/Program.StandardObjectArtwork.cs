using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyStandardObjectArtwork(string sourceRom)
    {
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "standard-objects-" + Guid.NewGuid().ToString("N")));
        string stock = Path.Combine(directory, "stock");
        string overrides = Path.Combine(directory, "overrides");
        var native = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
        try
        {
            StandardObjectArtworkFiles.Extract(native, stock, SupportedCartridge.Sha256);
            RoomCharacterAtlas baseline = StandardObjectArtworkFiles.Load(stock, overrides);
            string paletteDirectory = Path.Combine(directory, "palettes");
            GameplayBasePaletteFiles.Extract(native, paletteDirectory, SupportedCartridge.Sha256);
            GameplayBasePaletteCatalog palettes = GameplayBasePaletteFiles.Load(paletteDirectory, null);
            for (int offset = 0; offset < StandardObjectArtworkFormat.TransferByteCount; offset++)
                AssertEqual(native.ReadByte(StandardObjectArtworkAddresses.Source + offset),
                    baseline.Transfer.Span[offset], $"standard OBJ native byte {offset:X4}");

            var guard = new FrontendCartridgeReadGuard(
                SuperMetroidAddressSpace.LoadRetailRom(sourceRom));
            var runtime = new SuperMetroidRuntime(guard, initialPaletteArt: palettes)
                { StandardObjectArt = baseline };
            runtime.VramWrites.Enqueue(StandardObjectArtworkFormat.TransferByteCount,
                StandardObjectArtworkAddresses.Source,
                StandardObjectArtworkFormat.EncodedVramDestination);
            runtime.VramWrites.DrainTo(runtime.Vram, guard, runtime);
            AssertTrue(runtime.Vram.Bytes.Slice(0xc000,
                    StandardObjectArtworkFormat.TransferByteCount).SequenceEqual(baseline.Transfer.Span),
                "actual queued gameplay DMA uses installed OBJ bytes without reading ROM");

            string stockPath = Path.Combine(stock, StandardObjectArtworkFormat.FileName);
            byte[] originalPng = File.ReadAllBytes(stockPath);
            int tiles = StandardObjectArtworkFormat.TransferByteCount /
                RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
            int rows = (tiles + columns - 1) / columns;
            IndexedPngImage image = IndexedPng.Read(new MemoryStream(originalPng),
                columns * 8, rows * 8);
            image.Pixels[0] = (byte)(image.Pixels[0] == 1 ? 2 : 1);
            Directory.CreateDirectory(overrides);
            string overridePath = Path.Combine(overrides, StandardObjectArtworkFormat.FileName);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            RoomCharacterAtlas edited = StandardObjectArtworkFiles.Load(stock, overrides);
            AssertTrue(!edited.Transfer.Span.SequenceEqual(baseline.Transfer.Span),
                "selected indexed PNG changes the native standard OBJ transfer");
            var editedRuntime = new SuperMetroidRuntime(guard, initialPaletteArt: palettes)
                { StandardObjectArt = edited };
            editedRuntime.VramWrites.Enqueue(StandardObjectArtworkFormat.TransferByteCount,
                StandardObjectArtworkAddresses.Source,
                StandardObjectArtworkFormat.EncodedVramDestination);
            editedRuntime.VramWrites.DrainTo(editedRuntime.Vram, guard, editedRuntime);
            AssertTrue(editedRuntime.Vram.Bytes.Slice(0xc000,
                    StandardObjectArtworkFormat.TransferByteCount).SequenceEqual(edited.Transfer.Span),
                "edited OBJ tile reaches live VRAM through production DMA queue");
            StandardObjectArtworkFiles.Extract(native, stock, SupportedCartridge.Sha256);
            AssertTrue(StandardObjectArtworkFiles.Load(stock, overrides).Transfer.Span
                .SequenceEqual(edited.Transfer.Span),
                "stock re-extraction preserves the selected external OBJ override");

            File.WriteAllText(overridePath, "not an indexed PNG");
            AssertThrows<InvalidDataException>(() => StandardObjectArtworkFiles.Load(stock, overrides),
                "invalid OBJ override does not silently fall back");
            File.Delete(overridePath);
            AssertTrue(StandardObjectArtworkFiles.Load(stock, overrides).Transfer.Span
                .SequenceEqual(baseline.Transfer.Span),
                "removing override restores stock standard OBJ transfer");
            File.WriteAllText(stockPath, "damaged stock");
            AssertThrows<InvalidDataException>(() => StandardObjectArtworkFiles.Load(stock, overrides),
                "damaged stock OBJ provenance fails loudly");
            Console.WriteLine("Standard OBJ art: all $2E00 native bytes, guarded queued DMA, visible PNG edit, invalid override and stock provenance pass.");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}
