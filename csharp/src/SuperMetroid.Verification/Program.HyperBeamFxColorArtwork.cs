using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
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

        var stored = (Dictionary<int, ushort>)typeof(HyperBeamFxColorCatalog).GetField("colors",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(catalog)!;
        AssertEqual(32, stored.Count, "Shared yellow highlights remove four further whole inputs");
        for (int color = 4; color < 8; color++)
            AssertEqual(ReadVerificationWord(bus, 0x8dd906 + 40 + color * 2),
                SamusHyperBeamColorFormat.YellowFromGreen(ReadVerificationWord(bus, 0x8dd906 + 80 + color * 2)),
                "Every original yellow highlight derives from its green hue");
        var extracted = new HyperBeamPaletteFxState();
        var nativeCgram = new SnesCgram();
        var extractedCgram = new SnesCgram();
        for (int color = 0; color < SnesCgram.ColorCount; color++)
        {
            nativeCgram.SetColor(color, (ushort)(color * 31));
            extractedCgram.SetColor(color, (ushort)(color * 31));
        }

        extracted.Spawn();
        for (int call = 0; call < HyperBeamFxColorFormat.FrameCount *
            HyperBeamPaletteFxProgramDefinitions.FrameDuration + 1; call++)
        {
            int expectedFrame = call / 2 % 10;
            ushort originalDuration = ReadVerificationWord(bus, 0x8dd904 + 20 * expectedFrame);
            AssertEqual((ushort)2, originalDuration, "Native Hyper Beam FX duration");
            for (int color = 0; color < 8; color++)
                nativeCgram.SetColor(225 + color, ReadVerificationWord(bus, 0x8dd906 + 20 * expectedFrame + 2 * color));
            HyperBeamPaletteFxStepResult extractedStep = extracted.Step(
                new ProjectileCompositionForbiddenBus(), extractedCgram, catalog);
            AssertEqual(expectedFrame, extractedStep.FrameIndex, "Native FX frame order including loop");
            AssertEqual((call & 1) == 0, extractedStep.PaletteWritten, "Native FX paint/hold cadence");
            AssertEqual((ushort)((call & 1) == 0 ? originalDuration : originalDuration - 1), extractedStep.InstructionTimer, "Native FX timer");
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

        foreach (int invalidFrame in new[] { -1, 10, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Apply(new SnesCgram(), invalidFrame, 225), "FX frame bounds");
        foreach (string editScope in new[] { "all", "even", "green" })
        {
            var document = JsonSerializer.Deserialize<HyperBeamFxColorDocument>(json, MapPresentationFormat.JsonOptions)!;
            for (int frame = 0; frame < 10; frame++)
            for (int color = 0; color < 8; color++)
            {
                if (editScope == "even" && (frame & 1) != 0 || editScope == "green" && frame != 4) continue;
                int value = 1000 + frame * 8 + color;
                document.Frames[frame][color] = new PaletteRgb5 { Red = value & 31, Green = value >> 5 & 31, Blue = value >> 10 & 31 };
            }
            var modified = HyperBeamFxColorCatalog.Load(new MemoryStream(HyperBeamFxColorCatalog.Write(document)));
            var result = new SnesCgram();
            for (int frame = 0; frame < 10; frame++)
            {
                modified.Apply(result, frame, 225);
                for (int color = 0; color < 8; color++)
                {
                    bool changed = editScope == "all" || editScope == "even" && (frame & 1) == 0 || editScope == "green" && frame == 4;
                    ushort expected = changed ? (ushort)(1000 + frame * 8 + color) :
                        ReadVerificationWord(bus, 0x8dd906 + 20 * frame + 2 * color);
                    AssertEqual(expected, result.Colors[225 + color], "All independent FX edits and endpoint-only edits survive shared hues");
                    AssertEqual((frame & 1) == 0 && (color != 0 || frame == 0) && !(frame == 2 && color >= 4), stored.ContainsKey(frame * 8 + color),
                        "Original FX input ownership");
                }
            }
        }
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
