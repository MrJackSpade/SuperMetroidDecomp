using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.BeamPaletteArtworkVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileTrailArtworkVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileTrailDefinitionsVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks extracted beam PNGs and palettes through the projectile queue and runtime NMI path.</summary>
internal static class BeamTileArtworkVerification
{
    /// <summary>Compares all beam selections with native VRAM, checks one-pixel edits, and exercises queued artwork rebinding.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyBeamTileArtwork(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailDefinitions), () => VerifyProjectileTrailDefinitions(bus));
        Suite(nameof(VerifyProjectileTrailArtwork), () => VerifyProjectileTrailArtwork(bus));
        var files = BeamTileExtractor.Extract(bus);
        var palettes = BeamPaletteCatalog.Load(new MemoryStream(BeamPaletteExtractor.Extract(bus)));
        var catalog = BeamTileCatalog.Load(files, palettes);
        Suite(nameof(VerifyBeamPaletteArtwork), () => VerifyBeamPaletteArtwork(bus));
        Suite(nameof(VerifyRuntimeBeamArtwork), () => VerifyRuntimeBeamArtwork(files, catalog));
        AssertEqual(14, files.Count, "Every legal beam combination and both bounded invalid uploads have editable artwork");
        for (ushort selection = 0; selection < 12; selection++)
        {
            byte[] png = files[BeamTileAtlasDefinitions.FileName(SamusBeamCombinations.FromTableIndex(selection))];
            var atlas = BeamTileAtlas.Load(new MemoryStream(png), SamusBeamCombinations.FromTableIndex(selection));
            var native = new SnesVram(); var extracted = new SnesVram();
            LoadNativeBeamFixture(bus, native, new SnesCgram(), null, selection);
            atlas.LoadTo(extracted);
            AssertTrue(native.ImageEquals(extracted), "PNG matches production beam upload across complete VRAM");
            var image = IndexedPng.Read(new MemoryStream(png), new(64, 8));
            image.Pixels[0] ^= 1;
            using var editedPng = new MemoryStream();
            IndexedPng.Write(editedPng, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            editedPng.Position = 0;
            var edited = BeamTileAtlas.Load(editedPng, SamusBeamCombinations.FromTableIndex(selection));
            edited.LoadTo(extracted);
            int firstByte = ObjCharacterNativeWords.Of(ProjectileObjectPageDefinitions.BeamFirstCharacter) * 2;
            AssertEqual((byte)(native.Bytes[firstByte] ^ 0x80), extracted.Bytes[firstByte], "Edited pixel changes the correct tile plane bit");
            for (int index = 0; index < native.Bytes.Length; index++)
                if (index != firstByte) AssertEqual(native.Bytes[index], extracted.Bytes[index], "Beam edit cannot change neighboring VRAM or other pixels");
            AssertEqual(BeamTileAtlasDefinitions.ByteCount, edited.Characters.Length, "Edited upload retains native DMA size");
            var actualPalette = new SamusColourRig(); var expectedPalette = new SnesCgram();
            var nativeQueue = new VramWriteQueue();
            LoadNativeBeamFixture(bus, null, expectedPalette, nativeQueue, selection);
            SamusProjectileSystem.LoadBeamPalette(actualPalette.Samus.Beam, (SamusBeamFlags)selection, catalog.Palettes);
            var legacyVram = new SnesVram();
            nativeQueue.DrainTo(legacyVram, new BeamTileTransfers(catalog), null, BgArtworkFixtures.For(legacyVram));
            AssertTrue(legacyVram.ImageEquals(native), "Legacy beam source resolves exact native pixels without ROM DMA");
            // The page the selection binds shows the native beam characters at their DMA destination.
            var pages = new ProjectileObjectPages(SpriteFrameTestArtwork.StandardObjects, catalog,
                InstalledFixtures.Projectiles.Trails.Tiles ?? throw new InvalidOperationException("The installation has no trail characters."));
            AssertTrue(pages.For((SamusBeamFlags)selection).Characters.Slice(ProjectileObjectPageDefinitions.BeamFirstCharacter * SpriteArtwork.CharacterByteCount,
                    BeamTileAtlasDefinitions.ByteCount).SequenceEqual(native.Bytes.Slice(ObjCharacterNativeWords.Of(ProjectileObjectPageDefinitions.BeamFirstCharacter) * 2, BeamTileAtlasDefinitions.ByteCount)),
                "The bound projectile page shows the native beam characters");
            AssertTrue(new CgramPalettes(actualPalette).ImageEquals(new CgramPalettes(expectedPalette)), "PNG selection leaves beam palette behavior unchanged");
            var replacements = new Dictionary<string, byte[]>(files) { [BeamTileAtlasDefinitions.FileName(SamusBeamCombinations.FromTableIndex(selection))] = editedPng.ToArray() };
            var editedPages = new ProjectileObjectPages(SpriteFrameTestArtwork.StandardObjects, BeamTileCatalog.Load(replacements),
                InstalledFixtures.Projectiles.Trails.Tiles ?? throw new InvalidOperationException("The installation has no trail characters."));
            AssertTrue(editedPages.For((SamusBeamFlags)selection).Characters.Slice(ProjectileObjectPageDefinitions.BeamFirstCharacter * SpriteArtwork.CharacterByteCount,
                    BeamTileAtlasDefinitions.ByteCount).SequenceEqual(extracted.Bytes.Slice(ObjCharacterNativeWords.Of(ProjectileObjectPageDefinitions.BeamFirstCharacter) * 2, BeamTileAtlasDefinitions.ByteCount)),
                "The bound page shows the edited PNG");
        }
        using var wrongSize = new MemoryStream();
        IndexedPng.Write(wrongSize, new(8, 8), new byte[64], new[] { new Rgba32(0, 0, 0, 255) });
        wrongSize.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(wrongSize, 0), "Wrong beam atlas dimensions rejected");
        using var invalidIndex = new MemoryStream();
        var colors = Enumerable.Range(0, 17).Select(i => new Rgba32((byte)i, 0, 0, 255)).ToArray();
        var pixels = new byte[512]; pixels[0] = 16;
        IndexedPng.Write(invalidIndex, new(64, 8), pixels, colors);
        invalidIndex.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(invalidIndex, 0), "Beam index exceeds four bit hardware palette");
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(new MemoryStream(new byte[8]), 0), "Malformed beam PNG rejected");
        Console.WriteLine("Beam PNG artwork: twelve native uploads reproduced on the bound page, exact edited-pixel isolation and malformed resource rejection pass.");
    }

    /// <summary>Reconstructs the cartridge's selected beam tile transfer and palette as a native comparison fixture.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the native beam tile artwork fixture")]
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
            cgram.SetColor(SamusColourDefinitions.BeamFirstColour + i, Bgr555.FromWord(checked(CartridgeImportSource.Require(bus).ReadCartridgeWord((colors + i * 2)))));
    }
    /// <summary>Checks accepted NMI publication of edited PNG bytes and host artwork rebinding after a runtime restore.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyRuntimeBeamArtwork(Dictionary<string, byte[]> files, BeamTileCatalog stock)
    {
        var image = IndexedPng.Read(new MemoryStream(files[BeamTileAtlasDefinitions.FileName(0)]), new(64, 8));
        image.Pixels[0] ^= 1;
        using var png = new MemoryStream();
        IndexedPng.Write(png, new(64, 8), image.Pixels, image.Colours);
        var changed = new Dictionary<string, byte[]>(files) { [BeamTileAtlasDefinitions.FileName(0)] = png.ToArray() };
        var edited = BeamTileCatalog.Load(changed, stock.Palettes);
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        byte[] previous = runtime.Vram.Bytes.ToArray();
        runtime.Artwork.BeamArtwork = edited;
        AssertTrue(previous.SequenceEqual(runtime.Vram.Bytes), "Binding beam PNG is a page binding, not a VRAM write");
        int first = ProjectileObjectPageDefinitions.BeamFirstCharacter * SpriteArtwork.CharacterByteCount;
        AssertTrue(BoundBeamCharacters(runtime, 0).SequenceEqual(edited.Resolve(BeamTileCatalog.AssetFor(0)).Span),
            "The bound page shows the current PNG");
        using var saved = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(saved, runtime);
        saved.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Runtime.SuperMetroidRuntime>(saved);
        AssertTrue(restored.Artwork.BeamArtwork is null, "Beam artwork is not embedded in runtime state");
        BindRetailRuntimeFixture(restored);
        restored.Artwork.BeamArtwork = stock;
        AssertTrue(BoundBeamCharacters(restored, 1).SequenceEqual(stock.Resolve(BeamTileCatalog.AssetFor(SamusBeamCombination.Wave)).Span),
            "Restored runtime binds the equipped selection through its current provider");
        Console.WriteLine("Runtime beam PNG: VRAM untouched by rebind, bound page follows current PNG and restored provider pass.");

        ReadOnlySpan<byte> BoundBeamCharacters(SuperMetroid.Core.Runtime.SuperMetroidRuntime target, ushort equippedBeams) =>
            (target.Artwork.ProjectileObjects ?? throw new InvalidOperationException("The runtime has no projectile page."))
                .For((SamusBeamFlags)equippedBeams).Characters.Slice(first, BeamTileAtlasDefinitions.ByteCount);
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
