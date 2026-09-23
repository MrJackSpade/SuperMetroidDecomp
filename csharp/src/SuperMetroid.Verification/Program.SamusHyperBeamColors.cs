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

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        SamusHyperBeamColorCatalog catalog = SamusHyperBeamColorCatalog.Load(
            new MemoryStream(SamusHyperBeamColorExtractor.Extract(rom)));
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
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() =>
            SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteSource(10),
            "compiled Hyper Beam pointer table remains bounded");

        var nativeSamus = new SamusState();
        var installedSamus = new SamusState();
        nativeSamus.Drained.EnableRainbow(nativeSamus);
        installedSamus.Drained.EnableRainbow(installedSamus);
        installedSamus.Drained.PresentationColors = catalog;
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        var guarded = new ForbiddenHyperBeamColorBus(rom);
        for (int call = 0; call < 22; call++)
        {
            AssertEqual(nativeSamus.Drained.UpdatePalette(rom, nativeCgram, 0),
                installedSamus.Drained.UpdatePalette(guarded, installedCgram, 0),
                $"Hyper Beam call {call} owns palette");
            AssertEqual(nativeSamus.Drained.ChargePaletteIndex,
                installedSamus.Drained.ChargePaletteIndex,
                $"Hyper Beam call {call} next frame");
            AssertEqual(nativeSamus.Drained.CommonPaletteTimer,
                installedSamus.Drained.CommonPaletteTimer,
                $"Hyper Beam call {call} native timer");
            for (int color = 0; color < SamusHyperBeamColorFormat.ColorsPerFrame; color++)
                AssertEqual(nativeCgram.Colors[192 + color], installedCgram.Colors[192 + color],
                    $"Hyper Beam call {call} CGRAM color {color}");
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
        AreaMapPresentationCatalog baseline, ISnesAddressSpace rom)
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

        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(rom)
        {
            MapPresentation = edited,
        };
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

    private sealed class ForbiddenHyperBeamColorBus(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public int ForbiddenReads { get; private set; }
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
