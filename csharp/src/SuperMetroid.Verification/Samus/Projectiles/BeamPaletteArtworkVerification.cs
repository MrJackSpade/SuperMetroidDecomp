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
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks beam palette extraction, live ownership, and editable color changes.</summary>
internal static class BeamPaletteArtworkVerification
{
    /// <summary>Compares every beam selection's CGRAM with native output and checks isolated JSON color edits.</summary>
    internal static void VerifyBeamPaletteArtwork(ISnesAddressSpace bus)
    {
        byte[] bytes = BeamPaletteExtractor.Extract(bus);
        var catalog = BeamPaletteFile.Read(new MemoryStream(bytes));
        Suite(nameof(VerifyBeamPaletteOwnership), () => VerifyBeamPaletteOwnership(bus, catalog));
        for (ushort selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
        {
            var expected = new SnesCgram(); var actual = new SamusColourRig();
            for (int i = 0; i < SnesCgram.ColorCount; i++)
            {
                expected.SetColor(i, RgbColor.FromWord((ushort)(i * 31)));
                actual.SetColor(i, RgbColor.FromWord((ushort)(i * 31)));
            }
            var nativeQueue = new VramWriteQueue();
            LoadNativeBeamFixture(bus, null, expected, nativeQueue, selection);
            SamusProjectileSystem.LoadBeamPalette(actual.Samus.Beam, (SamusBeamFlags)selection, catalog);
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)), "Extracted beam palette matches native full CGRAM including untouched neighbors");
            var document = RequiredJson.Parse(bytes);
            var red = document.Required("palettes").Required(BeamPaletteDefinitions.Key(SamusBeamCombinations.FromTableIndex(selection))).Required(3);
            red["red"] = red.Required("red").GetValue<int>() ^ 1;
            var edited = BeamPaletteFile.Read(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString())));
            SamusProjectileSystem.LoadBeamPalette(actual.Samus.Beam, (SamusBeamFlags)selection, edited);
            for (int i = 0; i < SnesCgram.ColorCount; i++)
                AssertEqual((ushort)(expected.Colors[i].ToWord() ^ (i == SamusColourDefinitions.BeamFirstColour + 3 ? 1 : 0)),
                    actual.Colors[i], "Palette edit changes exactly the selected color channel bit");
            catalog.LoadTo(actual.Samus.Beam, SamusBeamCombinations.FromTableIndex(selection));
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)), "Previously loaded palette remains immutable after editing input");
        }
        string json = Encoding.UTF8.GetString(bytes);
        AssertThrows<InvalidDataException>(() => BeamPaletteFile.Read(new MemoryStream(Encoding.UTF8.GetBytes(json.Insert(1, "\"version\":1,")))), "Duplicate palette metadata rejected");
        var invalid = RequiredJson.Parse(bytes);
        invalid.Required("palettes").Required(BeamPaletteDefinitions.Key(0)).Required(0)["red"] = 32;
        Reject(invalid, "Out-of-range RGB rejected");
        invalid = JsonNode.Parse(bytes);
        (invalid ?? throw new InvalidOperationException("Verification requires invalid.")).Required("palettes").AsObject().Remove(BeamPaletteDefinitions.Key(0));
        Reject(invalid, "Missing beam selection rejected");
        invalid = JsonNode.Parse(bytes);
        (invalid ?? throw new InvalidOperationException("Verification requires invalid.")).Required("palettes").Required(BeamPaletteDefinitions.Key(0)).AsArray().RemoveAt(0);
        Reject(invalid, "Incomplete palette rejected");
        invalid = JsonNode.Parse(bytes);
        (invalid ?? throw new InvalidOperationException("Verification requires invalid.")).Required("palettes").Required(BeamPaletteDefinitions.Key(0)).Required(0)["damage"] = 100;
        Reject(invalid, "Presentation cannot introduce gameplay properties");
        Console.WriteLine("Beam palettes: twelve native full-CGRAM matches, exact isolated RGB edits, ROM-free queue path and strict JSON validation pass.");

        static void Reject(JsonNode document, string message) => AssertThrows<InvalidDataException>(
            () => BeamPaletteFile.Read(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()))), message);
    }

    /// <summary>Checks NMI rebinding respects Crystal Flash and Hyper owners and uses edited Grapple colors.</summary>
    private static void VerifyBeamPaletteOwnership(ISnesAddressSpace bus, BeamPaletteCatalog palettes)
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var expected = new SamusColourRig();
        palettes.LoadTo(expected.Samus.Beam, 0);
        runtime.Colours.AsPaletteColors().SetColor(SamusColourDefinitions.BeamFirstColour, new RgbColor(27, 3, 0));
        runtime.Artwork.BeamPalettes = palettes;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(runtime.Colours.Colors.Slice(224, 16).SequenceEqual(expected.Colors.Slice(224, 16)), "Accepted NMI refreshes normal beam palette");

        var flash = runtime.Player.IntroducedSamus.CrystalFlash;
        flash.SpecialPaletteKind = SamusSpecialPaletteType.CrystalFlash;
        runtime.Colours.AsPaletteColors().SetColor(224, new RgbColor(27, 3, 0));
        runtime.Artwork.BeamPalettes = palettes;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(123, runtime.Colours.Colors[224], "Rebind cannot erase active Crystal Flash palette");
        flash.SpecialPaletteTimer = WrappingCountdown.Start(ushort.MaxValue);
        AssertTrue(flash.UpdatePalette(runtime.Player.Colours, runtime.Player.IntroducedSamus, palettes), "Crystal Flash restores catalog without ROM reads");
        AssertTrue(runtime.Colours.Colors.Slice(224, 16).SequenceEqual(expected.Colors.Slice(224, 16)), "Crystal Flash completion restores selected palette");
        AssertEqual(SamusSpecialPaletteType.None, flash.SpecialPaletteKind, "Crystal Flash still clears its owner on completion");
        runtime.Player.IntroducedSamus.Drained.HyperBeamPaletteFx.Spawn();
        runtime.Colours.AsPaletteColors().SetColor(224, new RgbColor(8, 14, 0));
        runtime.Artwork.BeamPalettes = palettes;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(456, runtime.Colours.Colors[224], "Rebind cannot erase active Hyper palette");

        // Debug grapple selection uses the same palette index as native firing, but
        // installed artwork must win over the cartridge pointer table after binding.
        var nativeRuntime = CreateRetailRuntimeFixture();
        nativeRuntime.LoadDebugGrapplePalette();
        var grappleExpected = new SamusColourRig();
        palettes.LoadTo(grappleExpected.Samus.Beam, SamusBeamCombination.Ice);
        AssertTrue(nativeRuntime.Colours.Colors.Slice(224, 16).SequenceEqual(grappleExpected.Colors.Slice(224, 16)),
            "Stock debug grapple selection matches native beam palette index two");

        var editedDocument = RequiredJson.Parse(BeamPaletteExtractor.Extract(bus));
        var editedColor = editedDocument.Required("palettes").Required(BeamPaletteDefinitions.Key(SamusBeamCombination.Ice)).Required(3);
        editedColor["red"] = editedColor.Required("red").GetValue<int>() ^ 1;
        var editedPalettes = BeamPaletteFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes(editedDocument.ToJsonString())));
        nativeRuntime.Artwork.BeamPalettes = editedPalettes;
        nativeRuntime.LoadDebugGrapplePalette();
        editedPalettes.LoadTo(grappleExpected.Samus.Beam, SamusBeamCombination.Ice);
        AssertTrue(nativeRuntime.Colours.Colors.Slice(224, 16).SequenceEqual(grappleExpected.Colors.Slice(224, 16)),
            "Bound debug grapple selection uses edited extracted palette, not cartridge colors");
        AssertEqual(32657, nativeRuntime.Colours.Colors[223], "Debug grapple retains the adjacent fixed flare color");
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
