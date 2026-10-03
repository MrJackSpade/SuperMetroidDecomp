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
        AssertEqual(103, stored.Count, "Hyper Beam aliases and hue interpolation remove57 words");
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
                AssertEqual(source == frame * 16 + index && !(frame == 7 && index != 0) && !midpoint, stored.ContainsKey(frame * 16 + index), "Hyper Beam input ownership");
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
            AssertEqual((ushort)(green + 32 * green + 1024 * blue), SamusHyperBeamColorFormat.YellowFromGreen((ushort)rgb), "Complete RGB5 green-to-yellow domain");
            int midpoint = (int)Math.Ceiling((rgb % 32 + green) / 2.0);
            AssertEqual((ushort)(midpoint + 32 * green + 1024 * blue), SamusHyperBeamColorFormat.GreenYellowMidpoint((ushort)rgb), "Complete RGB5 hue midpoint domain");
        }
        foreach (bool sourcesOnly in new[] { false, true })
        {
            var document = JsonSerializer.Deserialize<SamusHyperBeamColorDocument>(originalJson, MapPresentationFormat.JsonOptions)!;
            for (int frame = 0; frame < 10; frame++)
            for (int color = 0; color < 16; color++)
            {
                if (sourcesOnly && !(color is 2 or 3 or 10 || (frame == 2 && color == 0))) continue;
                int word = 2000 + frame * 16 + color;
                document.Frames[frame][color] = new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
            }
            var edited = SamusHyperBeamColorCatalog.Load(new MemoryStream(SamusHyperBeamColorCatalog.Write(document)));
            for (int frame = 0; frame < 10; frame++)
            for (int color = 0; color < 16; color++)
            {
                ushort pointer = ReadVerificationWord(rom, 0x91d99e + frame * 2);
                ushort expected = !sourcesOnly || color is 2 or 3 or 10 || (frame == 2 && color == 0) ?
                    (ushort)(2000 + frame * 16 + color) : ReadVerificationWord(rom, 0x9b0000 | (pointer + color * 2));
                AssertEqual(expected, edited.Resolve(frame, color), "Every independently supplied Hyper Beam color survives aliases/source edits");
            }
        }

        var greenEdit = JsonSerializer.Deserialize<SamusHyperBeamColorDocument>(originalJson, MapPresentationFormat.JsonOptions)!;
        for (int color = 0; color < 16; color++) greenEdit.Frames[5][color] = new PaletteRgb5 { Red = color, Green = 31 - color, Blue = color + 1 };
        var greenEdited = SamusHyperBeamColorCatalog.Load(new MemoryStream(SamusHyperBeamColorCatalog.Write(greenEdit)));
        for (int frame = 0; frame < 10; frame++)
        for (int color = 0; color < 16; color++)
        {
            ushort pointer = ReadVerificationWord(rom, 0x91d99e + frame * 2);
            ushort expected = frame == 5 ? (ushort)(color | (31 - color) << 5 | (color + 1) << 10) :
                ReadVerificationWord(rom, 0x9b0000 | (pointer + color * 2));
            AssertEqual(expected, greenEdited.Resolve(frame, color), "Green-only edits preserve supplied yellow and all other frames");
        }

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
        Console.WriteLine("  Hyper Beam: ten native pointer/palette frames and guarded runtime cycle pass.");
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
            if ((address >= pointerStart && address < pointerStart +
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
