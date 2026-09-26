using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyHyperBeamFxColorArtwork(ISnesAddressSpace bus)
    {
        byte[] json = HyperBeamFxColorExtractor.Extract(bus);
        var catalog = HyperBeamFxColorCatalog.Load(new MemoryStream(json));
        AssertEqual(HyperBeamPaletteFxProgramDefinitions.FrameCount,
            HyperBeamFxColorFormat.FrameCount, "Extracted Hyper Beam frame count matches compiled control");
        AssertEqual(HyperBeamPaletteFxProgramDefinitions.ColorsPerFrame,
            HyperBeamFxColorFormat.ColorsPerFrame, "Extracted Hyper Beam color count matches compiled control");

        var native = new HyperBeamPaletteFxState();
        var extracted = new HyperBeamPaletteFxState();
        var nativeCgram = new SnesCgram();
        var extractedCgram = new SnesCgram();
        for (int color = 0; color < SnesCgram.ColorCount; color++)
        {
            nativeCgram.SetColor(color, (ushort)(color * 31));
            extractedCgram.SetColor(color, (ushort)(color * 31));
        }
        native.Spawn();
        extracted.Spawn();
        for (int call = 0; call < HyperBeamFxColorFormat.FrameCount *
            HyperBeamPaletteFxProgramDefinitions.FrameDuration + 1; call++)
        {
            HyperBeamPaletteFxStepResult nativeStep = native.Step(bus, nativeCgram);
            HyperBeamPaletteFxStepResult extractedStep = extracted.Step(
                new ProjectileCompositionForbiddenBus(), extractedCgram, catalog);
            AssertEqual(nativeStep, extractedStep,
                "Extracted Hyper Beam color frames preserve native frame order and timing");
            AssertTrue(nativeCgram.Colors.SequenceEqual(extractedCgram.Colors),
                "Extracted Hyper Beam frame matches full CGRAM including untouched colors");
        }
        AssertEqual(1, extracted.CompletedCycles, "Hyper Beam palette loop retains compiled control cadence");

        var edit = JsonNode.Parse(json)!;
        JsonNode red = edit["frames"]![3]![2]!["red"]!;
        edit["frames"]![3]![2]!["red"] = red.GetValue<int>() ^ 1;
        var edited = HyperBeamFxColorCatalog.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(edit.ToJsonString())));
        nativeCgram = new SnesCgram();
        extractedCgram = new SnesCgram();
        catalog.Apply(nativeCgram, 3, destination: 225);
        edited.Apply(extractedCgram, 3, destination: 225);
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            AssertEqual((ushort)(nativeCgram.Colors[color] ^ (color == 227 ? 1 : 0)),
                extractedCgram.Colors[color],
                "Hyper Beam FX edit changes only the selected visual color channel");

        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.BeamArtwork = BeamTileCatalog.Load(BeamTileExtractor.Extract(bus),
            hyperBeamFxColors: edited);
        runtime.Samus!.Drained.HyperBeamPaletteFx.Spawn();
        for (int call = 0; call < 7; call++) runtime.StepFrame(0);
        AssertEqual(3, runtime.LastHyperBeamPaletteFxStep!.Value.FrameIndex,
            "Gameplay runtime reaches edited Hyper Beam FX frame at compiled cadence");
        AssertEqual(extractedCgram.Colors[227], runtime.Cgram.Colors[227],
            "Installed Hyper Beam FX color reaches gameplay CGRAM");

        string source = Encoding.UTF8.GetString(json);
        AssertThrows<InvalidDataException>(() => HyperBeamFxColorCatalog.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(source.Insert(1, "\"version\":1,")))),
            "Duplicate Hyper Beam FX metadata rejected");
        var invalid = JsonNode.Parse(json)!;
        invalid["frames"]![0]![0]!["red"] = 32;
        Reject(invalid, "Out-of-range Hyper Beam FX channel rejected");
        invalid = JsonNode.Parse(json)!;
        invalid["frames"]!.AsArray().RemoveAt(0);
        Reject(invalid, "Missing Hyper Beam FX frame rejected");
        invalid = JsonNode.Parse(json)!;
        invalid["frames"]![0]!.AsArray().RemoveAt(0);
        Reject(invalid, "Missing Hyper Beam FX color rejected");
        invalid = JsonNode.Parse(json)!;
        invalid["frames"]![0]![0]!["damage"] = 1;
        Reject(invalid, "Gameplay property in Hyper Beam FX artwork rejected");
        Console.WriteLine("Hyper Beam FX colors: 80 ROM words, all ten timed frames and loop, full-CGRAM parity, ROM-free live cycle, isolated edit and strict JSON pass.");

        static void Reject(JsonNode document, string message) => AssertThrows<InvalidDataException>(
            () => HyperBeamFxColorCatalog.Load(
                new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()))), message);
    }
}
