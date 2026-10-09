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
    /// <summary>Verifies native room-FX blend data, guarded load paths, and cosmetic color overrides against the retail ROM.</summary>
    private static void VerifyRoomFxPaletteBlends()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "FX blend oracle revision");
        Suite(nameof(VerifyFxBlendSelectorIdentities), () => VerifyFxBlendSelectorIdentities());
        Suite(nameof(VerifyFxBlendSourceAddresses), () => VerifyFxBlendSourceAddresses(rom));
        RoomFxPaletteBlendCatalog catalog = RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(RoomFxPaletteBlendExtractor.Extract(new BlackBlendSourceGuard(new DerivedBlendSourceGuard(rom)))));
        Suite(nameof(VerifyCeresDefaultRed), () => VerifyCeresDefaultRed(rom, catalog));
        Suite(nameof(VerifyCeresDefaultGreen), () => VerifyCeresDefaultGreen(rom, catalog));
        Suite(nameof(VerifyCeresDefaultBlue), () => VerifyCeresDefaultBlue(rom, catalog));
        Suite(nameof(VerifyCeresDefaultTintStructure), () => VerifyCeresDefaultTintStructure(rom));
        Suite(nameof(VerifyFxBlendBlackRed), () => VerifyFxBlendBlackRed(rom, catalog));
        Suite(nameof(VerifyFxBlendBlackGreen), () => VerifyFxBlendBlackGreen(rom, catalog));
        Suite(nameof(VerifyFxBlendBlackBlue), () => VerifyFxBlendBlackBlue(rom, catalog));
        Suite(nameof(VerifyFxBlendBlackStorageAndEdits), () => VerifyFxBlendBlackStorageAndEdits(catalog));
        Suite(nameof(VerifyFxPairRed), () => VerifyFxPairRed(rom, catalog));
        Suite(nameof(VerifyFxPairGreen), () => VerifyFxPairGreen(rom, catalog));
        Suite(nameof(VerifyFxPairBlue), () => VerifyFxPairBlue(rom, catalog));
        Suite(nameof(VerifyFxPairStorageAndEdits), () => VerifyFxPairStorageAndEdits(rom));
        Suite(nameof(VerifyFxWeatherThirdGreen), () => VerifyFxWeatherThirdGreen(rom, catalog));
        Suite(nameof(VerifyFxWeatherThirdBlue), () => VerifyFxWeatherThirdBlue(rom, catalog));
        Suite(nameof(VerifyFxBlendPageDispatch), () => VerifyFxBlendPageDispatch(rom, catalog));
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
        Suite(nameof(VerifyCeresHazeTintOverride), () => VerifyCeresHazeTintOverride(rom));
        Console.WriteLine("  Room-FX blend palettes: all 24 native words and guarded load/reload paths pass.");
    }

    /// <summary>Checks that Ceres haze tint overrides affect rendered channels without changing native timing or fade behavior.</summary>
    /// <param name="rom">Cartridge address space used to extract the stock tint values.</param>
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

    /// <summary>Checks installation, application, validation, and removal of a room-FX palette blend override.</summary>
    /// <param name="stock">Directory containing the stock map-presentation and room-FX resources.</param>
    /// <param name="overrides">Directory where the edited blend document is written and later removed.</param>
    /// <param name="baseline">Unedited presentation catalog used to compare content identity and restored stock colors.</param>
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

    /// <summary>Loads a selected compiled room-FX blend into a fresh state while guarding against bank-$89 palette reads.</summary>
    /// <param name="catalog">Palette blend catalog supplying the selected authored colors.</param>
    /// <param name="tilemaps">Layer-3 tilemap catalog required by the room-FX state.</param>
    /// <param name="selection">Blend selection byte placed in the default room-FX record.</param>
    /// <returns>The initialized effect state, guarded bus, and CGRAM receiving the blend.</returns>
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

    /// <summary>Finds the default-door compiled room-FX record that selects the requested palette blend.</summary>
    /// <param name="selection">Blend identifier stored in the compiled room-FX record.</param>
    /// <returns>The matching record, or throws when no compiled default selects that blend.</returns>
    private static RoomFxRecordDefinition SelectCompiledBlendRecord(byte selection) =>
        RoomFxRecordDefinitions.All.FirstOrDefault(candidate =>
            candidate.DoorPointer == 0 && candidate.PaletteBlend == selection)
        ?? throw new InvalidDataException($"No compiled default room-FX record selects blend ${selection:X2}.");

    /// <summary>Address-space wrapper that counts and rejects reads from the native bank-$89 palette tables.</summary>
    /// <param name="inner">Test memory used for all addresses outside the forbidden palette bank.</param>
    private sealed class ForbiddenRoomFxPaletteBus(TestAddressSpace inner) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Underlying fixture memory used after the bank-$89 read check succeeds.</summary>
        public TestAddressSpace Inner { get; } = inner;
        /// <summary>Number of attempted reads from bank $89.</summary>
        public int ForbiddenReads { get; private set; }

        /// <summary>Records and rejects an address in the cartridge palette bank used by native room-FX blends.</summary>
        /// <param name="address">Address being checked before the wrapped bus is read.</param>
        private void RejectPaletteSource(int address)
        {
            if ((address >> 16) == 0x89)
            {
                ForbiddenReads++;
                throw new InvalidOperationException($"Installed room-FX blend read ROM ${address:X6}.");
            }
        }

        /// <summary>Checks for a forbidden bank-$89 access before forwarding an address-space read.</summary>
        /// <param name="address">Byte address requested from the test bus.</param>
        /// <returns>The inner memory byte when the address is permitted.</returns>
        public byte ReadByte(int address)
        {
            RejectPaletteSource(address);
            return Inner.ReadByte(address);
        }

        /// <summary>Checks for a forbidden bank-$89 access before forwarding a cartridge-source read.</summary>
        /// <param name="address">Cartridge byte address requested by the importer.</param>
        /// <returns>The inner memory byte when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address)
        {
            RejectPaletteSource(address);
            return Inner.ReadCartridgeByte(address);
        }

        /// <summary>Forwards writes unchanged; this wrapper rejects reads from the native palette bank.</summary>
        /// <param name="address">Destination byte address.</param>
        /// <param name="value">Byte written to the inner test memory.</param>
        public void WriteByte(int address, byte value) => Inner.WriteByte(address, value);
    }
}
