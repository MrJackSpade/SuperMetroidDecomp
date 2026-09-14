using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBeamPaletteArtwork(ISnesAddressSpace bus, BeamTileCatalog tiles)
    {
        byte[] bytes = BeamPaletteExtractor.Extract(bus);
        var catalog = BeamPaletteCatalog.Load(new MemoryStream(bytes));
        VerifyBeamPaletteOwnership(bus, catalog);
        for (ushort selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
        {
            var expected = new SnesCgram(); var actual = new SnesCgram();
            for (int i = 0; i < SnesCgram.ColorCount; i++)
            {
                expected.SetColor(i, (ushort)(i * 31));
                actual.SetColor(i, (ushort)(i * 31));
            }
            var nativeQueue = new VramWriteQueue(); var queue = new VramWriteQueue();
            SamusProjectileSystem.QueueBeamTilesAndLoadPalette(bus, nativeQueue, expected, selection);
            SamusProjectileSystem.QueueBeamTilesAndLoadPalette(new ProjectileCompositionForbiddenBus(), queue, actual, selection, tiles, catalog);
            AssertTrue(expected.Colors.SequenceEqual(actual.Colors), "Extracted beam palette matches native full CGRAM including untouched neighbors");
            AssertEqual(nativeQueue.TailInBytes, queue.TailInBytes, "Palette selection retains tile queue timing");
            var document = JsonNode.Parse(bytes)!;
            var red = document["palettes"]![BeamPaletteDefinitions.Key(selection)]![3]!;
            red["red"] = red["red"]!.GetValue<int>() ^ 1;
            var edited = BeamPaletteCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString())));
            SamusProjectileSystem.QueueBeamTilesAndLoadPalette(new ProjectileCompositionForbiddenBus(), new VramWriteQueue(), actual, selection, tiles, edited);
            for (int i = 0; i < SnesCgram.ColorCount; i++)
                AssertEqual((ushort)(expected.Colors[i] ^ (i == SamusProjectileRomData.Palettes.BeamDestinationIndex + 3 ? 1 : 0)),
                    actual.Colors[i], "Palette edit changes exactly the selected color channel bit");
            catalog.LoadTo(actual, selection);
            AssertTrue(expected.Colors.SequenceEqual(actual.Colors), "Previously loaded palette remains immutable after editing input");
        }
        string json = Encoding.UTF8.GetString(bytes);
        AssertThrows<InvalidDataException>(() => BeamPaletteCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(json.Insert(1, "\"version\":1,")))), "Duplicate palette metadata rejected");
        var invalid = JsonNode.Parse(bytes)!;
        invalid["palettes"]![BeamPaletteDefinitions.Key(0)]![0]!["red"] = 32;
        Reject(invalid, "Out-of-range RGB rejected");
        invalid = JsonNode.Parse(bytes)!;
        invalid["palettes"]!.AsObject().Remove(BeamPaletteDefinitions.Key(0));
        Reject(invalid, "Missing beam selection rejected");
        invalid = JsonNode.Parse(bytes)!;
        invalid["palettes"]![BeamPaletteDefinitions.Key(0)]!.AsArray().RemoveAt(0);
        Reject(invalid, "Incomplete palette rejected");
        invalid = JsonNode.Parse(bytes)!;
        invalid["palettes"]![BeamPaletteDefinitions.Key(0)]![0]!["damage"] = 100;
        Reject(invalid, "Presentation cannot introduce gameplay properties");
        Console.WriteLine("Beam palettes: twelve native full-CGRAM matches, exact isolated RGB edits, ROM-free queue path and strict JSON validation pass.");

        static void Reject(JsonNode document, string message) => AssertThrows<InvalidDataException>(
            () => BeamPaletteCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()))), message);
    }

    private static void VerifyBeamPaletteOwnership(ISnesAddressSpace bus, BeamPaletteCatalog palettes)
    {
        var artwork = BeamTileCatalog.Load(BeamTileExtractor.Extract(bus), palettes);
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.RunNmi(0, true);
        var expected = new SnesCgram();
        palettes.LoadTo(expected, 0);
        runtime.Cgram.SetColor(SamusProjectileRomData.Palettes.BeamDestinationIndex, 123);
        runtime.BeamArtwork = artwork;
        runtime.RunNmi(0, false);
        AssertEqual(123, runtime.Cgram.Colors[SamusProjectileRomData.Palettes.BeamDestinationIndex], "Lag NMI retains palette before rebind publication");
        runtime.RunNmi(0, true);
        AssertTrue(runtime.Cgram.Colors.Slice(224, 16).SequenceEqual(expected.Colors.Slice(224, 16)), "Accepted NMI refreshes normal beam palette");

        var flash = runtime.Samus!.CrystalFlash;
        typeof(SamusCrystalFlashState).GetProperty(nameof(SamusCrystalFlashState.SpecialPaletteType))!.SetValue(flash, (ushort)SamusSpecialPaletteType.CrystalFlash);
        runtime.Cgram.SetColor(224, 123);
        runtime.BeamArtwork = artwork;
        runtime.RunNmi(0, true);
        AssertEqual(123, runtime.Cgram.Colors[224], "Rebind cannot erase active Crystal Flash palette");
        typeof(SamusCrystalFlashState).GetProperty(nameof(SamusCrystalFlashState.SpecialPaletteTimer))!.SetValue(flash, ushort.MaxValue);
        AssertTrue(flash.UpdatePalette(new ProjectileCompositionForbiddenBus(), runtime.Cgram, runtime.Samus, palettes), "Crystal Flash restores catalog without ROM reads");
        AssertTrue(runtime.Cgram.Colors.Slice(224, 16).SequenceEqual(expected.Colors.Slice(224, 16)), "Crystal Flash completion restores selected palette");
        AssertEqual(SamusSpecialPaletteType.None, flash.SpecialPaletteKind, "Crystal Flash still clears its owner on completion");
        runtime.Samus.Drained.HyperBeamPaletteFx.Spawn();
        runtime.Cgram.SetColor(224, 456);
        runtime.BeamArtwork = artwork;
        runtime.RunNmi(0, true);
        AssertEqual(456, runtime.Cgram.Colors[224], "Rebind cannot erase active Hyper palette");
    }
}
