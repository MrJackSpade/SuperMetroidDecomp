using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Samus.Projectiles.BeamTileArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Checks beam palette extraction and live ownership.</summary>
internal static class BeamPaletteArtworkVerification
{
    /// <summary>Compares every beam selection's CGRAM with native output and rejects malformed palette documents.</summary>
    internal static void VerifyBeamPaletteArtwork(ISnesAddressSpace bus)
    {
        byte[] bytes = BeamPaletteExtractor.Extract(bus);
        var catalog = BeamPaletteFile.Read(new MemoryStream(bytes));
        Suite(nameof(VerifyBeamPaletteOwnership), () => VerifyBeamPaletteOwnership(catalog));
        Suite(nameof(VerifyCrystalFlashFinishBeamPalette), () => VerifyCrystalFlashFinishBeamPalette(catalog));
        for (ushort selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
        {
            var expected = new SnesCgram(); var actual = new SamusColours();
            for (int i = 0; i < SnesCgram.ColorCount; i++)
                expected.SetColor(i, RgbColor.FromWord((ushort)(i * 31)));
            var nativeQueue = new VramWriteQueue();
            LoadNativeBeamFixture(bus, null, expected, nativeQueue, selection);
            // The beams name the selection's shade (#1512); the import resolves it from the extracted catalog.
            SamusProjectileSystem.LoadBeamPalette(actual, SamusEquipmentWordCodec.FromBeamWord(selection));
            RgbColor[] shown = actual.ShownBeams(InstalledFixtures.SamusShadeSources with { BeamPalettes = catalog });
            for (int i = 0; i < BeamPaletteDefinitions.ColorCount; i++)
                AssertEqual(expected.Colors[SamusColourNative.BeamFirstCgram + i], shown[i], "Extracted beam palette matches the native beam colours");
            AssertTrue(catalog.Colours(SamusBeamCombinationTableOrder.FromTableIndex(selection)).SequenceEqual(shown),
                "The shown beams are the extracted selection's palette");
        }
        string json = Encoding.UTF8.GetString(bytes);
        AssertThrows<InvalidDataException>(() => BeamPaletteFile.Read(new MemoryStream(Encoding.UTF8.GetBytes(json.Insert(1, "\"version\":1,")))), "Duplicate palette metadata rejected");
        var invalid = RequiredJson.Parse(bytes);
        invalid.Required("palettes").Required(BeamPaletteFile.Key(0)).Required(0)["red"] = 32;
        Reject(invalid, "Out-of-range RGB rejected");
        invalid = JsonNode.Parse(bytes);
        (invalid ?? throw new InvalidOperationException("Verification requires invalid.")).Required("palettes").AsObject().Remove(BeamPaletteFile.Key(0));
        Reject(invalid, "Missing beam selection rejected");
        invalid = JsonNode.Parse(bytes);
        (invalid ?? throw new InvalidOperationException("Verification requires invalid.")).Required("palettes").Required(BeamPaletteFile.Key(0)).AsArray().RemoveAt(0);
        Reject(invalid, "Incomplete palette rejected");
        invalid = JsonNode.Parse(bytes);
        (invalid ?? throw new InvalidOperationException("Verification requires invalid.")).Required("palettes").Required(BeamPaletteFile.Key(0)).Required(0)["damage"] = 100;
        Reject(invalid, "Presentation cannot introduce gameplay properties");
        Console.WriteLine("Beam palettes: twelve native full-CGRAM matches, ROM-free queue path and strict JSON validation pass.");

        static void Reject(JsonNode document, string message) => AssertThrows<InvalidDataException>(
            () => BeamPaletteFile.Read(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()))), message);
    }

    /// <summary>
    /// Crystal Flash's finish reloads the equipped beams' palette through the loader equipping a beam uses (#1506): the
    /// Chainsaw combination, where the finish once refused it, shows Plasma and Wave's palette (#1736), and a retail
    /// combination its installed palette.
    /// </summary>
    private static void VerifyCrystalFlashFinishBeamPalette(InstalledBeamPalettes palettes)
    {
        SamusBeamFlags chainsawBeams = SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Wave;
        AssertEqual(ChainsawBeamGraphicsDefinitions.Selection, SamusBeamCombinations.Of(chainsawBeams), "Spazer, Plasma and Wave select the Chainsaw beam");
        foreach (SamusBeamFlags beams in new[] { SamusBeamFlags.Plasma | SamusBeamFlags.Wave, chainsawBeams })
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { EquippedBeams = beams };
            samus.CrystalFlash.SpecialPaletteKind = SamusSpecialPaletteType.CrystalFlash;
            samus.CrystalFlash.BeamPaletteRestoreRequested = true;
            var finishedColours = new SamusColours();
            AssertTrue(samus.CrystalFlash.UpdatePalette(finishedColours, samus),
                $"{beams}: Crystal Flash's finish runs");
            var expected = new SamusColours();
            SamusProjectileSystem.LoadBeamPalette(expected, beams);
            AssertEqual(expected.Beams, finishedColours.Beams, $"{beams}: Crystal Flash's finish shows the shade equipping the beams shows");
            var sources = InstalledFixtures.SamusShadeSources with { BeamPalettes = palettes };
            AssertTrue(finishedColours.ShownBeams(sources).SequenceEqual(expected.ShownBeams(sources)),
                $"{beams}: Crystal Flash's finish shows the colours equipping the beams shows");
            AssertEqual(SamusSpecialPaletteType.None, samus.CrystalFlash.SpecialPaletteKind, $"{beams}: the finish releases the palette");
        }
    }

    /// <summary>
    /// Checks that Crystal Flash's completion and the debug grapple show beam shades (#1512), and that the grapple's shade
    /// shows the extracted Ice beam palette.
    /// </summary>
    private static void VerifyBeamPaletteOwnership(InstalledBeamPalettes palettes)
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        var flash = runtime.Player.IntroducedSamus.CrystalFlash;
        flash.SpecialPaletteKind = SamusSpecialPaletteType.CrystalFlash;
        flash.BeamPaletteRestoreRequested = true;
        AssertTrue(flash.UpdatePalette(runtime.Player.Colours, runtime.Player.IntroducedSamus), "Crystal Flash completes");
        AssertEqual(SamusShades.Beam(SamusBeamCombinations.Of(runtime.Player.IntroducedSamus.EquippedBeams)), runtime.Player.Colours.Beams,
            "Crystal Flash completion shows the equipped beams' shade");
        AssertEqual(SamusSpecialPaletteType.None, flash.SpecialPaletteKind, "Crystal Flash still clears its owner on completion");

        var nativeRuntime = CreateRetailRuntimeFixture();
        nativeRuntime.LoadDebugGrapplePalette();
        AssertEqual(SamusShades.Beam(SamusBeamCombination.Ice), nativeRuntime.Player.Colours.Beams,
            "Debug grapple selection shows the Ice beam's shade, native beam palette index two");
        AssertEqual(SamusShades.GrappleFiringFlare, nativeRuntime.Player.Colours.GrappleFlare,
            "Debug grapple shows the grapple flare's shade");
        AssertTrue(nativeRuntime.Player.Colours.ShownBeams(InstalledFixtures.SamusShadeSources with { BeamPalettes = palettes })
                .SequenceEqual(palettes.Colours(SamusBeamCombination.Ice)),
            "Stock debug grapple colours match the Ice beam palette");
    }

    /// <summary>Runs the beam palette extraction and live ownership checks.</summary>
    [VerificationCommand("--beam-palette-artwork")]
    internal static int BeamPaletteArtworkCommand()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBeamPaletteArtwork), () => VerifyBeamPaletteArtwork(rom));
        return 0;
    }
}
