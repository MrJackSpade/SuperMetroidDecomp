using SuperMetroid.TestSupport.VerificationOracles;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Bosses.Torizo.BombTorizoHandVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Bomb Torizo hand debris art from extraction through queued VRAM transfer.</summary>
internal static class BombTorizoHandArtworkVerification
{
    /// <summary>Confirm #1162's exact PLM-to-installed-art-to-NMI handoff, not a room sweep.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Bomb Torizo hand artwork")]
    internal static void VerifyBombTorizoHandArtwork()
    {
        var importer = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        string directory = Path.Combine(Path.GetFullPath("csharp/test-temp"),
            "bomb-torizo-hand-artwork-" + Guid.NewGuid().ToString("N"));
        try
        {
            EnemyTileArtworkFiles.Extract(importer, directory, SupportedCartridge.Sha256);
            InstalledEnemyArtwork stock = EnemyTileArtworkFiles.Load(directory, null);
            TileSheetDefinition page = TorizoInstructionVramArtworkDefinitions.ChozoDebris;
            AssertTrue(new EnemyTileTransfers(stock).TryResolve(page.SourceAddress, page.ByteCount, out ReadOnlyMemory<byte> bytes),
                "installed Chozo debris page exists");
            for (int index = 0; index < bytes.Length; index++)
                AssertEqual(importer.ReadCartridgeByte(page.SourceAddress + index), bytes.Span[index],
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

    /// <summary>Checks a PNG override reaches an already queued DMA request and preserves other tile bytes.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyBombTorizoDebrisOverride(string directory,
        InstalledEnemyArtwork stock, TileSheetDefinition page)
    {
        string overrides = Path.Combine(directory, "overrides");
        Directory.CreateDirectory(overrides);
        string pngPath = Path.Combine(directory, page.FileName);
        using (var input = File.OpenRead(pngPath))
        {
            int tiles = page.ByteCount / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(tiles, RoomCharacterAtlasFormat.TileColumns);
            var image = IndexedPng.Read(input, new(checked((ushort)(columns * 8)), checked((ushort)((tiles + columns - 1) / columns * 8))));
            image.Pixels[0] ^= 1;
            using var output = File.Create(Path.Combine(overrides, page.FileName));
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        }
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(directory, overrides);
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: BombTorizoHandFixturePalette());
        runtime.EnemySession.TileArtwork = stock.Enemies;
        runtime.BossArtwork.Artwork = stock.Bosses;
        runtime.AreaArtwork.Artwork = stock.Areas;
        // Already-pending native-address descriptors must use the rebound current
        // PNG, not stale byte blobs captured with the original simulation graph.
        var queue = new VramWriteQueue();
        queue.Enqueue(checked((ushort)page.ByteCount), page.SourceAddress,
            GameplayVram.ObjectCharactersDma(BombTorizoHandPlmProgramDefinitions.DebrisObjectCharacterWordOffset));
        runtime.EnemySession.TileArtwork = edited.Enemies;
        runtime.BossArtwork.Artwork = edited.Bosses;
        runtime.AreaArtwork.Artwork = edited.Areas;
        queue.DrainTo(runtime.Vram, runtime.Artwork, null, runtime.BgArtwork);
        AssertTrue(new EnemyTileTransfers(stock).TryResolve(page.SourceAddress, page.ByteCount, out ReadOnlyMemory<byte> original),
            "stock debris page remains installed");
        AssertEqual((byte)(original.Span[0] ^ 0x80),
            runtime.Vram.ReadByte(GameplayVram.ObjectCharactersDma(BombTorizoHandPlmProgramDefinitions.DebrisObjectCharacterWordOffset).Encoded * 2),
            "edited debris PNG changes the queued planar pixel");
        for (int offset = 1; offset < page.ByteCount; offset++)
            AssertEqual(original.Span[offset], runtime.Vram.ReadByte(
                GameplayVram.ObjectCharactersDma(BombTorizoHandPlmProgramDefinitions.DebrisObjectCharacterWordOffset).Encoded * 2 + offset),
                $"edited debris PNG preserves other byte {offset:X4}");
        File.Delete(pngPath);
        AssertThrows<FileNotFoundException>(() => EnemyTileArtworkFiles.Load(directory, null),
            "a missing required debris page is rejected at installation loading, not during play");
    }

    /// <summary>Creates the minimal valid palette catalog for a ROM-free hand artwork fixture.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static GameplayBasePaletteCatalog BombTorizoHandFixturePalette() =>
        GameplayBasePaletteCatalog.Load(new MemoryStream(GameplayBasePaletteCatalog.Write(
            new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
                Enumerable.Range(0, SnesCgram.ColorCount).Select(_ => new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 }).ToArray(),
                Enumerable.Range(0, GameplayBasePaletteFormat.SpriteColorCount)
                    .Select(_ => new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 }).ToArray()))));

    /// <summary>Runs the Bomb Torizo hand artwork extraction and override checks.</summary>
    [VerificationCommand("--bomb-torizo-hand-artwork")]
    internal static int BombTorizoHandArtworkCommand()
    {
        Suite(nameof(VerifyBombTorizoHandArtwork), () => VerifyBombTorizoHandArtwork());
        return 0;
    }
}
