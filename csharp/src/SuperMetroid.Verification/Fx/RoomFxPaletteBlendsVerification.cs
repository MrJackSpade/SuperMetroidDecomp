using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Areas.Ceres.LookupCeresDefaultTintsVerification;
using static SuperMetroid.Verification.Fx.LookupFxBlendBlackColorsVerification;
using static SuperMetroid.Verification.Fx.LookupFxBlendSelectorsVerification;
using static SuperMetroid.Verification.Fx.LookupFxPairColorsVerification;
using static SuperMetroid.Verification.Fx.LookupFxWeatherBlueVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Checks extracted room-FX blend colors, installed CGRAM writes, and the Ceres haze tints.</summary>
internal static class RoomFxPaletteBlendsVerification
{
    /// <summary>The colour a Ceres haze layer adds at the start of screen line 120, drawn over black by the display renderer.</summary>
    private static RgbaPixel HazeLine(ColourAddWindowLayer haze) =>
        DisplayRendering.Render(new LayeredScene(FramePaint.FromNative(new RgbColor[SnesCgram.ColorCount], []),
            new DisplayLayer[] { haze }, DisplayBrightness.Full))[120 * DisplayScreen.Width];

    /// <summary>Compares compiled FX palettes with the retail source and checks guarded load and reload paths.</summary>
    internal static void VerifyRoomFxPaletteBlends()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "FX blend oracle revision");
        Suite(nameof(VerifyFxBlendSelectorIdentities), () => VerifyFxBlendSelectorIdentities());
        Suite(nameof(VerifyFxBlendSourceAddresses), () => VerifyFxBlendSourceAddresses(rom));
        RoomFxBlendInks catalog = RoomFxPaletteBlendFile.Read(
            new MemoryStream(RoomFxPaletteBlendExtractor.Extract(NewBlackBlendSourceGuard(new DerivedBlendSourceGuard(rom)))));
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
        foreach (RoomFxPaletteBlend id in RoomFxPaletteBlendDocumentRules.Ids)
        {
            ReadOnlySpan<RgbColor> compiled = catalog.Resolve(id);
            // The layer-3 pictures' shade of the blend holds its three colours over the blend inks (#1512).
            SheetShade shade = BlendShade(catalog, id);
            AssertEqual(RoomFxNativeSources.PaletteBlendDestinationIndex, shade.Inks[0], $"room-FX blend {id} shades the blend inks");
            for (int index = 0; index < compiled.Length; index++)
                AssertEqual(compiled[index], shade.Colours[index], $"room-FX blend {id} shade colour {index}");

            (RoomLayer3FxState state, CartridgeReadGuard bus) = ConstructBlendLoad(id);
            AssertEqual<RoomFxPaletteBlend?>(id, state.PaletteBlend, $"room-FX blend {id} load names its blend");
            AssertEqual(0, bus.ForbiddenReadAttempts, $"room-FX blend {id} load does not read bank-$89");

            _ = state.ApplyEntry(SelectCompiledBlendRecord(id));
            AssertEqual<RoomFxPaletteBlend?>(id, state.PaletteBlend, $"room-FX blend {id} FX entry names its blend");
            AssertEqual(0, bus.ForbiddenReadAttempts, $"room-FX blend {id} FX entry does not read bank-$89");
        }
        (RoomLayer3FxState emptyState, CartridgeReadGuard emptyBus) = ConstructBlendLoad(RoomFxPaletteBlend.None);
        AssertEqual<RoomFxPaletteBlend?>(RoomFxPaletteBlend.None, emptyState.PaletteBlend, "zero blend names the empty blend");
        SheetShade empty = BlendShade(catalog, RoomFxPaletteBlend.None);
        AssertTrue(empty.Inks.SequenceEqual([RoomFxNativeSources.PaletteBlendDestinationIndex + 2]) && empty.Colours.SequenceEqual([RgbColor.Black]),
            "zero blend's shade clears only the third blend ink");
        AssertEqual(0, emptyBus.ForbiddenReadAttempts, "zero blend does not read bank-$89");
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendFile.Read(
            new MemoryStream([1, 2, 3])), "corrupt room-FX blend resource fails loudly");
        string duplicate = "{\"version\":1,\"version\":1,\"blends\":{}}";
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendFile.Read(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(duplicate))),
            "duplicate room-FX blend property fails loudly");
        Suite(nameof(VerifyCeresHazeTint), () => VerifyCeresHazeTint(rom));
        Console.WriteLine("  Room-FX blend palettes: all 24 native words and guarded load/reload paths pass.");
    }

    /// <summary>Checks the extracted Ceres haze tints, their native scanline colours, and the tint codec's channel bound.</summary>
    private static void VerifyCeresHazeTint(ISnesAddressSpace rom)
    {
        RoomFxPaletteBlendDocument stock = JsonSerializer.Deserialize<RoomFxPaletteBlendDocument>(
            RoomFxPaletteBlendExtractor.Extract(rom), ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock room-FX color document is null.");
        AssertEqual(PaletteRgb5.From(RoomFxPaletteBlendDocumentRules.StockCeresHazeBlue), stock.CeresHazeBlue,
            "extractor names the stock Ceres blue tint");
        AssertEqual(PaletteRgb5.From(RoomFxPaletteBlendDocumentRules.StockCeresHazeRed), stock.CeresHazeRed,
            "extractor names the stock Ceres escape tint");
        // Each haze over a black screen, as the display renderer draws its line 120 (#1468).
        RgbaPixel stockLine = HazeLine(GameplayEffectCapture.CaptureCeresHaze(false));
        AssertEqual(new RgbaPixel(0, 0, 66, 255), stockLine,
            "stock Ceres blue haze retains its native scanline-eight fixed color");
        AssertEqual(new RgbaPixel(66, 0, 0, 255), HazeLine(GameplayEffectCapture.CaptureCeresHaze(true)),
            "stock Ceres escape haze retains the matching red fixed color");
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendFile.Read(new MemoryStream(RoomFxPaletteBlendFile.Write(
            stock with
            {
                CeresHazeBlue = (stock.CeresHazeBlue ?? throw new InvalidOperationException("The extractor writes the Ceres blue tint.")) with { Green = 32 },
            }), writable: false)), "reading an invalid Ceres tint component fails loudly");
    }

    /// <summary>Checks that the room-FX blend codec rejects out-of-range colours.</summary>
    internal static void VerifyRoomFxPaletteBlendsRejectMalformedDocuments(string stock)
    {
        RoomFxPaletteBlendDocument document = JsonSerializer.Deserialize<RoomFxPaletteBlendDocument>(
            File.ReadAllBytes(Path.Combine(stock, RoomFxPaletteBlendDefinitions.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock room-FX blend document is null.");
        string key = RoomFxPaletteBlendDocumentRules.Key(RoomFxPaletteBlend.Lava);
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendFile.Read(new MemoryStream(RoomFxPaletteBlendFile.Write(document with
        {
            Blends = new Dictionary<string, PaletteRgb5[]>(document.Blends)
            {
                [key] = [document.Blends[key][0] with { Red = 32 }, .. document.Blends[key].Skip(1)],
            },
        }), writable: false)), "reading an invalid room-FX blend component fails loudly");
    }

    /// <summary>Loads a selected compiled FX record through a bus that forbids bank-$89 reads.</summary>
    private static ConstructBlendLoadResult ConstructBlendLoad(RoomFxPaletteBlend selection)
    {
        RoomFxRecordDefinition record = SelectCompiledBlendRecord(selection);
        var memory = new TestAddressSpace();
        var bus = NewForbiddenRoomFxPaletteBus(memory);
        var state = RoomLayer3FxState.ForDefinition(new RoomFxSession(), record, randomNumber: 0);
        return new ConstructBlendLoadResult(state, bus);
    }

    /// <summary>The shade the import pre-colours the layer-3 pictures in for <paramref name="selection"/> from <paramref name="catalog"/>.</summary>
    private static SheetShade BlendShade(RoomFxBlendInks catalog, RoomFxPaletteBlend selection) =>
        RoomFxBlendSheetShades.Of(catalog).Single(shade => shade.Shade == RoomFxBlendShading.Of(selection));

    /// <summary>Finds the compiled default FX record whose palette selector matches the requested blend.</summary>
    private static RoomFxRecordDefinition SelectCompiledBlendRecord(RoomFxPaletteBlend selection) =>
        RoomFxDefinitions.AllRecords.FirstOrDefault(candidate =>
            candidate.DoorPointer == 0 && candidate.PaletteBlend == selection)
        ?? throw new InvalidDataException($"No compiled default room-FX record selects blend {selection}.");

    /// <summary>Wraps fixture memory with a guard against installed palette-table reads from bank $89.</summary>
    private static CartridgeReadGuard NewForbiddenRoomFxPaletteBus(TestAddressSpace inner) =>
        new CartridgeReadGuard(inner).Forbid(address => (SnesAddressMath.BankNumber(address)) == 0x89,
            address => $"Installed room-FX blend read ROM ${address:X6}.");

    /// <summary>Runs the room-FX blend checks from the lookup command.</summary>
    [VerificationCommand("--lookup-fx-blends")]
    internal static int LookupFxBlendsCommand()
    {
        Suite(nameof(VerifyRoomFxPaletteBlends), () => VerifyRoomFxPaletteBlends());
        return 0;
    }

    /// <summary>Runs the standalone room-FX palette blend verification command.</summary>
    [VerificationCommand("--room-fx-palette-blends")]
    internal static int RoomFxPaletteBlendsCommand()
    {
        Suite(nameof(VerifyRoomFxPaletteBlends), () => VerifyRoomFxPaletteBlends());
        return 0;
    }
}
