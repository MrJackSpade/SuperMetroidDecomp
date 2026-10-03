using System.Text;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySamusHyperBeamColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Hyper Beam colors: cartridge comparison skipped (private ROM absent).");
            return;
        }

        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hyper Beam color oracle revision");
        SamusHyperBeamColorCatalog catalog = SamusHyperBeamColorCatalog.Load(
            new MemoryStream(SamusHyperBeamColorExtractor.Extract(rom)));
        var stored = (Dictionary<int, ushort>)typeof(SamusHyperBeamColorCatalog).GetField("colors",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(catalog)!;
        AssertEqual(6, stored.Count, "Hyper Beam whole inputs after cyan shadow midpoint calculation");
        var endpointInputs = (Dictionary<int, SamusHyperBeamColorCatalog.EndpointChannels>)typeof(SamusHyperBeamColorCatalog)
            .GetField("endpointInputs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(catalog)!;
        AssertEqual(9, endpointInputs.Count, "Three endpoint hues each have three independent shade inputs");
        int endpointComponentCount = 0;
        foreach (var entry in endpointInputs.Values)
            foreach (var field in typeof(SamusHyperBeamColorCatalog.EndpointChannels).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                if (field.GetValue(entry) is not null) endpointComponentCount++;
        AssertEqual(8, endpointComponentCount, "Only eight independent endpoint channels remain after middle-shadow calculation");
        var componentInputs = (Dictionary<int, LoadingPaletteInputView.Channels>)typeof(SamusHyperBeamColorCatalog)
            .GetField("intermediateInputs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(catalog)!;
        AssertEqual(6, componentInputs.Count, "Only independent differing intermediate colors carry component inputs");
        AssertTrue(!componentInputs.ContainsKey(2 * 16 + 13), "Magenta-cyan middle shadow needs no stored correction");
        int componentCount = 0;
        foreach (var entry in componentInputs.Values)
            foreach (var field in typeof(LoadingPaletteInputView.Channels).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                if (field.GetValue(entry) is not null) componentCount++;
        AssertEqual(7, componentCount, "Seven independently supplied intermediate components remain under review");
        var shadeInputs = (Dictionary<int, LoadingPaletteInputView.Channels>)typeof(SamusHyperBeamColorCatalog)
            .GetField("shadeInputs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(catalog)!;
        AssertEqual(3, shadeInputs.Count, "Only three original colors have differing shade channels");
        int shadeComponentCount = 0;
        foreach (var entry in shadeInputs.Values)
            foreach (var field in typeof(LoadingPaletteInputView.Channels).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                if (field.GetValue(entry) is not null) shadeComponentCount++;
        AssertEqual(3, shadeComponentCount, "Magenta blue shares its red shade input");
        for (int frame = 0; frame < SamusHyperBeamColorFormat.FrameCount; frame++)
        {
            int pointerAddress = SamusPaletteRomData.FullBodyCycles.HyperBeamPointers + frame * 2;
            ushort pointer = unchecked((ushort)(rom.ReadByte(pointerAddress) |
                rom.ReadByte(pointerAddress + 1) << 8));
            int paletteAddress = SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteSource(frame);
            AssertEqual(paletteAddress, SamusPaletteRomData.Banks.Palette | pointer,
                $"Hyper Beam frame {frame} compiled pointer");
            for (int index = 0; index < SamusHyperBeamColorFormat.ColorsPerFrame; index++)
            {
                int wordAddress = paletteAddress + index * 2;
                ushort native = unchecked((ushort)(rom.ReadByte(wordAddress) |
                    rom.ReadByte(wordAddress + 1) << 8));
                AssertEqual(native, catalog.Resolve(frame, index),
                    $"Hyper Beam frame {frame} color {index}");
                int sourceFrame = frame, sourceColor = index;
                if (index == 0)
                {
                    for (int candidate = 0; candidate < frame; candidate++)
                    {
                        ushort first = ReadVerificationWord(rom, 0x91d99e + candidate * 2);
                        if (ReadVerificationWord(rom, 0x9b0000 | first) == native) { sourceFrame = candidate; break; }
                    }
                }
                else
                    for (int candidate = 1; candidate < index; candidate++)
                        if (ReadVerificationWord(rom, paletteAddress + candidate * 2) == native) { sourceColor = candidate; break; }
                int source = sourceFrame * 16 + sourceColor;
                AssertEqual(source, SamusHyperBeamColorFormat.CanonicalColorIndex(frame, index), "Every native Hyper Beam alias");
                var shade = SamusHyperBeamColorFormat.ShadeSource(index);
                bool sharedShade = source == frame * 16 + index && shade.Ink != index;
                if (sharedShade)
                {
                    ushort shadow = ReadVerificationWord(rom, paletteAddress + 2 * shade.Ink);
                    bool differs = false;
                    string[] names = ["red", "green", "blue"];
                    for (int channel = 0; channel < 3; channel++)
                    {
                        int shift = 5 * channel, expectedChannel = frame == 1 && channel == 2 ? native & 31 : (shadow >> shift & 31) + shade.Brightness;
                        int? differing = (native >> shift & 31) == expectedChannel ? null : native >> shift & 31;
                        differs |= differing.HasValue;
                        object? actual = shadeInputs.TryGetValue(frame * 16 + index, out var shadeInput) ?
                            typeof(LoadingPaletteInputView.Channels).GetField(names[channel],
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(shadeInput) : null;
                        AssertTrue(Equals(differing, actual), "Only original differing shade components are stored");
                    }
                    AssertEqual(differs, shadeInputs.ContainsKey(frame * 16 + index), "Exact shade component ownership");
                }
                if (frame == 7 && index != 0)
                {
                    ushort greenPointer = ReadVerificationWord(rom, 0x91d9a8);
                    AssertEqual(native, SamusHyperBeamColorFormat.YellowFromGreen(ReadVerificationWord(rom,
                        0x9b0000 | (greenPointer + 2 * index))), "Every original green-to-yellow hue word");
                }
                bool midpoint = frame == 6 && index is not (0 or 1 or 8 or 11);
                if (midpoint)
                {
                    ushort greenPointer = ReadVerificationWord(rom, 0x91d9a8);
                    AssertEqual(native, SamusHyperBeamColorFormat.GreenYellowMidpoint(ReadVerificationWord(rom,
                        0x9b0000 | (greenPointer + 2 * index))), "Every original regular midpoint color");
                }
                bool cyanGreen = frame == 4 && index is not (0 or 7);
                if (cyanGreen)
                {
                    ushort cyanPointer = ReadVerificationWord(rom, 0x91d9a4), greenPointer = ReadVerificationWord(rom, 0x91d9a8);
                    AssertEqual(native, SamusHyperBeamColorFormat.HueMidpoint(
                        ReadVerificationWord(rom, 0x9b0000 | (cyanPointer + 2 * index)),
                        ReadVerificationWord(rom, 0x9b0000 | (greenPointer + 2 * index))), "Every original cyan-green midpoint word");
                }
                bool yellowRed = frame == 8 && index is not (0 or 1 or 7 or 8 or 11);
                if (yellowRed)
                {
                    ushort yellowPointer = ReadVerificationWord(rom, 0x91d9ac), redPointer = ReadVerificationWord(rom, 0x91d9b0);
                    AssertEqual(native, SamusHyperBeamColorFormat.HueMidpoint(
                        ReadVerificationWord(rom, 0x9b0000 | (yellowPointer + 2 * index)),
                        ReadVerificationWord(rom, 0x9b0000 | (redPointer + 2 * index))), "Every original yellow-red midpoint word");
                }
                bool remainingMidpoint = (frame == 0 && index is 4 or 5 or 9 or 13) || (frame == 2 && index == 7);
                if (remainingMidpoint)
                {
                    ushort firstPointer = ReadVerificationWord(rom, frame == 0 ? 0x91d9b0 : 0x91d9a0);
                    ushort secondPointer = ReadVerificationWord(rom, frame == 0 ? 0x91d9a0 : 0x91d9a4);
                    AssertEqual(native, SamusHyperBeamColorFormat.HueMidpoint(
                        ReadVerificationWord(rom, 0x9b0000 | (firstPointer + 2 * index)),
                        ReadVerificationWord(rom, 0x9b0000 | (secondPointer + 2 * index))), "Original cycle-wrap and magenta-cyan midpoint colors");
                }
                if (!sharedShade && index != 0 && (frame & 1) == 0 && source == frame * 16 + index && !(frame == 2 && index == 13))
                {
                    ushort before = ReadVerificationWord(rom, 0x91d99e + ((frame + 9) % 10) * 2);
                    ushort after = ReadVerificationWord(rom, 0x91d99e + (frame + 1) * 2);
                    ushort first = ReadVerificationWord(rom, 0x9b0000 | (before + 2 * index));
                    ushort second = ReadVerificationWord(rom, 0x9b0000 | (after + 2 * index));
                    int expected = 0;
                    for (int shift = 0; shift < 15; shift += 5)
                        expected |= (int)Math.Ceiling(((first >> shift & 31) + (second >> shift & 31)) / 2.0) << shift;
                    AssertEqual(native != expected, componentInputs.ContainsKey(frame * 16 + index), "Original differing channel ownership");
                    if (componentInputs.TryGetValue(frame * 16 + index, out var inputs))
                    {
                        string[] names = ["red", "green", "blue"];
                        for (int channel = 0; channel < 3; channel++)
                        {
                            int shift = channel * 5;
                            object? actual = typeof(LoadingPaletteInputView.Channels).GetField(names[channel],
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(inputs);
                            int? wanted = (native >> shift & 31) == (expected >> shift & 31) ? null : native >> shift & 31;
                            AssertTrue(Equals(wanted, actual), "Only original differing components are stored");
                        }
                    }
                }
                bool endpointOwner = !sharedShade && source == frame * 16 + index && index != 0 && frame is 1 or 5 or 9;
                AssertEqual(endpointOwner, endpointInputs.ContainsKey(frame * 16 + index), "Endpoint channel ownership");
                if (endpointOwner)
                {
                    var input = endpointInputs[frame * 16 + index];
                    var type = typeof(SamusHyperBeamColorCatalog.EndpointChannels);
                    object? red = type.GetField("red", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(input);
                    object? blue = type.GetField("blue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(input);
                    AssertTrue(blue is null, "Original endpoint blue is shared, never stored");
                    AssertTrue(Equals(frame == 9 || index == 13 ? null : (int?)(native & 31), red), "Original red endpoint is shared or a calculated middle shadow");
                    object? green = type.GetField("green", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(input);
                    AssertTrue(Equals(frame == 1 || index == 13 ? null : (int?)(native >> 5 & 31), green), "Original green endpoint is shared or a calculated middle shadow");
                    AssertEqual(native, input.Resolve(frame == 9, native & 31, native >> 5 & 31), "Original endpoint native channel reconstruction");
                }
                AssertEqual(!sharedShade && source == frame * 16 + index && !(frame == 7 && index != 0) && !(index != 0 && (frame & 1) == 0) && !(index != 0 && frame is 1 or 5 or 9) && !(frame == 3 && index == 13), stored.ContainsKey(frame * 16 + index), "Hyper Beam input ownership");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() =>
            SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteSource(10),
            "compiled Hyper Beam pointer table remains bounded");
        foreach (int invalid in new[] { -1, 10, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(invalid, 0), "Hyper Beam frame bounds");
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(0, invalid), "Hyper Beam color bounds");
        byte[] originalJson = SamusHyperBeamColorExtractor.Extract(rom);
        for (int rgb = 0; rgb < 32768; rgb++)
        {
            int green = rgb / 32 % 32, blue = rgb / 1024;
            foreach (int brightness in new[] { 2, 4, 7, 8 })
            {
                bool fits = rgb % 32 <= 31 - brightness && green <= 31 - brightness && blue <= 31 - brightness;
                AssertEqual(fits, SamusHyperBeamColorFormat.TryBrighten((ushort)rgb, brightness, out ushort brighter),
                    "Every RGB5 shade source accepts exactly the non-overflow domain");
                if (fits) AssertEqual((ushort)(rgb + brightness * 1057), brighter, "Every valid RGB5 uniform shade increment");
            }
            foreach (bool redHue in new[] { false, true })
            {
                var inputs = new SamusHyperBeamColorCatalog.EndpointChannels((ushort)rgb, redHue, rgb % 32);
                AssertEqual((ushort)rgb, inputs.Resolve(redHue, rgb % 32), "Every RGB5 endpoint input survives channel separation");
                var independent = new SamusHyperBeamColorCatalog.EndpointChannels((ushort)rgb, redHue, (rgb + 1) % 32);
                AssertEqual((ushort)rgb, independent.Resolve(redHue, (rgb + 1) % 32), "Every RGB5 endpoint preserves a differing source maximum");
            }
            var minimumInput = new SamusHyperBeamColorCatalog.EndpointChannels((ushort)rgb, false, 0, green);
            AssertEqual((ushort)rgb, minimumInput.Resolve(false, 0, green), "Every RGB5 shared endpoint minimum");
            var editedMinimum = new SamusHyperBeamColorCatalog.EndpointChannels((ushort)rgb, false, 0, (green + 1) % 32);
            AssertEqual((ushort)rgb, editedMinimum.Resolve(false, 0, (green + 1) % 32), "Every RGB5 independently edited endpoint minimum");
            AssertEqual((ushort)(green + 32 * green + 1024 * blue), SamusHyperBeamColorFormat.YellowFromGreen((ushort)rgb), "Complete RGB5 green-to-yellow domain");
            int midpoint = (int)Math.Ceiling((rgb % 32 + green) / 2.0);
            AssertEqual((ushort)(midpoint + 32 * green + 1024 * blue), SamusHyperBeamColorFormat.GreenYellowMidpoint((ushort)rgb), "Complete RGB5 hue midpoint domain");
            for (int other = 0; other < 32; other++)
            {
                int expected = (int)Math.Ceiling((rgb % 32 + other) / 2.0) +
                    32 * (int)Math.Ceiling((green + other) / 2.0) + 1024 * (int)Math.Ceiling((blue + other) / 2.0);
                AssertEqual((ushort)expected, SamusHyperBeamColorFormat.HueMidpoint((ushort)rgb, (ushort)(other * 1057)), "All RGB5 first colors and all independent per-channel midpoint pairs");
                int lower = (int)Math.Floor((rgb % 32 + other) / 2.0) +
                    32 * (int)Math.Floor((green + other) / 2.0) + 1024 * (int)Math.Floor((blue + other) / 2.0);
                AssertEqual((ushort)lower, SamusHyperBeamColorFormat.HueMidpoint((ushort)rgb, (ushort)(other * 1057), roundUp: false), "Complete downward RGB5 midpoint arithmetic");
            }
        }
        foreach (int invalid in new[] { int.MinValue, -1, 0, 1, 3, 5, 6, 9, 31, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.TryBrighten(0, invalid, out _), "Unsupported shade increments reject");
        foreach (int invalid in new[] { -1, 0, 2, 3, 4, 6, 7, 8, 10, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.EndpointSourceChannels(invalid, 3, 0, 0, 0), "Invalid endpoint hue");
        foreach (int invalid in new[] { -1, 0, 1, 2, 4, 10, 12, 14, 15, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.EndpointSourceChannels(1, invalid, 0, 0, 0), "Invalid endpoint shadow ink");
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.EndpointSourceChannels(1, 13, invalid, 0, 0), "Invalid green-hue input");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.EndpointSourceChannels(1, 13, 0, invalid, 0), "Invalid low shadow input");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.EndpointSourceChannels(1, 13, 0, 0, invalid), "Invalid high shadow input");
        }
        foreach (int shadowFrame in new[] { 1, 2, 3, 5, 9 })
        foreach (int ink in new[] { 3, 11, 13 })
        for (int channel = 0; channel < 3; channel++)
        for (int intensity = 0; intensity < 32; intensity++)
        {
            var document = JsonSerializer.Deserialize<SamusHyperBeamColorDocument>(originalJson, MapPresentationFormat.JsonOptions)!;
            ushort shadowPointer = ReadVerificationWord(rom, 0x91d99e + 2 * shadowFrame);
            ushort original = ReadVerificationWord(rom, 0x9b0000 | (shadowPointer + 2 * ink));
            ushort changed = (ushort)((original & ~(31 << (channel * 5))) | intensity << (channel * 5));
            document.Frames[shadowFrame][ink] = new PaletteRgb5 { Red = changed & 31, Green = changed >> 5 & 31, Blue = changed >> 10 };
            var editedCyan = SamusHyperBeamColorCatalog.Load(new MemoryStream(SamusHyperBeamColorCatalog.Write(document)));
            for (int frame = 0; frame < 10; frame++)
            for (int color = 0; color < 16; color++)
            {
                ushort pointer = ReadVerificationWord(rom, 0x91d99e + frame * 2);
                ushort expected = frame == shadowFrame && color == ink ? changed : ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
                AssertEqual(expected, editedCyan.Resolve(frame, color), "Shadow endpoint/middle edits preserve every independently supplied color");
            }
        }
        foreach (string scope in new[] { "all", "aliases", "shades" })
        {
            var document = JsonSerializer.Deserialize<SamusHyperBeamColorDocument>(originalJson, MapPresentationFormat.JsonOptions)!;
            for (int frame = 0; frame < 10; frame++)
            for (int color = 0; color < 16; color++)
            {
                bool edit = scope == "all" || (scope == "aliases" && (color is 2 or 3 or 10 || (frame == 2 && color == 0))) ||
                    (scope == "shades" && color is 3 or 11 or 13);
                if (!edit) continue;
                int word = 2000 + frame * 16 + color;
                document.Frames[frame][color] = new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
            }
            var edited = SamusHyperBeamColorCatalog.Load(new MemoryStream(SamusHyperBeamColorCatalog.Write(document)));
            for (int frame = 0; frame < 10; frame++)
            for (int color = 0; color < 16; color++)
            {
                ushort pointer = ReadVerificationWord(rom, 0x91d99e + frame * 2);
                bool edit = scope == "all" || (scope == "aliases" && (color is 2 or 3 or 10 || (frame == 2 && color == 0))) ||
                    (scope == "shades" && color is 3 or 11 or 13);
                ushort expected = edit ?
                    (ushort)(2000 + frame * 16 + color) : ReadVerificationWord(rom, 0x9b0000 | (pointer + color * 2));
                AssertEqual(expected, edited.Resolve(frame, color), "Every independently supplied Hyper Beam color survives aliases/source edits");
            }
        }

        foreach (int sourceFrame in new[] { 1, 3, 5, 7, 9 })
        {
        var greenEdit = JsonSerializer.Deserialize<SamusHyperBeamColorDocument>(originalJson, MapPresentationFormat.JsonOptions)!;
        for (int color = 0; color < 16; color++) greenEdit.Frames[sourceFrame][color] = new PaletteRgb5 { Red = color, Green = 31 - color, Blue = color + 1 };
        var greenEdited = SamusHyperBeamColorCatalog.Load(new MemoryStream(SamusHyperBeamColorCatalog.Write(greenEdit)));
        for (int frame = 0; frame < 10; frame++)
        for (int color = 0; color < 16; color++)
        {
            ushort pointer = ReadVerificationWord(rom, 0x91d99e + frame * 2);
            ushort expected = frame == sourceFrame ? (ushort)(color | (31 - color) << 5 | (color + 1) << 10) :
                ReadVerificationWord(rom, 0x9b0000 | (pointer + color * 2));
            AssertEqual(expected, greenEdited.Resolve(frame, color), "Hue-source-only edits preserve supplied intermediate and all other frames");
        }

        }
        byte[] chargeJson = SamusChargeColorExtractor.Extract(rom);
        var shot = SamusChargeColorCatalog.Load(new MemoryStream(chargeJson));
        AssertEqual(typeof(SamusHyperBeamColorCatalog), typeof(SamusChargeColorCatalog).GetField("hyperShot",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.FieldType,
            "Hyper-shot owns the shared calculated representation, not another whole palette table");
        var shotCgram = new SnesCgram();
        for (int frame = 0; frame < 10; frame++)
        {
            ushort pointer = ReadVerificationWord(rom, 0x91d83d - 2 * frame);
            AssertEqual(ReadVerificationWord(rom, 0x91d99e + 2 * (9 - frame)), pointer,
                "Both native pointer views address the same rows in reverse playback order");
            shot.ApplyHyper(shotCgram, frame);
            for (int color = 0; color < 16; color++)
            {
                ushort expected = ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
                AssertEqual(expected, shot.ResolveHyper(frame, color), "Every original Hyper-shot color");
                AssertEqual(expected, shotCgram.Colors[192 + color], "Every original Hyper-shot applied color");
            }
        }
        foreach (int invalid in new[] { -1, 10, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => shot.ResolveHyper(invalid, 0), "Hyper-shot frame bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => shot.ApplyHyper(shotCgram, invalid), "Hyper-shot apply frame bounds");
        }
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => shot.ResolveHyper(0, invalid), "Hyper-shot color bounds");
        foreach (bool sourcesOnly in new[] { false, true })
        {
            var document = JsonSerializer.Deserialize<SamusChargeColorDocument>(chargeJson, MapPresentationFormat.JsonOptions)!;
            for (int frame = 0; frame < 10; frame++)
            for (int color = 0; color < 16; color++)
            {
                if (sourcesOnly && !(frame == 4 || color is 3 or 11 or 13)) continue;
                int word = 1000 + frame * 16 + color;
                document.HyperShot[frame][color] = new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
            }
            var edited = SamusChargeColorCatalog.Load(new MemoryStream(SamusChargeColorCatalog.Write(document)));
            for (int frame = 0; frame < 10; frame++)
            for (int color = 0; color < 16; color++)
            {
                ushort pointer = ReadVerificationWord(rom, 0x91d83d - 2 * frame);
                ushort expected = !sourcesOnly || frame == 4 || color is 3 or 11 or 13 ?
                    (ushort)(1000 + frame * 16 + color) : ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
                AssertEqual(expected, edited.ResolveHyper(frame, color), "Reverse-view independent edits preserve all supplied colors");
                AssertEqual(ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color)), catalog.Resolve(9 - frame, color),
                    "Shot edits cannot mutate the separate full-body asset");
            }
        }
        var shotSamus = new SamusState { HyperBeam = 0x8000, ChargeColors = shot };
        var projectiles = new SamusProjectileSystem();
        typeof(SamusProjectileSystem).GetProperty(nameof(SamusProjectileSystem.ChargedShotGlowTimer))!.SetValue(projectiles, (ushort)0x8014);
        var shotGuard = new ForbiddenHyperBeamColorBus(rom);
        for (int call = 0; call < 20; call++)
        {
            var step = projectiles.UpdateBeamChargePalette(shotGuard, shotCgram, shotSamus);
            AssertEqual((call & 1) == 0 ? SamusBeamChargePaletteAction.HyperPalette : SamusBeamChargePaletteAction.HyperHold,
                step.Action, "Original Hyper-shot alternating paint/hold cadence");
            ushort pointer = ReadVerificationWord(rom, 0x91d83d - 2 * (call / 2));
            for (int color = 0; color < 16; color++)
                AssertEqual(ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color)), shotCgram.Colors[192 + color],
                    "Guarded Hyper-shot cycle preserves original applied colors and held rows");
        }
        AssertEqual(0, shotGuard.ForbiddenReads, "Hyper-shot calculated colors avoid original palette and pointer reads");
        var installedSamus = new SamusState();
        installedSamus.Drained.EnableRainbow(installedSamus);
        installedSamus.Drained.PresentationColors = catalog;
        var installedCgram = new SnesCgram();
        var guarded = new ForbiddenHyperBeamColorBus(rom);
        for (int call = 0; call < 22; call++)
        {
            AssertTrue(installedSamus.Drained.UpdatePalette(guarded, installedCgram, 0),
                $"Hyper Beam call {call} owns palette");
            // Original91D96F copies the selected row before DEC; EnableRainbow's
            // timer/reload are1, so every call advances and wraps at native limit10.
            AssertEqual((ushort)((call + 1) % 10), installedSamus.Drained.ChargePaletteIndex,
                $"Hyper Beam call {call} next frame");
            AssertEqual((ushort)1, installedSamus.Drained.CommonPaletteTimer,
                $"Hyper Beam call {call} native timer");
            ushort originalPointer = ReadVerificationWord(rom, 0x91d99e + 2 * (call % 10));
            for (int color = 0; color < 16; color++)
                AssertEqual((ushort)(ReadVerificationWord(rom, 0x9b0000 | (originalPointer + 2 * color)) & 0x7fff),
                    installedCgram.Colors[192 + color], $"Hyper Beam call {call} original CGRAM color {color}");
        }
        AssertEqual(0, guarded.ForbiddenReads,
            "installed Hyper Beam cycle does not read pointer or palette ROM tables");
        AssertThrows<InvalidDataException>(() => SamusHyperBeamColorCatalog.Load(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate Hyper Beam JSON property fails loudly");
        AssertThrows<InvalidDataException>(() => SamusHyperBeamColorCatalog.Load(
            new MemoryStream([1, 2, 3])), "corrupt Hyper Beam JSON fails loudly");
        Console.WriteLine("  Hyper Beam: both native palette views, independent edits and guarded runtime cycles pass.");
    }

    private static void VerifySamusHyperBeamColorOverride(string stock, string overrides,
        AreaMapPresentationCatalog baseline, ISnesAddressSpace rom,
        GameplayBasePaletteCatalog initialPalettes,
        MapPresentationInstalledRoomAssets fixtureAssets)
    {
        string path = Path.Combine(overrides, SamusHyperBeamColorFormat.FileName);
        SamusHyperBeamColorDocument document = JsonSerializer.Deserialize<SamusHyperBeamColorDocument>(
            File.ReadAllBytes(Path.Combine(stock, SamusHyperBeamColorFormat.FileName)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock Hyper Beam color document is null.");
        PaletteRgb5 original = document.Frames[4][7];
        document.Frames[4][7] = original with { Red = (original.Red + 1) % 32 };
        File.WriteAllBytes(path, SamusHyperBeamColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != baseline.ContentIdentity,
            "Hyper Beam color override changes installed content identity");

        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(rom,
            initialPaletteArt: initialPalettes)
        {
            MapPresentation = edited,
        };
        fixtureAssets.Bind(runtime);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        SamusState samus = runtime.Samus ??
            throw new InvalidOperationException("Ceres initialization did not construct Samus.");
        AssertTrue(ReferenceEquals(edited.SamusHyperBeamColors, samus.Drained.PresentationColors),
            "new Samus binds installed Hyper Beam colors");
        samus.Drained.EnableRainbow(samus);
        var cgram = new SnesCgram();
        var guarded = new ForbiddenHyperBeamColorBus(rom);
        for (int frame = 0; frame <= 4; frame++)
            AssertTrue(samus.Drained.UpdatePalette(guarded, cgram, samus.EquippedItems),
                $"edited Hyper Beam frame {frame} writes colors");
        AssertEqual(edited.SamusHyperBeamColors.Resolve(4, 7), cgram.Colors[199],
            "edited Hyper Beam color reaches CGRAM on its selected frame");
        AssertEqual(0, guarded.ForbiddenReads,
            "edited Hyper Beam colors avoid pointer and palette ROM tables");
        AssertThrows<InvalidDataException>(() => SamusHyperBeamColorCatalog.Write(document with
        {
            Frames = [.. document.Frames.Take(4),
                [.. document.Frames[4].Take(7), original with { Red = 32 },
                 .. document.Frames[4].Skip(8)], .. document.Frames.Skip(5)],
        }), "out-of-range Hyper Beam RGB5 fails loudly");
        File.Delete(path);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing Hyper Beam override restores stock content identity");
        Console.WriteLine("Hyper Beam override: edited frame reaches CGRAM and stock restores.");
    }

    private sealed class ForbiddenHyperBeamColorBus(ISnesAddressSpace inner) : ISnesAddressSpace, IImportCartridgeSource
    {
        public int ForbiddenReads { get; private set; }
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            int pointerStart = SamusPaletteRomData.FullBodyCycles.HyperBeamPointers;
            int paletteStart = SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteSource(9);
            int paletteEnd = SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteSource(0) +
                SamusHyperBeamColorFormat.ColorsPerFrame * sizeof(ushort);
            if ((address >= 0x91d829 && address < 0x91d83f) ||
                (address >= pointerStart && address < pointerStart +
                    SamusHyperBeamColorFormat.FrameCount * sizeof(ushort)) ||
                (address >= paletteStart && address < paletteEnd))
            {
                ForbiddenReads++;
                throw new InvalidOperationException($"Installed Hyper Beam read native artwork ${address:X6}.");
            }
            return inner.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
