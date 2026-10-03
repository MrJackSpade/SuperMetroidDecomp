using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyRoomFxPaletteBlends()
    {
        var rom = CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "FX blend oracle revision");
        VerifyFxBlendSelectorIdentities();
        VerifyFxBlendSourceAddresses(rom);
        RoomFxPaletteBlendCatalog catalog = RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(RoomFxPaletteBlendExtractor.Extract(new BlackBlendSourceGuard(new DerivedBlendSourceGuard(rom)))));
        VerifyCeresDefaultRed(rom, catalog);
        VerifyCeresDefaultGreen(rom, catalog);
        VerifyCeresDefaultBlue(rom, catalog);
        VerifyCeresDefaultTintStructure(rom);
        VerifyFxBlendBlackRed(rom, catalog);
        VerifyFxBlendBlackGreen(rom, catalog);
        VerifyFxBlendBlackBlue(rom, catalog);
        VerifyFxBlendBlackStorageAndEdits(catalog);
        VerifyFxPairRed(rom, catalog);
        VerifyFxPairGreen(rom, catalog);
        VerifyFxPairBlue(rom, catalog);
        VerifyFxPairStorageAndEdits(rom);
        VerifyFxWeatherThirdGreen(rom, catalog);
        VerifyFxWeatherThirdBlue(rom, catalog);
        VerifyFxBlendPageDispatch(rom, catalog);
        RoomFxLayer3TilemapCatalog tilemaps = RoomFxLayer3TilemapCatalog.Load(
            new MemoryStream(RoomFxLayer3TilemapExtractor.Extract(rom)));
        foreach (byte id in RoomFxPaletteBlendDefinitions.Ids)
        {
            ReadOnlySpan<ushort> compiled = catalog.Resolve(id);

            (RoomLayer3FxState state, ForbiddenRoomFxPaletteBus bus, SnesCgram cgram) =
                ConstructBlendLoad(catalog, tilemaps, id);
            for (int index = 0; index < compiled.Length; index++)
                AssertEqual(compiled[index], cgram.Colors[RoomFxRomData.Layer3.PaletteBlendDestinationIndex + index],
                    $"room-FX blend {id:X2} installed load color {index}");
            AssertEqual(0, bus.ForbiddenReads, $"room-FX blend {id:X2} load does not read bank-$89");

            ushort reloadRecord = SelectCompiledBlendRecord(id).Pointer;
            _ = state.ApplyEntry(bus, cgram, reloadRecord);
            for (int index = 0; index < compiled.Length; index++)
                AssertEqual(compiled[index], cgram.Colors[RoomFxRomData.Layer3.PaletteBlendDestinationIndex + index],
                    $"room-FX blend {id:X2} installed FX-entry color {index}");
            AssertEqual(0, bus.ForbiddenReads, $"room-FX blend {id:X2} FX entry does not read bank-$89");
        }
        (RoomLayer3FxState emptyState, ForbiddenRoomFxPaletteBus emptyBus, SnesCgram emptyCgram) =
            ConstructBlendLoad(catalog, tilemaps, 0);
        _ = emptyState;
        AssertEqual((ushort)0x1234, emptyCgram.Colors[25], "zero blend preserves color 25");
        AssertEqual((ushort)0x2345, emptyCgram.Colors[26], "zero blend preserves color 26");
        AssertEqual((ushort)0, emptyCgram.Colors[27], "zero blend clears only color 27");
        AssertEqual(0, emptyBus.ForbiddenReads, "zero blend does not read bank-$89");
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendCatalog.Load(
            new MemoryStream([1, 2, 3])), "corrupt room-FX blend resource fails loudly");
        string duplicate = "{\"version\":1,\"version\":1,\"blends\":{}}";
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(duplicate))),
            "duplicate room-FX blend property fails loudly");
        VerifyCeresHazeTintOverride(rom);
        Console.WriteLine("  Room-FX blend palettes: all 24 native words and guarded load/reload paths pass.");
    }

    private static void VerifyCeresHazeTintOverride(ISnesAddressSpace rom)
    {
        RoomFxPaletteBlendDocument stock = JsonSerializer.Deserialize<RoomFxPaletteBlendDocument>(
            RoomFxPaletteBlendExtractor.Extract(rom), MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock room-FX color document is null.");
        AssertEqual(RoomFxPaletteBlendDefinitions.StockCeresHazeBlue, stock.CeresHazeBlue,
            "extractor names the stock Ceres blue tint");
        AssertEqual(RoomFxPaletteBlendDefinitions.StockCeresHazeRed, stock.CeresHazeRed,
            "extractor names the stock Ceres escape tint");
        var editedDocument = stock with
        {
            CeresHazeBlue = new PaletteRgb5 { Red = 0, Green = 15, Blue = 0 },
            CeresHazeRed = new PaletteRgb5 { Red = 0, Green = 0, Blue = 15 },
        };
        RoomFxPaletteBlendCatalog edited = RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(RoomFxPaletteBlendCatalog.Write(editedDocument)));
        ColorAddWindow stockLine = SnesGameplayFrameRenderer.CaptureCeresHaze(false).Windows[120];
        AssertEqual(new ColorAddWindow(0, 255, 0, 0, 66), stockLine,
            "stock Ceres blue haze retains its native scanline-eight fixed color");
        AssertEqual(new ColorAddWindow(0, 255, 66, 0, 0),
            SnesGameplayFrameRenderer.CaptureCeresHaze(true).Windows[120],
            "stock Ceres escape haze retains the matching red fixed color");
        ColorAddWindow editedLine = SnesGameplayFrameRenderer.CaptureCeresHaze(
            false, colors: edited).Windows[120];
        AssertEqual(stockLine.Blue, editedLine.Green,
            "edited Ceres tint replaces blue with equal-amplitude green on the same scanline");
        AssertEqual((byte)0, editedLine.Blue, "edited Ceres tint removes blue addition");
        ColorAddWindow editedEscapeLine = SnesGameplayFrameRenderer.CaptureCeresHaze(
            true, colors: edited).Windows[120];
        AssertEqual(stockLine.Blue, editedEscapeLine.Blue,
            "edited escape tint reaches the post-Ridley channel without changing the gradient");
        AssertEqual((byte)0, editedEscapeLine.Red,
            "edited escape tint removes the stock red channel");
        AssertEqual(stockLine, SnesGameplayFrameRenderer.CaptureCeresHaze(false).Windows[120],
            "cosmetic override does not mutate stock presentation");
        Rgba32[] pixels = Enumerable.Repeat(new Rgba32(0, 0, 0, 255),
            SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height).ToArray();
        SnesGameplayFrameRenderer.ApplyCeresHaze(pixels, false, colors: edited);
        AssertEqual(editedLine.Green, pixels[120 * SnesGameplayFrameRenderer.Width].G,
            "software renderer consumes the same edited Ceres tint as captured PPU rendering");
        AssertEqual((byte)0, pixels[120 * SnesGameplayFrameRenderer.Width].B,
            "software renderer does not retain the stock blue channel");
        AssertEqual((byte)0, SnesGameplayFrameRenderer.CaptureCeresHaze(
            false, intensity: 0, colors: edited).Windows[120].Green,
            "cosmetic color cannot change the native fade-out duration");
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendCatalog.Write(
            editedDocument with
            {
                CeresHazeBlue = editedDocument.CeresHazeBlue with { Green = 32 },
            }), "invalid Ceres tint component fails loudly");
        RoomFxPaletteBlendCatalog legacy = RoomFxPaletteBlendCatalog.Load(new MemoryStream(
            RoomFxPaletteBlendCatalog.Write(stock with
            {
                CeresHazeBlue = null,
                CeresHazeRed = null,
            })));
        AssertEqual(RoomFxPaletteBlendDefinitions.StockCeresHazeBlue, legacy.CeresHazeBlue,
            "older color overrides inherit the stock blue haze tint");
        AssertEqual(RoomFxPaletteBlendDefinitions.StockCeresHazeRed, legacy.CeresHazeRed,
            "older color overrides inherit the stock red haze tint");
    }

    private static void VerifyRoomFxPaletteBlendOverride(string stock, string overrides,
        AreaMapPresentationCatalog baseline)
    {
        string path = Path.Combine(overrides, RoomFxPaletteBlendDefinitions.FileName);
        RoomFxPaletteBlendDocument document = JsonSerializer.Deserialize<RoomFxPaletteBlendDocument>(
            File.ReadAllBytes(Path.Combine(stock, RoomFxPaletteBlendDefinitions.FileName)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock room-FX blend document is null.");
        string key = RoomFxPaletteBlendDefinitions.Key(RoomFxPaletteBlendDefinitions.Lava);
        PaletteRgb5 original = document.Blends[key][0];
        document.Blends[key][0] = original with { Red = (original.Red + 1) % 32 };
        document = document with
        {
            CeresHazeBlue = new PaletteRgb5 { Red = 0, Green = 15, Blue = 0 },
        };
        File.WriteAllBytes(path, RoomFxPaletteBlendCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != baseline.ContentIdentity,
            "room-FX blend edit changes installed content identity");
        ushort expected = edited.RoomFxPaletteBlends.Resolve(RoomFxPaletteBlendDefinitions.Lava)[0];
        AssertTrue(expected != baseline.RoomFxPaletteBlends.Resolve(RoomFxPaletteBlendDefinitions.Lava)[0],
            "room-FX blend edit changes the authored color");
        (_, ForbiddenRoomFxPaletteBus bus, SnesCgram cgram) = ConstructBlendLoad(
            edited.RoomFxPaletteBlends, edited.RoomFxLayer3Tilemaps,
            RoomFxPaletteBlendDefinitions.Lava);
        AssertEqual(expected, cgram.Colors[RoomFxRomData.Layer3.PaletteBlendDestinationIndex],
            "edited room-FX blend color reaches production CGRAM");
        AssertEqual(0, bus.ForbiddenReads, "edited room-FX blend does not read native palette table");
        AssertTrue(SnesGameplayFrameRenderer.CaptureCeresHaze(false,
                colors: edited.RoomFxPaletteBlends).Windows[120].Green > 0,
            "installed room-FX color override reaches Ceres haze rendering");
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendCatalog.Write(document with
        {
            Blends = new Dictionary<string, PaletteRgb5[]>(document.Blends)
            {
                [key] = [original with { Red = 32 }, .. document.Blends[key].Skip(1)],
            },
        }), "invalid room-FX blend component fails loudly");
        File.Delete(path);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing room-FX blend override restores stock identity");
        Console.WriteLine("Room-FX blend override: edited color reaches CGRAM and removal restores stock.");
    }

    private static (RoomLayer3FxState State, ForbiddenRoomFxPaletteBus Bus, SnesCgram Cgram)
        ConstructBlendLoad(RoomFxPaletteBlendCatalog catalog,
            RoomFxLayer3TilemapCatalog tilemaps, byte selection)
    {
        RoomFxRecordDefinition record = SelectCompiledBlendRecord(selection);
        var memory = new TestAddressSpace();
        var bus = new ForbiddenRoomFxPaletteBus(memory);
        var cgram = new SnesCgram();
        cgram.SetColor(25, 0x1234);
        cgram.SetColor(26, 0x2345);
        cgram.SetColor(27, 0x3456);
        var state = new RoomLayer3FxState
        {
            PaletteBlendColors = catalog,
            Layer3Tilemaps = tilemaps,
        };
        state.Load(bus, new SnesVram(), cgram, record.Pointer, doorPointer: 0, randomNumber: 0);
        return (state, bus, cgram);
    }

    private static RoomFxRecordDefinition SelectCompiledBlendRecord(byte selection) =>
        RoomFxRecordDefinitions.All.FirstOrDefault(candidate =>
            candidate.DoorPointer == 0 && candidate.PaletteBlend == selection)
        ?? throw new InvalidDataException($"No compiled default room-FX record selects blend ${selection:X2}.");

    private sealed class ForbiddenRoomFxPaletteBus(TestAddressSpace inner) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public TestAddressSpace Inner { get; } = inner;
        public int ForbiddenReads { get; private set; }

        private void RejectPaletteSource(int address)
        {
            if ((address >> 16) == 0x89)
            {
                ForbiddenReads++;
                throw new InvalidOperationException($"Installed room-FX blend read ROM ${address:X6}.");
            }
        }

        public byte ReadByte(int address)
        {
            RejectPaletteSource(address);
            return Inner.ReadByte(address);
        }

        public byte ReadCartridgeByte(int address)
        {
            RejectPaletteSource(address);
            return Inner.ReadCartridgeByte(address);
        }

        public void WriteByte(int address, byte value) => Inner.WriteByte(address, value);
    }
}
