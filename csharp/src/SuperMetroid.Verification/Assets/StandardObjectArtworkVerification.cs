using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed standard OBJ characters and their override path through gameplay DMA.</summary>
internal static class StandardObjectArtworkVerification
{
    /// <summary>Compares every native OBJ byte, then checks edited PNG transfer, provenance, and ROM-free queued DMA.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for standard object artwork")]
    internal static void VerifyStandardObjectArtwork(string sourceRom)
    {
        using var directoryScratch = new TestScratchDirectory("standard-objects");
        string directory = directoryScratch.Root;
        string stock = Path.Combine(directory, "stock");
        string overrides = Path.Combine(directory, "overrides");
        var native = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        try
        {
            StandardObjectArtworkFiles.Extract(native, stock, SupportedCartridge.Sha256);
            RoomCharacterAtlas baseline = StandardObjectArtworkFiles.Load(stock, overrides);
            string paletteDirectory = Path.Combine(directory, "palettes");
            GameplayBasePaletteFiles.Extract(native, paletteDirectory, SupportedCartridge.Sha256);
            GameplayBasePaletteCatalog palettes = GameplayBasePaletteFiles.Load(paletteDirectory, null);
            for (int offset = 0; offset < StandardObjectArtworkFormat.TransferByteCount; offset++)
                AssertEqual(native.ReadCartridgeByte(StandardObjectArtworkAddresses.Source + offset),
                    baseline.Transfer.Span[offset], $"standard OBJ native byte {offset:X4}");

            var guard = NewFrontendCartridgeReadGuard(
                SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom));
            var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: palettes);
            runtime.Artwork.StandardObjectArt = baseline;
            runtime.HudSetup.QueueStandardObjectTiles();
            runtime.VramWrites.DrainTo(runtime.Vram, runtime.Artwork);
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
                new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
            image.Pixels[0] = (byte)(image.Pixels[0] == 1 ? 2 : 1);
            Directory.CreateDirectory(overrides);
            string overridePath = Path.Combine(overrides, StandardObjectArtworkFormat.FileName);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
            RoomCharacterAtlas edited = StandardObjectArtworkFiles.Load(stock, overrides);
            AssertTrue(!edited.Transfer.Span.SequenceEqual(baseline.Transfer.Span),
                "selected indexed PNG changes the native standard OBJ transfer");
            var editedRuntime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: palettes);
            editedRuntime.Artwork.StandardObjectArt = edited;
            editedRuntime.HudSetup.QueueStandardObjectTiles();
            editedRuntime.VramWrites.DrainTo(editedRuntime.Vram, editedRuntime.Artwork);
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

    /// <summary>Runs the standard object-artwork check against the repository's retail ROM.</summary>
    [VerificationCommand("--standard-object-artwork")]
    internal static int StandardObjectArtworkCommand()
    {
        string standardObjectRom = RepositoryRomPath;
        Suite(nameof(VerifyStandardObjectArtwork), () => VerifyStandardObjectArtwork(standardObjectRom));
        return 0;
    }
}
