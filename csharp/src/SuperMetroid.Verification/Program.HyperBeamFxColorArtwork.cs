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
        AssertEqual(10, stored.Count, "Neutral highlight stores one intensity instead of a whole color");
        int neutralIntensity = (int)typeof(HyperBeamFxColorCatalog).GetField("neutralIntensity",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(catalog)!;
        AssertEqual((int)(ReadVerificationWord(bus, 0x8dd906) & 31), neutralIntensity, "Original neutral intensity");
        object neutralOverrides = typeof(HyperBeamFxColorCatalog).GetField("neutralOverrides",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(catalog)!;
        foreach (var field in typeof(LoadingPaletteInputView.Channels).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
            AssertTrue(field.GetValue(neutralOverrides) is null, "No duplicate original neutral channels are stored");
        for (int intensity = 0; intensity < 32; intensity++)
            AssertEqual((ushort)(intensity * 1057), HyperBeamFxColorFormat.Neutral(intensity), "Complete neutral RGB5 domain");
        foreach (int invalidIntensity in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => HyperBeamFxColorFormat.Neutral(invalidIntensity), "Neutral intensity bounds");
        var shadeInputs = (Dictionary<int, LoadingPaletteInputView.Channels>)typeof(HyperBeamFxColorCatalog)
            .GetField("shadeInputs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(catalog)!;
        AssertEqual(4, shadeInputs.Count, "Four middle colors have differing components");
        int shadeComponentCount = 0;
        foreach (var entry in shadeInputs)
        {
            int frame = entry.Key / 8, color = entry.Key % 8;
            ushort native = ReadVerificationWord(bus, 0x8dd906 + 20 * frame + 2 * color);
            ushort first = ReadVerificationWord(bus, 0x8dd906 + 20 * frame + 2 * (color - 1));
            ushort second = ReadVerificationWord(bus, 0x8dd906 + 20 * frame + 2 * (color + 1));
            string[] names = ["red", "green", "blue"];
            for (int channel = 0; channel < 3; channel++)
            {
                int shift = 5 * channel;
                int expected = (int)Math.Ceiling(((first >> shift & 31) + (second >> shift & 31)) / 2.0);
                int? differing = (native >> shift & 31) == expected ? null : native >> shift & 31;
                object? actual = typeof(LoadingPaletteInputView.Channels).GetField(names[channel],
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(entry.Value);
                AssertTrue(Equals(differing, actual), "Only originally differing middle-shade channels are stored");
                if (actual is not null) shadeComponentCount++;
            }
        }
        AssertEqual(6, shadeComponentCount, "Six differing middle-shade components remain under review");
        var pairedInputs = (Dictionary<int, HyperBeamFxColorCatalog.PairedChannels>)typeof(HyperBeamFxColorCatalog)
            .GetField("pairedInputs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(catalog)!;
        AssertEqual(7, pairedInputs.Count, "Hue endpoint transforms remove two paired inputs");
        int pairedComponentCount = 0;
        foreach (var entry in pairedInputs)
        {
            int frame = entry.Key / 8, color = entry.Key % 8;
            ushort original = ReadVerificationWord(bus, 0x8dd906 + 20 * frame + 2 * color);
            ushort whiteSource = ReadVerificationWord(bus, 0x8dd906), redSource = ReadVerificationWord(bus, 0x8dd90c);
            int sharedRed = frame == 0 ? whiteSource & 31 : redSource & 31;
            int sharedGreen = frame == 4 ? redSource & 31 : redSource >> 5 & 31;
            AssertEqual(original, entry.Value.Resolve(frame == 0, sharedRed, sharedGreen), "Original paired endpoint shared-intensity reconstruction");
            foreach (var field in typeof(HyperBeamFxColorCatalog.PairedChannels).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                if (field.GetValue(entry.Value) is not null) pairedComponentCount++;
            AssertTrue(typeof(HyperBeamFxColorCatalog.PairedChannels).GetField("blue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(entry.Value) is null,
                "No original duplicate blue channel is stored");
        }
        AssertEqual(7, pairedComponentCount, "Seven varying paired components remain; duplicate extrema are calculated");
        for (int rgb = 0; rgb < 32768; rgb++)
        foreach (bool redHue in new[] { false, true })
        {
            AssertEqual((ushort)rgb, new HyperBeamFxColorCatalog.PairedChannels((ushort)rgb, redHue).Resolve(redHue),
                "Complete RGB5 endpoint inputs preserve equal and independently edited blue channels");
            int redComponent = rgb & 31, greenComponent = rgb >> 5 & 31;
            AssertEqual((ushort)rgb, new HyperBeamFxColorCatalog.PairedChannels((ushort)rgb, redHue, redComponent, greenComponent)
                .Resolve(redHue, redComponent, greenComponent), "All RGB5 shared extrema preserve supplied colors");
            int otherRed = (redComponent + 1) % 32, otherGreen = (greenComponent + 1) % 32;
            AssertEqual((ushort)rgb, new HyperBeamFxColorCatalog.PairedChannels((ushort)rgb, redHue, otherRed, otherGreen)
                .Resolve(redHue, otherRed, otherGreen), "All RGB5 differing extrema preserve independent inputs");
        }
        for (int color = 4; color < 8; color++)
            AssertEqual(ReadVerificationWord(bus, 0x8dd906 + 40 + color * 2),
                SamusHyperBeamColorFormat.YellowFromGreen(ReadVerificationWord(bus, 0x8dd906 + 80 + color * 2)),
                "Every original yellow highlight derives from its green hue");
        for (int ink = 4; ink <= 7; ink++)
            AssertEqual(ReadVerificationWord(bus, 0x8dd906 + 2 * ink),
                HyperBeamFxColorFormat.RedHighlight(ReadVerificationWord(bus, 0x8dd90c), ReadVerificationWord(bus, 0x8dd906), ink),
                "Every original red highlight is a fifth-step white blend");
        for (int first = 0; first < 32768; first++)
        for (int second = 0; second < 32; second++)
        for (int ink = 4; ink <= 7; ink++)
        {
            double whiteWeight = (8 - ink) / 5.0;
            int expected = 0;
            for (int shift = 0; shift < 15; shift += 5)
                expected |= (int)Math.Round((first >> shift & 31) * (1 - whiteWeight) + second * whiteWeight) << shift;
            AssertEqual((ushort)expected, HyperBeamFxColorFormat.RedHighlight((ushort)first, (ushort)(second * 1057), ink),
                "All RGB5 first endpoints and independent channel pairs for every highlight weight");
        }
        foreach (int invalidInk in new[] { int.MinValue, -1, 0, 1, 2, 3, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => HyperBeamFxColorFormat.RedHighlight(0, 0, invalidInk), "Highlight ink bounds");
        foreach (var shade in new[] { (Frame: 0, Ink: 2), (Frame: 4, Ink: 5), (Frame: 8, Ink: 2), (Frame: 8, Ink: 5) })
            AssertEqual(ReadVerificationWord(bus, 0x8dd906 + 20 * shade.Frame + 2 * shade.Ink),
                SamusHyperBeamColorFormat.HueMidpoint(
                    ReadVerificationWord(bus, 0x8dd906 + 20 * shade.Frame + 2 * (shade.Ink - 1)),
                    ReadVerificationWord(bus, 0x8dd906 + 20 * shade.Frame + 2 * (shade.Ink + 1))),
                "Every original within-hue shade midpoint");
        ushort nativeRedEndpoint = ReadVerificationWord(bus, 0x8dd90c);
        AssertEqual(ReadVerificationWord(bus, 0x8dd964), HyperBeamFxColorFormat.GreenFromRed(nativeRedEndpoint), "Original saturated green endpoint");
        AssertEqual(ReadVerificationWord(bus, 0x8dd9a8), HyperBeamFxColorFormat.MagentaFromRed(nativeRedEndpoint), "Original saturated magenta endpoint");
        for (int rgb = 0; rgb < 32768; rgb++)
        {
            int redComponent = rgb % 32, greenComponent = rgb / 32 % 32, blueComponent = rgb / 1024;
            AssertEqual((ushort)(greenComponent + 32 * redComponent + 1024 * blueComponent), HyperBeamFxColorFormat.GreenFromRed((ushort)rgb), "Complete RGB5 red-to-green domain");
            AssertEqual((ushort)(redComponent + 32 * greenComponent + 1024 * redComponent), HyperBeamFxColorFormat.MagentaFromRed((ushort)rgb), "Complete RGB5 red-to-magenta domain");
        }
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
        foreach (string editScope in new[] { "all", "even", "green", "red", "white", "shade-sources" })
        {
            var document = JsonSerializer.Deserialize<HyperBeamFxColorDocument>(json, MapPresentationFormat.JsonOptions)!;
            for (int frame = 0; frame < 10; frame++)
            for (int color = 0; color < 8; color++)
            {
                if (editScope == "even" && (frame & 1) != 0 || editScope == "green" && frame != 4 ||
                    editScope == "red" && !(frame == 0 && color == 3) || editScope == "white" && !(frame == 0 && color == 0) ||
                    editScope == "shade-sources" && !(frame is 0 or 2 or 4 or 6 or 8 && color is 1 or 3 or 4 or 6)) continue;
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
                    bool changed = editScope == "all" || editScope == "even" && (frame & 1) == 0 || editScope == "green" && frame == 4 ||
                        editScope == "red" && frame == 0 && color == 3 || editScope == "white" && frame == 0 && color == 0 ||
                        editScope == "shade-sources" && frame is 0 or 2 or 4 or 6 or 8 && color is 1 or 3 or 4 or 6;
                    ushort expected = changed ? (ushort)(1000 + frame * 8 + color) :
                        ReadVerificationWord(bus, 0x8dd906 + 20 * frame + 2 * color);
                    AssertEqual(expected, result.Colors[225 + color], "All independent FX edits and endpoint-only edits survive shared hues");
                    bool middleShade = (frame & 1) == 0 && color == 2 || frame is 4 or 6 or 8 && color == 5;
                    bool transformedEndpoint = frame == 4 && color == 7 || frame == 8 && color == 1;
                    bool pairedOwner = !middleShade && !transformedEndpoint && (frame == 0 && color == 3 || frame == 4 && color >= 4 || frame == 8 && color != 0);
                    AssertEqual(pairedOwner, pairedInputs.ContainsKey(frame * 8 + color), "Original paired endpoint ownership");
                    AssertEqual((frame & 1) == 0 && color != 0 && !(frame == 2 && color >= 4) && !pairedOwner && !middleShade && !transformedEndpoint && !(frame == 0 && color >= 4), stored.ContainsKey(frame * 8 + color),
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
