using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.BeamPaletteArtworkVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileTrailArtworkVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileTrailDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Samus.Artwork;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks extracted beam PNGs and palettes through the projectile queue and runtime NMI path.</summary>
internal static class BeamTileArtworkVerification
{
    /// <summary>Compares all beam selections with native VRAM, checks the queued transfer and bound page, and rejects malformed PNGs.</summary>
    internal static void VerifyBeamTileArtwork(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailDefinitions), () => VerifyProjectileTrailDefinitions(bus));
        Suite(nameof(VerifyProjectileTrailArtwork), () => VerifyProjectileTrailArtwork(bus));
        var files = BeamTileExtractor.Extract(bus);
        var palettes = BeamPaletteFile.Read(new MemoryStream(BeamPaletteExtractor.Extract(bus)));
        var catalog = BeamTileCatalog.Load(files, palettes);
        Suite(nameof(VerifyBeamPaletteArtwork), () => VerifyBeamPaletteArtwork(bus));
        AssertEqual(14, files.Count, "Every legal beam combination and both bounded invalid uploads have installed artwork");
        for (ushort selection = 0; selection < 12; selection++)
        {
            byte[] png = files[BeamTileAtlasDefinitions.FileName(SamusBeamCombinations.FromTableIndex(selection))];
            var atlas = BeamTileAtlas.Load(new MemoryStream(png), SamusBeamCombinations.FromTableIndex(selection));
            var native = new SnesVram(); var extracted = new SnesVram();
            LoadNativeBeamFixture(bus, native, new SnesCgram(), null, selection);
            atlas.LoadTo(extracted);
            AssertTrue(native.ImageEquals(extracted), "PNG matches production beam upload across complete VRAM");
            AssertEqual(BeamTileAtlasDefinitions.ByteCount, atlas.Characters.Length, "Extracted upload retains native DMA size");
            var actualPaletteColours = new SamusColours(); var expectedPalette = new SnesCgram();
            var paletteSources = InstalledFixtures.SamusShadeSources with { BeamPalettes = catalog.Palettes ?? throw new InvalidDataException("Beam palettes are installed.") };
            var nativeQueue = new VramWriteQueue();
            LoadNativeBeamFixture(bus, null, expectedPalette, nativeQueue, selection);
            SamusProjectileSystem.LoadBeamPalette(actualPaletteColours, (SamusBeamFlags)selection);
            var legacyVram = new SnesVram();
            nativeQueue.DrainTo(legacyVram, new BeamTileTransfers(catalog), null, BgArtworkFixtures.For(legacyVram));
            AssertTrue(legacyVram.ImageEquals(native), "Legacy beam source resolves exact native pixels without ROM DMA");
            // The page the selection binds shows the native beam characters at their DMA destination.
            var pages = new ProjectileObjectPages(SpriteFrameTestArtwork.StandardObjects, catalog,
                InstalledFixtures.Projectiles.TrailTiles);
            AssertTrue(pages.For((SamusBeamFlags)selection).Characters.Slice(ProjectileObjectPageDefinitions.BeamFirstCharacter * SpriteArtwork.CharacterByteCount,
                    BeamTileAtlasDefinitions.ByteCount).SequenceEqual(native.Bytes.Slice(ObjCharacterNativeWords.Of(ProjectileObjectPageDefinitions.BeamFirstCharacter) * 2, BeamTileAtlasDefinitions.ByteCount)),
                "The bound projectile page shows the native beam characters");
            RgbColor[] shownBeams = actualPaletteColours.ShownBeams(paletteSources);
            for (int color = 0; color < 16; color++)
                AssertEqual(expectedPalette.Colors[0xe0 + color], shownBeams[color], "PNG selection leaves beam palette behavior unchanged");
        }
        using var wrongSize = new MemoryStream();
        IndexedPng.Write(wrongSize, new(8, 8), new byte[64], new[] { new RgbaPixel(0, 0, 0, 255) });
        wrongSize.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(wrongSize, 0), "Wrong beam atlas dimensions rejected");
        using var invalidIndex = new MemoryStream();
        var colors = Enumerable.Range(0, 17).Select(i => new RgbaPixel((byte)i, 0, 0, 255)).ToArray();
        var pixels = new byte[512]; pixels[0] = 16;
        IndexedPng.Write(invalidIndex, new(64, 8), pixels, colors);
        invalidIndex.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(invalidIndex, 0), "Beam index exceeds four bit hardware palette");
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(new MemoryStream(new byte[8]), 0), "Malformed beam PNG rejected");
        Console.WriteLine("Beam PNG artwork: twelve native uploads reproduced on the bound page and malformed resource rejection pass.");
    }

    /// <summary>Reconstructs the cartridge's selected beam tile transfer and palette as a native comparison fixture.</summary>
    internal static void LoadNativeBeamFixture(ISnesAddressSpace bus, SnesVram? vram,
        SnesCgram cgram, VramWriteQueue? queue, ushort selection)
    {
        int tileSource = SamusSourceDefinitions.CharacterDataBank.Value |
            CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.TilePointers.Value + selection * 2));
        if (vram is not null)
        {
            var bytes = new byte[BeamTileAtlasDefinitions.ByteCount];
            for (int i = 0; i < bytes.Length; i++) bytes[i] = bus.ReadCartridgeByte(tileSource + i);
            vram.LoadBytes(ObjCharacterNativeWords.Of(ProjectileObjectPageDefinitions.BeamFirstCharacter) * 2, bytes);
        }
        queue?.Enqueue(BeamTileAtlasDefinitions.ByteCount, tileSource, VramDmaDestination.FromNative(ObjCharacterNativeWords.Of(ProjectileObjectPageDefinitions.BeamFirstCharacter)));
        int colors = SamusSourceDefinitions.MovementBank.Value |
            CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.PalettePointers.Value + selection * 2));
        for (int i = 0; i < BeamPaletteDefinitions.ColorCount; i++)
            cgram.SetColor(SamusColourNative.BeamFirstCgram + i, RgbColor.FromWord(checked(CartridgeImportSource.Require(bus).ReadCartridgeWord((colors + i * 2)))));
    }

    /// <summary>Runs beam artwork and runtime transfer checks with the retail cartridge oracle.</summary>
    [VerificationCommand("--beam-tile-artwork-fixture")]
    internal static int BeamTileArtworkFixtureCommand()
    {
        Suite(nameof(VerifyBeamTileArtwork), () => VerifyBeamTileArtwork(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
