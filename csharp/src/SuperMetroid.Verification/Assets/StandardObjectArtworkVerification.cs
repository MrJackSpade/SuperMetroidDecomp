using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
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
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed standard OBJ characters and their override path through gameplay setup's selection.</summary>
internal static class StandardObjectArtworkVerification
{
    /// <summary>The cartridge's standard sprite sheet at $9A:D200.</summary>
    private const int NativeStandardObjects = 0x9ad200;

    /// <summary>The bytes the OBJ character table shows over the standard sheet's span, as the session records them.</summary>
    private static ReadOnlySpan<byte> ShownStandardObjects(SuperMetroidRuntime runtime, RoomCharacterAtlas standardObjects)
    {
        SpriteArtworkPlacement shown = runtime.EnemySession.SpriteLooks.LastOver(0, StandardObjectArtworkFormat.ByteCount)
            ?? throw new InvalidOperationException("Nothing is shown over the standard OBJ characters.");
        AssertEqual(0, shown.FirstByte, "the standard OBJ characters start the table");
        AssertEqual(StandardObjectArtworkFormat.ByteCount, shown.ByteCount, "the whole standard sheet is shown");
        return shown.Source.Resolve(new RuntimeArtworkContent(runtime, standardObjects)).Span;
    }

    /// <summary>Compares every native OBJ byte, then checks the edited PNG, provenance, and the ROM-free setup selection.</summary>
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
            GameplayBasePalette palettes = GameplayBasePaletteFiles.Load(paletteDirectory, null);
            for (int offset = 0; offset < StandardObjectArtworkFormat.ByteCount; offset++)
                AssertEqual(native.ReadCartridgeByte(NativeStandardObjects + offset),
                    baseline.Characters.Span[offset], $"standard OBJ native byte {offset:X4}");

            var guard = NewFrontendCartridgeReadGuard(
                SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom));
            var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, InstalledFixtures.SamusOverreads, initialPaletteArt: palettes);
            runtime.HudSetup.ShowStandardObjectCharacters();
            AssertTrue(ShownStandardObjects(runtime, baseline).SequenceEqual(baseline.Characters.Span),
                "gameplay setup selects the installed OBJ bytes without reading ROM");

            string stockPath = Path.Combine(stock, StandardObjectArtworkFormat.FileName);
            byte[] originalPng = File.ReadAllBytes(stockPath);
            int tiles = StandardObjectArtworkFormat.ByteCount /
                RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
            int rows = (tiles + columns - 1) / columns;
            IndexedPngImage image = IndexedPng.Read(new MemoryStream(originalPng),
                new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
            image.Pixels[0] = (byte)(image.Pixels[0] == 1 ? 2 : 1);
            Directory.CreateDirectory(overrides);
            string overridePath = Path.Combine(overrides, StandardObjectArtworkFormat.FileName);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            RoomCharacterAtlas edited = StandardObjectArtworkFiles.Load(stock, overrides);
            AssertTrue(!edited.Characters.Span.SequenceEqual(baseline.Characters.Span),
                "selected indexed PNG changes the native standard OBJ transfer");
            var editedRuntime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, InstalledFixtures.SamusOverreads, initialPaletteArt: palettes);
            editedRuntime.HudSetup.ShowStandardObjectCharacters();
            AssertTrue(ShownStandardObjects(editedRuntime, edited).SequenceEqual(edited.Characters.Span),
                "edited OBJ tile is what gameplay setup selects over the OBJ characters");
            StandardObjectArtworkFiles.Extract(native, stock, SupportedCartridge.Sha256);
            AssertTrue(StandardObjectArtworkFiles.Load(stock, overrides).Characters.Span
                .SequenceEqual(edited.Characters.Span),
                "stock re-extraction preserves the selected external OBJ override");

            File.WriteAllText(overridePath, "not an indexed PNG");
            AssertThrows<InvalidDataException>(() => StandardObjectArtworkFiles.Load(stock, overrides),
                "invalid OBJ override does not silently fall back");
            File.Delete(overridePath);
            AssertTrue(StandardObjectArtworkFiles.Load(stock, overrides).Characters.Span
                .SequenceEqual(baseline.Characters.Span),
                "removing override restores stock standard OBJ transfer");
            File.WriteAllText(stockPath, "damaged stock");
            AssertThrows<InvalidDataException>(() => StandardObjectArtworkFiles.Load(stock, overrides),
                "damaged stock OBJ provenance fails loudly");
            Console.WriteLine("Standard OBJ art: all $2E00 native bytes, guarded setup selection, visible PNG edit, invalid override and stock provenance pass.");
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
