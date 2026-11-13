using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileCompositionsVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Checks Chainsaw beam DMA against native cartridge behavior, and the palette the Chainsaw beam shows.</summary>
internal static class InvalidBeamGraphicsVerification
{
    /// <summary>Verifies ordinary beam palettes, the Chainsaw beam's graphics load and the listed palette it shows.</summary>
    internal static void VerifyInvalidBeamGraphics()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertThrows<InvalidOperationException>(() => CpuBusRegionRead.Read(bus, 0x907fff),
            "plain high-level address reads do not acquire an invented open-bus value");
        var artwork = InstalledFixtures.ImportProjectiles.BeamTiles;
        AssertTrue(artwork.TryResolve(ChainsawBeamGraphicsDefinitionsConstants.TileSource, 256, out var legacyTiles),
            "restored native Chainsaw DMA resolves through installed artwork");
        AssertTrue(legacyTiles.Span.SequenceEqual(artwork.Resolve(InstalledArtworkId.BeamChainsawTiles).Span),
            "native and typed Chainsaw transfers select the same installed sheet");
        var gameplayBus = NewProjectileCompositionForbiddenBus();
        for (ushort beam = 0; beam < 12; beam++)
        {
            var ordinary = new SamusColours();
            SamusProjectileSystem.LoadBeamPalette(ordinary, SamusEquipmentWordCodec.FromBeamWord(beam));
            RgbColor[] ordinaryBeams = ordinary.ShownBeams(InstalledFixtures.SamusShadeSources with { BeamPalettes = artwork.Palettes ?? throw new InvalidDataException("Beam palettes are installed.") });
            int table = SamusSourceDefinitions.PalettePointers.Value + beam * 2;
            int source = (int)new SnesAddress(0x90, LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, table), CpuBusRegionRead.Read(bus, table + 1)));
            for (int color = 0; color < 16; color++)
                AssertEqual((LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, source + color * 2), CpuBusRegionRead.Read(bus, source + color * 2 + 1))) & 0x7fff,
                    ordinaryBeams[color], $"ordinary beam {beam} palette remains unchanged");
        }
        {
            // The palette table lists no Spazer-and-Plasma colours, so the Chainsaw beam shows Plasma and Wave's (#1736).
            var samusColours = new SamusColours();
            SamusProjectileSystem.LoadBeamPalette(samusColours, SamusBeamFlags.Wave | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma);
            AssertEqual(SamusShades.Beam(SamusBeamCombination.PlasmaWave), samusColours.Beams, "the Chainsaw beam shows Plasma and Wave's shade");
            AssertTrue(samusColours.ShownBeams(InstalledFixtures.SamusShadeSources).SequenceEqual(
                    InstalledFixtures.SamusShadeSources.BeamPalettes.Colours(SamusBeamCombination.PlasmaWave)),
                "the Chainsaw beam shows Plasma and Wave's installed palette");
            // The page Chainsaw's equipped word binds shows the bytes its native DMA reads.
            var pages = new ProjectileObjectPages(SpriteFrameTestArtwork.StandardObjects, artwork,
                InstalledFixtures.ImportProjectiles.TrailTiles);
            ReadOnlySpan<byte> page = pages.For(SamusBeamFlags.Wave | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma).Characters;
            int first = ProjectileObjectPageDefinitions.BeamFirstCharacter * SpriteArtwork.CharacterByteCount;
            for (int offset = 0; offset < 256; offset++)
                AssertEqual(CpuBusRegionRead.Read(bus, 0x9ac421 + offset), page[first + offset], $"Chainsaw native DMA byte {offset}");
        }
        Console.WriteLine("Chainsaw graphics: the bound page matches native DMA and the beam shows Plasma and Wave's palette.");
    }

    /// <summary>Runs invalid-beam graphics checks.</summary>
    [VerificationCommand("--invalid-beam-graphics")]
    internal static int InvalidBeamGraphicsCommand()
    {
        Suite(nameof(VerifyInvalidBeamGraphics), () => VerifyInvalidBeamGraphics());
        return 0;
    }
}
