using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Samus.Palettes;

using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Retail and installed-artwork checks for Samus's ten-frame Hyper Beam rainbow palette.</summary>
internal static class SamusHyperBeamColorsVerification
{
    /// <summary>Checks extracted RGB5 endpoint sharing, native CGRAM rows and guarded rainbow playback against the supported cartridge.</summary>
    internal static void VerifySamusHyperBeamColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Hyper Beam colors: cartridge comparison skipped (private ROM absent).");
            return;
        }

        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hyper Beam color oracle revision");
        InstalledSamusHyperBeamColors catalog = SamusHyperBeamColorFile.Read(
            new MemoryStream(SamusHyperBeamColorExtractor.Extract(rom)));
        var stored = catalog.colors;
        AssertEqual(6, stored.Count, "Hyper Beam whole inputs after cyan shadow midpoint calculation");
        var endpointInputs = catalog.endpointInputs;
        AssertEqual(9, endpointInputs.Count, "Three endpoint hues each have three independent shade inputs");
        int endpointComponentCount = 0;
        foreach (var entry in endpointInputs.Values)
            foreach (var field in MetadataShape.Fields(typeof(InstalledSamusHyperBeamColors.EndpointChannels), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                if (field.GetValue(entry) is not null) endpointComponentCount++;
        AssertEqual(8, endpointComponentCount, "Only eight independent endpoint channels remain after middle-shadow calculation");
        var componentInputs = catalog.intermediateInputs;
        AssertEqual(0, componentInputs.Count, "Every native intermediate shadow is calculated without stored corrections");
        var shadeInputs = catalog.shadeInputs;
        AssertEqual(3, shadeInputs.Count, "Only three original colors have differing shade channels");
        int shadeComponentCount = 0;
        foreach (var entry in shadeInputs.Values)
            foreach (var field in MetadataShape.Fields(typeof(LoadingPaletteInputView.Channels), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                if (field.GetValue(entry) is not null) shadeComponentCount++;
        AssertEqual(3, shadeComponentCount, "Magenta blue shares its red shade input");
        for (int frame = 0; frame < SamusHyperBeamColorFormat.FrameCount; frame++)
        {
            int pointerAddress = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPointers + frame * 2;
            ushort pointer = rom.ReadCartridgeWord(pointerAddress);
            int paletteAddress = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteSource(frame);
            AssertEqual(paletteAddress, SamusSourceDefinitions.PaletteBank.Value | pointer,
                $"Hyper Beam frame {frame} compiled pointer");
            for (int index = 0; index < SamusHyperBeamColorFormat.ColorsPerFrame; index++)
            {
                int wordAddress = paletteAddress + index * 2;
                ushort native = rom.ReadCartridgeWord(wordAddress);
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
                    for (int channel = 0; channel < 3; channel++)
                    {
                        int shift = 5 * channel, expectedChannel = frame == 1 && channel == 2 ? native & 31 : (shadow >> shift & 31) + shade.Brightness;
                        int? differing = (native >> shift & 31) == expectedChannel ? null : native >> shift & 31;
                        differs |= differing.HasValue;
                        object? actual = shadeInputs.TryGetValue(frame * 16 + index, out var shadeInput) ?
                            shadeInput.Component(channel) : null;
                        AssertTrue(Equals(differing, actual), "Only original differing shade components are stored");
                    }
                    AssertEqual(differs, shadeInputs.ContainsKey(frame * 16 + index), "Exact shade component ownership");
                }
                if (frame == 7 && index != 0)
                {
                    ushort greenPointer = ReadVerificationWord(rom, 0x91d9a8);
                    AssertEqual(native, HyperBeamHues.YellowFromGreen(RgbColor.FromWord(ReadVerificationWord(rom,
                        0x9b0000 | (greenPointer + 2 * index)))), "Every original green-to-yellow hue word");
                }
                bool midpoint = frame == 6 && index is not (0 or 1 or 8 or 11);
                if (midpoint)
                {
                    ushort greenPointer = ReadVerificationWord(rom, 0x91d9a8);
                    AssertEqual(native, SamusHyperBeamColorFormat.GreenYellowMidpoint(RgbColor.FromWord(ReadVerificationWord(rom,
                        0x9b0000 | (greenPointer + 2 * index)))), "Every original regular midpoint color");
                }
                bool cyanGreen = frame == 4 && index is not (0 or 7);
                if (cyanGreen)
                {
                    ushort cyanPointer = ReadVerificationWord(rom, 0x91d9a4), greenPointer = ReadVerificationWord(rom, 0x91d9a8);
                    AssertEqual(native, HyperBeamHues.HueMidpoint(
                        RgbColor.FromWord(checked(ReadVerificationWord(rom, 0x9b0000 | (cyanPointer + 2 * index)))),
                        RgbColor.FromWord(checked(ReadVerificationWord(rom, 0x9b0000 | (greenPointer + 2 * index))))), "Every original cyan-green midpoint word");
                }
                bool yellowRed = frame == 8 && index is not (0 or 1 or 7 or 8 or 11);
                if (yellowRed)
                {
                    ushort yellowPointer = ReadVerificationWord(rom, 0x91d9ac), redPointer = ReadVerificationWord(rom, 0x91d9b0);
                    AssertEqual(native, HyperBeamHues.HueMidpoint(
                        RgbColor.FromWord(checked(ReadVerificationWord(rom, 0x9b0000 | (yellowPointer + 2 * index)))),
                        RgbColor.FromWord(checked(ReadVerificationWord(rom, 0x9b0000 | (redPointer + 2 * index))))), "Every original yellow-red midpoint word");
                }
                bool remainingMidpoint = (frame == 0 && index is 4 or 5 or 9 or 13) || (frame == 2 && index == 7);
                if (remainingMidpoint)
                {
                    ushort firstPointer = ReadVerificationWord(rom, frame == 0 ? 0x91d9b0 : 0x91d9a0);
                    ushort secondPointer = ReadVerificationWord(rom, frame == 0 ? 0x91d9a0 : 0x91d9a4);
                    AssertEqual(native, HyperBeamHues.HueMidpoint(
                        RgbColor.FromWord(checked(ReadVerificationWord(rom, 0x9b0000 | (firstPointer + 2 * index)))),
                        RgbColor.FromWord(checked(ReadVerificationWord(rom, 0x9b0000 | (secondPointer + 2 * index))))), "Original cycle-wrap and magenta-cyan midpoint colors");
                }
                bool endpointOwner = !sharedShade && source == frame * 16 + index && index != 0 && frame is 1 or 5 or 9;
                AssertEqual(endpointOwner, endpointInputs.ContainsKey(frame * 16 + index), "Endpoint channel ownership");
                if (endpointOwner)
                {
                    var input = endpointInputs[frame * 16 + index];
                    int? red = input.red;
                    int? blue = input.blue;
                    AssertTrue(blue is null, "Original endpoint blue is shared, never stored");
                    AssertTrue(Equals(frame == 9 || index == 13 ? null : (int?)(native & 31), red), "Original red endpoint is shared or a calculated middle shadow");
                    int? green = input.green;
                    AssertTrue(Equals(frame == 1 || index == 13 ? null : (int?)(native >> 5 & 31), green), "Original green endpoint is shared or a calculated middle shadow");
                    AssertEqual(native, input.Resolve(frame == 9, native & 31, native >> 5 & 31), "Original endpoint native channel reconstruction");
                }
                AssertEqual(!sharedShade && source == frame * 16 + index && !(frame == 7 && index != 0) && !(index != 0 && (frame & 1) == 0) && !(index != 0 && frame is 1 or 5 or 9) && !(frame == 3 && index == 13), stored.ContainsKey(new TableColourPlace(frame, index)), "Hyper Beam input ownership");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() =>
            SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteSource(10),
            "compiled Hyper Beam pointer table remains bounded");
        foreach (int invalid in new[] { -1, 10, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(invalid, 0), "Hyper Beam frame bounds");
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(0, invalid), "Hyper Beam color bounds");
        for (int rgb = 0; rgb < 32768; rgb++)
        {
            int green = rgb / 32 % 32, blue = rgb / 1024;
            foreach (int ink in new[] { 3, 11 })
            {
                bool fits = ink == 3 ? green > 0 : rgb % 32 < 31 && green < 31 && blue < 31;
                AssertEqual(fits, SamusHyperBeamColorFormat.TryCyanTransitionShadow(RgbColor.FromWord(checked((ushort)rgb)), ink, out RgbColor shadow), "Complete transition-shadow acceptance domain");
                AssertEqual(fits ? (ushort)(rgb + (ink == 3 ? -32 : 1057)) : (ushort)0, shadow, "Transition shadows preserve channel arithmetic and reject overflow");
            }
            foreach (int brightness in new[] { 2, 4, 7, 8 })
            {
                bool fits = rgb % 32 <= 31 - brightness && green <= 31 - brightness && blue <= 31 - brightness;
                AssertEqual(fits, SamusHyperBeamColorFormat.TryBrighten(RgbColor.FromWord(checked((ushort)rgb)), brightness, out RgbColor brighter),
                    "Every RGB5 shade source accepts exactly the non-overflow domain");
                if (fits) AssertEqual((ushort)(rgb + brightness * 1057), brighter, "Every valid RGB5 uniform shade increment");
            }
            foreach (bool redHue in new[] { false, true })
            {
                var inputs = new InstalledSamusHyperBeamColors.EndpointChannels(RgbColor.FromWord(checked((ushort)rgb)), redHue, rgb % 32);
                AssertEqual((ushort)rgb, inputs.Resolve(redHue, rgb % 32), "Every RGB5 endpoint input survives channel separation");
            }
            var minimumInput = new InstalledSamusHyperBeamColors.EndpointChannels(RgbColor.FromWord(checked((ushort)rgb)), false, 0, green);
            AssertEqual((ushort)rgb, minimumInput.Resolve(false, 0, green), "Every RGB5 shared endpoint minimum");
            AssertEqual((ushort)(green + 32 * green + 1024 * blue), HyperBeamHues.YellowFromGreen(RgbColor.FromWord(checked((ushort)rgb))), "Complete RGB5 green-to-yellow domain");
            int midpoint = (int)Math.Ceiling((rgb % 32 + green) / 2.0);
            AssertEqual((ushort)(midpoint + 32 * green + 1024 * blue), SamusHyperBeamColorFormat.GreenYellowMidpoint(RgbColor.FromWord(checked((ushort)rgb))), "Complete RGB5 hue midpoint domain");
            for (int other = 0; other < 32; other++)
            {
                int expected = (int)Math.Ceiling((rgb % 32 + other) / 2.0) +
                    32 * (int)Math.Ceiling((green + other) / 2.0) + 1024 * (int)Math.Ceiling((blue + other) / 2.0);
                AssertEqual((ushort)expected, HyperBeamHues.HueMidpoint(RgbColor.FromWord(checked((ushort)rgb)), RgbColor.FromWord((ushort)(other * 1057))), "All RGB5 first colors and all independent per-channel midpoint pairs");
                int lower = (int)Math.Floor((rgb % 32 + other) / 2.0) +
                    32 * (int)Math.Floor((green + other) / 2.0) + 1024 * (int)Math.Floor((blue + other) / 2.0);
                AssertEqual((ushort)lower, HyperBeamHues.HueMidpoint(RgbColor.FromWord(checked((ushort)rgb)), RgbColor.FromWord((ushort)(other * 1057)), direction: HueRounding.Down), "Complete downward RGB5 midpoint arithmetic");
                AssertEqual((ushort)((lower & 31) | (expected & 0x7fe0)),
                    SamusHyperBeamColorFormat.CyanTransitionMiddle(RgbColor.FromWord(checked((ushort)rgb)), RgbColor.FromWord((ushort)(other * 1057))), "Complete cyan-biased midpoint channel pairs");
            }
        }
        foreach (int invalid in new[] { int.MinValue, -1, 0, 1, 3, 5, 6, 9, 31, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.TryBrighten(new RgbColor(0, 0, 0), invalid, out _), "Unsupported shade increments reject");
        foreach (int invalid in new[] { -1, 0, 2, 3, 4, 6, 7, 8, 10, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.EndpointSourceChannels(invalid, 3, new RgbColor(0, 0, 0), new RgbColor(0, 0, 0), new RgbColor(0, 0, 0)), "Invalid endpoint hue");
        foreach (int invalid in new[] { -1, 0, 1, 2, 4, 10, 12, 14, 15, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.EndpointSourceChannels(1, invalid, new RgbColor(0, 0, 0), new RgbColor(0, 0, 0), new RgbColor(0, 0, 0)), "Invalid endpoint shadow ink");
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.EndpointSourceChannels(1, 13, RgbColor.FromWord(checked(invalid)), new RgbColor(0, 0, 0), new RgbColor(0, 0, 0)), "Invalid green-hue input");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.EndpointSourceChannels(1, 13, new RgbColor(0, 0, 0), RgbColor.FromWord(checked(invalid)), new RgbColor(0, 0, 0)), "Invalid low shadow input");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.EndpointSourceChannels(1, 13, new RgbColor(0, 0, 0), new RgbColor(0, 0, 0), RgbColor.FromWord(checked(invalid))), "Invalid high shadow input");
        }
        for (int low = 0; low < 32; low++)
        for (int middle = 0; middle < 32; middle++)
        {
            ushort green = (ushort)(low | middle << 5 | (31 - middle) << 10);
            bool valid = SamusHyperBeamColorFormat.TryGreenYellowHighShadow(RgbColor.FromWord(checked((ushort)low)), RgbColor.FromWord(checked((ushort)middle)), RgbColor.FromWord(checked(green)), out RgbColor result);
            AssertEqual(middle - low >= -middle && middle - low <= 31 - middle, valid, "Equal shadow step accepts precisely RGB5 red results");
            if (valid)
            {
                AssertEqual(middle - low, (result.ToWord() & 31) - middle, "Both red shadow intervals are equal");
                AssertEqual(green & 0x7fe0, result.ToWord() & 0x7fe0, "Green-yellow upper shadow preserves green/blue");
            }
            else AssertEqual(0, result, "Unrepresentable shadow is not clamped or wrapped");
        }
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.TryGreenYellowHighShadow(RgbColor.FromWord(checked(invalid)), new RgbColor(0, 0, 0), new RgbColor(0, 0, 0), out _), "Invalid lower shadow");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.TryGreenYellowHighShadow(new RgbColor(0, 0, 0), RgbColor.FromWord(checked(invalid)), new RgbColor(0, 0, 0), out _), "Invalid middle shadow");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.TryGreenYellowHighShadow(new RgbColor(0, 0, 0), new RgbColor(0, 0, 0), RgbColor.FromWord(checked(invalid)), out _), "Invalid green source");
        }
        foreach (int invalid in new[] { -1, 0, 1, 2, 4, 10, 12, 13, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.TryCyanTransitionShadow(new RgbColor(0, 0, 0), invalid, out _), "Invalid transition shadow selector");
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.CyanTransitionMiddle(RgbColor.FromWord(checked(invalid)), new RgbColor(0, 0, 0)), "Invalid transition magenta endpoint");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.CyanTransitionMiddle(new RgbColor(0, 0, 0), RgbColor.FromWord(checked(invalid))), "Invalid transition cyan endpoint");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusHyperBeamColorFormat.TryCyanTransitionShadow(RgbColor.FromWord(checked(invalid)), 3, out _), "Invalid transition middle ink");
        }
        byte[] chargeJson = SamusChargeColorExtractor.Extract(rom);
        var shot = SamusChargeColorFile.Read(new MemoryStream(chargeJson)).ToColours();
        AssertEqual(typeof(InstalledSamusHyperBeamColors), MetadataShape.FieldType(typeof(InstalledSamusChargeColors), "hyperShot", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic),
            "Hyper-shot owns the shared calculated representation, not another whole palette table");
        var shotColours = new SamusColours();
        // The Hyper shot names shades (#1512); the import resolves them from these charge colours.
        var shotSources = InstalledFixtures.SamusShadeSources with { Charge = shot };
        for (int frame = 0; frame < 10; frame++)
        {
            ushort pointer = ReadVerificationWord(rom, 0x91d83d - 2 * frame);
            AssertEqual(ReadVerificationWord(rom, 0x91d99e + 2 * (9 - frame)), pointer,
                "Both native pointer views address the same rows in reverse playback order");
            shotColours.ShowSuit(SamusShades.HyperShot(SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount - 1 - frame));
            for (int color = 0; color < 16; color++)
            {
                ushort expected = ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
                AssertEqual(expected, shot.ResolveHyper(frame, color), "Every original Hyper-shot color");
                AssertEqual(expected, shotColours.ShownSuit(shotSources)[color], "Every original Hyper-shot shade color");
            }
        }
        foreach (int invalid in new[] { -1, 10, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => shot.ResolveHyper(invalid, 0), "Hyper-shot frame bounds");
        }
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => shot.ResolveHyper(0, invalid), "Hyper-shot color bounds");
        var shotSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { HyperBeamEnabled = true };
        var projectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads);
        projectiles.Charge.ChargedShotGlowTimer = WrappingCountdown.Start(0x8014);
        var shotGuard = NewForbiddenHyperBeamColorBus(rom);
        for (int call = 0; call < 20; call++)
        {
            var step = projectiles.UpdateBeamChargePalette(shotColours, shotSamus);
            AssertEqual((call & 1) == 0 ? SamusBeamChargePaletteAction.HyperPalette : SamusBeamChargePaletteAction.HyperHold,
                step.Action, "Original Hyper-shot alternating paint/hold cadence");
            ushort pointer = ReadVerificationWord(rom, 0x91d83d - 2 * (call / 2));
            for (int color = 0; color < 16; color++)
                AssertEqual(ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color)), shotColours.ShownSuit(shotSources)[color],
                    "Guarded Hyper-shot cycle preserves original shade colors and held rows");
        }
        AssertEqual(0, shotGuard.ForbiddenReadAttempts, "Hyper-shot calculated colors avoid original palette and pointer reads");
        var installedSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence());
        installedSamus.Drained.EnableRainbow(installedSamus);
        var installedSources = InstalledFixtures.SamusShadeSources with { HyperBeamSuit = catalog.ToColours() };
        var installedColours = new SamusColours();
        var guarded = NewForbiddenHyperBeamColorBus(rom);
        for (int call = 0; call < 22; call++)
        {
            AssertTrue(installedSamus.Drained.UpdatePalette(installedColours, 0),
                $"Hyper Beam call {call} owns palette");
            // Original91D96F copies the selected row before DEC; EnableRainbow's
            // timer/reload are1, so every call advances and wraps at native limit10.
            AssertEqual((ushort)((call + 1) % 10), installedSamus.Drained.ChargePaletteIndex,
                $"Hyper Beam call {call} next frame");
            AssertEqual((ushort)1, installedSamus.Drained.CommonPaletteTimer.RawValue,
                $"Hyper Beam call {call} native timer");
            ushort originalPointer = ReadVerificationWord(rom, 0x91d99e + 2 * (call % 10));
            for (int color = 0; color < 16; color++)
                AssertEqual((ushort)(ReadVerificationWord(rom, 0x9b0000 | (originalPointer + 2 * color)) & 0x7fff),
                    installedColours.ShownSuit(installedSources)[color], $"Hyper Beam call {call} original shade color {color}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed Hyper Beam cycle does not read pointer or palette ROM tables");
        AssertThrows<InvalidDataException>(() => SamusHyperBeamColorFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate Hyper Beam JSON property fails loudly");
        AssertThrows<InvalidDataException>(() => SamusHyperBeamColorFile.Read(
            new MemoryStream([1, 2, 3])), "corrupt Hyper Beam JSON fails loudly");
        Console.WriteLine("  Hyper Beam: both native palette views and guarded runtime cycles pass.");
    }

    /// <summary>Checks that the Hyper Beam colour codec rejects out-of-range colours.</summary>
    internal static void VerifySamusHyperBeamColorsRejectMalformedDocuments(string stock)
    {
        SamusHyperBeamColorDocument document = JsonSerializer.Deserialize<SamusHyperBeamColorDocument>(
            File.ReadAllBytes(Path.Combine(stock, SamusHyperBeamColorFormat.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Hyper Beam color document is null.");
        AssertThrows<InvalidDataException>(() => SamusHyperBeamColorFile.Read(new MemoryStream(SamusHyperBeamColorFile.Write(document with
        {
            Frames = [.. document.Frames.Take(4),
                [.. document.Frames[4].Take(7), document.Frames[4][7] with { Red = 32 },
                 .. document.Frames[4].Skip(8)], .. document.Frames.Skip(5)],
        }))), "out-of-range Hyper Beam RGB5 fails loudly");
    }

    /// <summary>Blocks native Hyper Beam pointer and palette tables while installed colors update Samus.</summary>
    private static CartridgeReadGuard NewForbiddenHyperBeamColorBus(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int pointerStart = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPointers;
                int paletteStart = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteSource(9);
                int paletteEnd = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteSource(0) +
                    SamusHyperBeamColorFormat.ColorsPerFrame * sizeof(ushort);
                return (address >= 0x91d829 && address < 0x91d83f) ||
                    (address >= pointerStart && address < pointerStart +
                        SamusHyperBeamColorFormat.FrameCount * sizeof(ushort)) ||
                    (address >= paletteStart && address < paletteEnd);
            },
            address => $"Installed Hyper Beam read native artwork ${address:X6}.");

    /// <summary>Runs Samus Hyper Beam color extraction and runtime palette verification.</summary>
    [VerificationCommand("--samus-hyper-beam-colors")]
    internal static int SamusHyperBeamColorsCommand()
    {
        Suite(nameof(VerifySamusHyperBeamColors), () => VerifySamusHyperBeamColors());
        return 0;
    }
}
