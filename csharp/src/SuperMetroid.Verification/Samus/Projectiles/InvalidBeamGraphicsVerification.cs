using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileCompositionsVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks Chainsaw beam DMA and glitched palette reads against native cartridge behavior.</summary>
internal static class InvalidBeamGraphicsVerification
{
    /// <summary>Verifies bounded instruction reads, ordinary beam palettes, and both direct and queued Chainsaw graphics loads.</summary>
    internal static void VerifyInvalidBeamGraphics()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertThrows<InvalidOperationException>(() => CpuBusRegionRead.Read(bus, 0x907fff),
            "plain high-level address reads do not acquire an invented open-bus value");
        // ROM oracle for the typed glitched palette: colour c is the word whose low byte is the
        // byte before Samus_Animate's byte 2c (the open-bus pointer bank for colour zero) and
        // whose high byte is Samus_Animate's byte 2c, kept as CGRAM keeps it.
        for (int color = 0; color < BeamPaletteDefinitions.ColorCount; color++)
        {
            int highByteAddress = ChainsawBeamGraphicsDefinitionsConstants.InstructionAddress + color * 2;
            byte low = color == 0
                ? ChainsawBeamGraphicsDefinitionsConstants.OpenBusByte
                : bus.ReadCartridgeByte(highByteAddress - 1);
            RgbColor native = RgbColor.FromWordIgnoringTopBit(LittleEndianBytes.Word(low, bus.ReadCartridgeByte(highByteAddress)));
            AssertEqual(native, ChainsawBeamGraphicsDefinitions.Colours[color],
                $"Chainsaw palette colour {color} is the open-bus byte and Samus_Animate's instruction bytes as CGRAM keeps them");
        }
        using var scratch = new TestScratchDirectory("projectile-artwork");
        string installedDirectory = scratch.Root;
        SuperMetroid.AssetExtraction.Samus.ProjectilePresentationFiles.Extract(bus, installedDirectory);
        var artwork = SuperMetroid.AssetExtraction.Samus.ProjectilePresentationFiles.Load(installedDirectory, null).BeamTiles;
        AssertTrue(artwork.TryResolve(ChainsawBeamGraphicsDefinitionsConstants.TileSource, 256, out var legacyTiles),
            "restored native Chainsaw DMA resolves through installed artwork");
        AssertTrue(legacyTiles.Span.SequenceEqual(artwork.Resolve(InstalledArtworkId.BeamChainsawTiles).Span),
            "native and typed Chainsaw transfers select the same installed sheet");
        var gameplayBus = NewProjectileCompositionForbiddenBus();
        for (ushort beam = 0; beam < 12; beam++)
        {
            var ordinary = new SamusColourRig();
            SamusProjectileSystem.LoadBeamPalette(ordinary.Samus.Beam, (SamusBeamFlags)beam, artwork.Palettes);
            int table = SamusSourceDefinitions.PalettePointers.Value + beam * 2;
            int source = (int)new SnesAddress(0x90, LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, table), CpuBusRegionRead.Read(bus, table + 1)));
            for (int color = 0; color < 16; color++)
                AssertEqual((LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, source + color * 2), CpuBusRegionRead.Read(bus, source + color * 2 + 1))) & 0x7fff,
                    ordinary.Colors[0xe0 + color], $"ordinary beam {beam} palette remains unchanged");
        }
        // Independent AC8D/ACCD CPU trace with bus-latch tracking, recorded in the
        // native probe notes. These are WRAM words; CGRAM discards their high bit.
        ushort[] nativePalette = [0x0890, 0x30c2, 0x5822, 0x90ec, 0x6ead, 0x2919, 0x000f, 0xfcaa,
            0x8067, 0x1cad, 0xc90a, 0x004d, 0x19f0, 0x4ec9, 0xf000, 0xad14];
        {
            var cgram = new SamusColourRig();
            SamusProjectileSystem.LoadBeamPalette(cgram.Samus.Beam, (SamusBeamFlags)0x000d, artwork.Palettes);
            // The page Chainsaw's equipped word binds shows the bytes its native DMA reads.
            var pages = new ProjectileObjectPages(SpriteFrameTestArtwork.StandardObjects, artwork,
                InstalledFixtures.Projectiles.TrailTiles);
            ReadOnlySpan<byte> page = pages.For((SamusBeamFlags)0x000d).Characters;
            int first = ProjectileObjectPageDefinitions.BeamFirstCharacter * SpriteArtwork.CharacterByteCount;
            for (int offset = 0; offset < 256; offset++)
                AssertEqual(CpuBusRegionRead.Read(bus, 0x9ac421 + offset), page[first + offset], $"Chainsaw native DMA byte {offset}");
            for (int color = 0; color < nativePalette.Length; color++)
                AssertEqual(nativePalette[color] & 0x7fff, cgram.Colors[0xe0 + color], $"Chainsaw native palette color {color}");
        }
        Console.WriteLine("Chainsaw graphics: the bound page matches native DMA and the palette matches the instruction-context trace.");
    }

    /// <summary>Runs invalid-beam graphics checks.</summary>
    [VerificationCommand("--invalid-beam-graphics")]
    internal static int InvalidBeamGraphicsCommand()
    {
        Suite(nameof(VerifyInvalidBeamGraphics), () => VerifyInvalidBeamGraphics());
        return 0;
    }
}
