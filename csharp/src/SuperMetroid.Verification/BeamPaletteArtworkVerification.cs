using SuperMetroid.Tooling;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static Program;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static BeamTileArtworkVerification;
using static ProjectileCompositionsVerification;
using static RetailRuntimeFixtureVerification;
using static SuiteTimingVerification;

/// <summary>Checks beam palette extraction, live ownership, and editable color changes.</summary>
internal static class BeamPaletteArtworkVerification
{
    /// <summary>Compares every beam selection's CGRAM with native output and checks isolated JSON color edits.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    internal static void VerifyBeamPaletteArtwork(ISnesAddressSpace bus, BeamTileCatalog tiles)
    {
        byte[] bytes = BeamPaletteExtractor.Extract(bus);
        var catalog = BeamPaletteCatalog.Load(new MemoryStream(bytes));
        Suite(nameof(VerifyBeamPaletteOwnership), () => VerifyBeamPaletteOwnership(bus, catalog));
        for (ushort selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
        {
            var expected = new SnesCgram(); var actual = new SnesCgram();
            for (int i = 0; i < SnesCgram.ColorCount; i++)
            {
                expected.SetColor(i, Bgr555.FromWord((ushort)(i * 31)));
                actual.SetColor(i, Bgr555.FromWord((ushort)(i * 31)));
            }
            var nativeQueue = new VramWriteQueue(); var queue = new VramWriteQueue();
            LoadNativeBeamFixture(bus, null, expected, nativeQueue, selection);
            SamusProjectileSystem.QueueBeamTilesAndLoadPalette(NewProjectileCompositionForbiddenBus(), queue, actual, selection, tiles, catalog);
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)), "Extracted beam palette matches native full CGRAM including untouched neighbors");
            AssertEqual(nativeQueue.TailInBytes, queue.TailInBytes, "Palette selection retains tile queue timing");
            var document = RequiredJson.Parse(bytes);
            var red = document.Required("palettes").Required(BeamPaletteDefinitions.Key(SamusBeamCombinations.FromTableIndex(selection))).Required(3);
            red["red"] = red.Required("red").GetValue<int>() ^ 1;
            var edited = BeamPaletteCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString())));
            SamusProjectileSystem.QueueBeamTilesAndLoadPalette(NewProjectileCompositionForbiddenBus(), new VramWriteQueue(), actual, selection, tiles, edited);
            for (int i = 0; i < SnesCgram.ColorCount; i++)
                AssertEqual((ushort)(expected.Colors[i].ToWord() ^ (i == SamusProjectileRomData.Palettes.BeamDestinationIndex + 3 ? 1 : 0)),
                    actual.Colors[i], "Palette edit changes exactly the selected color channel bit");
            catalog.LoadTo(actual, SamusBeamCombinations.FromTableIndex(selection));
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)), "Previously loaded palette remains immutable after editing input");
        }
        string json = Encoding.UTF8.GetString(bytes);
        AssertThrows<InvalidDataException>(() => BeamPaletteCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(json.Insert(1, "\"version\":1,")))), "Duplicate palette metadata rejected");
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
            () => BeamPaletteCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()))), message);
    }

    /// <summary>Checks NMI rebinding respects Crystal Flash and Hyper owners and uses edited Grapple colors.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void VerifyBeamPaletteOwnership(ISnesAddressSpace bus, BeamPaletteCatalog palettes)
    {
        var artwork = BeamTileCatalog.Load(BeamTileExtractor.Extract(bus), palettes);
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.NmiHandler.RunNmi(0, true);
        var expected = new SnesCgram();
        palettes.LoadTo(expected, 0);
        runtime.Cgram.SetColor(SamusProjectileRomData.Palettes.BeamDestinationIndex, new Bgr555(27, 3, 0));
        runtime.Artwork.BeamArtwork = artwork;
        runtime.NmiHandler.RunNmi(0, false);
        AssertEqual(123, runtime.Cgram.Colors[SamusProjectileRomData.Palettes.BeamDestinationIndex], "Lag NMI retains palette before rebind publication");
        runtime.NmiHandler.RunNmi(0, true);
        AssertTrue(runtime.Cgram.Colors.Slice(224, 16).SequenceEqual(expected.Colors.Slice(224, 16)), "Accepted NMI refreshes normal beam palette");

        var flash = runtime.Player.IntroducedSamus.CrystalFlash;
        flash.SpecialPaletteType = (ushort)SamusSpecialPaletteType.CrystalFlash;
        runtime.Cgram.SetColor(224, new Bgr555(27, 3, 0));
        runtime.Artwork.BeamArtwork = artwork;
        runtime.NmiHandler.RunNmi(0, true);
        AssertEqual(123, runtime.Cgram.Colors[224], "Rebind cannot erase active Crystal Flash palette");
        flash.SpecialPaletteTimer = NativeCountdown.Start(ushort.MaxValue);
        AssertTrue(flash.UpdatePalette(NewProjectileCompositionForbiddenBus(), runtime.Cgram, runtime.Player.IntroducedSamus, palettes), "Crystal Flash restores catalog without ROM reads");
        AssertTrue(runtime.Cgram.Colors.Slice(224, 16).SequenceEqual(expected.Colors.Slice(224, 16)), "Crystal Flash completion restores selected palette");
        AssertEqual(SamusSpecialPaletteType.None, flash.SpecialPaletteKind, "Crystal Flash still clears its owner on completion");
        runtime.Player.IntroducedSamus.Drained.HyperBeamPaletteFx.Spawn();
        runtime.Cgram.SetColor(224, new Bgr555(8, 14, 0));
        runtime.Artwork.BeamArtwork = artwork;
        runtime.NmiHandler.RunNmi(0, true);
        AssertEqual(456, runtime.Cgram.Colors[224], "Rebind cannot erase active Hyper palette");

        // Debug grapple selection uses the same palette index as native firing, but
        // installed artwork must win over the cartridge pointer table after binding.
        var nativeRuntime = CreateRetailRuntimeFixture(bus);
        nativeRuntime.LoadDebugGrapplePalette();
        var grappleExpected = new SnesCgram();
        palettes.LoadTo(grappleExpected, SamusBeamCombination.Ice);
        AssertTrue(nativeRuntime.Cgram.Colors.Slice(224, 16).SequenceEqual(grappleExpected.Colors.Slice(224, 16)),
            "Stock debug grapple selection matches native beam palette index two");

        var editedDocument = RequiredJson.Parse(BeamPaletteExtractor.Extract(bus));
        var editedColor = editedDocument.Required("palettes").Required(BeamPaletteDefinitions.Key(SamusBeamCombination.Ice)).Required(3);
        editedColor["red"] = editedColor.Required("red").GetValue<int>() ^ 1;
        var editedPalettes = BeamPaletteCatalog.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(editedDocument.ToJsonString())));
        var editedArtwork = BeamTileCatalog.Load(BeamTileExtractor.Extract(bus), editedPalettes);
        nativeRuntime.Artwork.BeamArtwork = editedArtwork;
        nativeRuntime.LoadDebugGrapplePalette();
        editedPalettes.LoadTo(grappleExpected, SamusBeamCombination.Ice);
        AssertTrue(nativeRuntime.Cgram.Colors.Slice(224, 16).SequenceEqual(grappleExpected.Colors.Slice(224, 16)),
            "Bound debug grapple selection uses edited extracted palette, not cartridge colors");
        AssertEqual(32657, nativeRuntime.Cgram.Colors[223], "Debug grapple retains the adjacent fixed flare color");
    }

    /// <summary>Runs the beam palette extraction and live ownership checks.</summary>
    [VerificationCommand("--beam-palette-artwork")]
    internal static int BeamPaletteArtworkCommand()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBeamPaletteArtwork), () => VerifyBeamPaletteArtwork(rom, BeamTileCatalog.Load(BeamTileExtractor.Extract(rom))));
        return 0;
    }
}
