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
using SuperMetroid.Core.Frontend.Map.Artwork;
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
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Checks extracted room-FX blend colors, installed CGRAM writes, and Ceres haze color overrides.</summary>
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
        Suite(nameof(VerifyCeresHazeTintOverride), () => VerifyCeresHazeTintOverride(rom));
        Console.WriteLine("  Room-FX blend palettes: all 24 native words and guarded load/reload paths pass.");
    }

    /// <summary>Checks that edited haze channels reach both renderers without changing the stock fade timing.</summary>
    private static void VerifyCeresHazeTintOverride(ISnesAddressSpace rom)
    {
        RoomFxPaletteBlendDocument stock = JsonSerializer.Deserialize<RoomFxPaletteBlendDocument>(
            RoomFxPaletteBlendExtractor.Extract(rom), ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock room-FX color document is null.");
        AssertEqual(PaletteRgb5.From(RoomFxPaletteBlendDocumentRules.StockCeresHazeBlue), stock.CeresHazeBlue,
            "extractor names the stock Ceres blue tint");
        AssertEqual(PaletteRgb5.From(RoomFxPaletteBlendDocumentRules.StockCeresHazeRed), stock.CeresHazeRed,
            "extractor names the stock Ceres escape tint");
        var editedDocument = stock with
        {
            CeresHazeBlue = new PaletteRgb5 { Red = 0, Green = 15, Blue = 0 },
            CeresHazeRed = new PaletteRgb5 { Red = 0, Green = 0, Blue = 15 },
        };
        RoomFxBlendInks edited = RoomFxPaletteBlendFile.Read(
            new MemoryStream(RoomFxPaletteBlendFile.Write(editedDocument)));
        // Each haze over a black screen, as the display renderer draws its line 120 (#1468).
        RgbaPixel stockLine = HazeLine(GameplayEffectCapture.CaptureCeresHaze(false));
        AssertEqual(new RgbaPixel(0, 0, 66, 255), stockLine,
            "stock Ceres blue haze retains its native scanline-eight fixed color");
        AssertEqual(new RgbaPixel(66, 0, 0, 255), HazeLine(GameplayEffectCapture.CaptureCeresHaze(true)),
            "stock Ceres escape haze retains the matching red fixed color");
        RgbaPixel editedLine = HazeLine(GameplayEffectCapture.CaptureCeresHaze(false, colors: edited));
        AssertEqual(stockLine.B, editedLine.G,
            "edited Ceres tint replaces blue with equal-amplitude green on the same scanline");
        AssertEqual((byte)0, editedLine.B, "edited Ceres tint removes blue addition");
        RgbaPixel editedEscapeLine = HazeLine(GameplayEffectCapture.CaptureCeresHaze(true, colors: edited));
        AssertEqual(stockLine.B, editedEscapeLine.B,
            "edited escape tint reaches the post-Ridley channel without changing the gradient");
        AssertEqual((byte)0, editedEscapeLine.R,
            "edited escape tint removes the stock red channel");
        AssertEqual(stockLine, HazeLine(GameplayEffectCapture.CaptureCeresHaze(false)),
            "cosmetic override does not mutate stock presentation");
        AssertEqual((byte)0, HazeLine(GameplayEffectCapture.CaptureCeresHaze(false, intensity: 0, colors: edited)).G,
            "cosmetic color cannot change the native fade-out duration");
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendFile.Read(new MemoryStream(RoomFxPaletteBlendFile.Write(
            editedDocument with
            {
                CeresHazeBlue = editedDocument.CeresHazeBlue with { Green = 32 },
            }), writable: false)), "reading an invalid Ceres tint component fails loudly");
        RoomFxBlendInks legacy = RoomFxPaletteBlendFile.Read(new MemoryStream(
            RoomFxPaletteBlendFile.Write(stock with
            {
                CeresHazeBlue = null,
                CeresHazeRed = null,
            })));
        AssertEqual(RoomFxPaletteBlendDocumentRules.StockCeresHazeBlue, legacy.CeresHazeBlue,
            "older color overrides inherit the stock blue haze tint");
        AssertEqual(RoomFxPaletteBlendDocumentRules.StockCeresHazeRed, legacy.CeresHazeRed,
            "older color overrides inherit the stock red haze tint");
    }

    /// <summary>Installs a color override, verifies its CGRAM and haze effects, then checks identity restoration after removal.</summary>
    internal static void VerifyRoomFxPaletteBlendOverride(string stock, string overrides,
        AreaMapPresentation baseline)
    {
        string path = Path.Combine(overrides, RoomFxPaletteBlendDefinitions.FileName);
        RoomFxPaletteBlendDocument document = JsonSerializer.Deserialize<RoomFxPaletteBlendDocument>(
            File.ReadAllBytes(Path.Combine(stock, RoomFxPaletteBlendDefinitions.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock room-FX blend document is null.");
        string key = RoomFxPaletteBlendDocumentRules.Key(RoomFxPaletteBlend.Lava);
        PaletteRgb5 original = document.Blends[key][0];
        document.Blends[key][0] = original with { Red = (original.Red + 1) % 32 };
        document = document with
        {
            CeresHazeBlue = new PaletteRgb5 { Red = 0, Green = 15, Blue = 0 },
        };
        File.WriteAllBytes(path, RoomFxPaletteBlendFile.Write(document));
        AreaMapPresentation edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != baseline.ContentIdentity,
            "room-FX blend edit changes installed content identity");
        RgbColor expected = edited.RoomFxPaletteBlends.Resolve(RoomFxPaletteBlend.Lava)[0];
        AssertTrue(expected != baseline.RoomFxPaletteBlends.Resolve(RoomFxPaletteBlend.Lava)[0],
            "room-FX blend edit changes the authored color");
        AssertEqual(expected, BlendShade(edited.RoomFxPaletteBlends, RoomFxPaletteBlend.Lava).Colours[0],
            "edited room-FX blend color reaches the layer-3 pictures' shade");
        AssertTrue(HazeLine(GameplayEffectCapture.CaptureCeresHaze(false,
                colors: edited.RoomFxPaletteBlends)).G > 0,
            "installed room-FX color override reaches Ceres haze rendering");
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendFile.Read(new MemoryStream(RoomFxPaletteBlendFile.Write(document with
        {
            Blends = new Dictionary<string, PaletteRgb5[]>(document.Blends)
            {
                [key] = [original with { Red = 32 }, .. document.Blends[key].Skip(1)],
            },
        }), writable: false)), "reading an invalid room-FX blend component fails loudly");
        File.Delete(path);
        AreaMapPresentation restored = MapPresentationFiles.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing room-FX blend override restores stock identity");
        Console.WriteLine("Room-FX blend override: edited color reaches the blend's shade and removal restores stock.");
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
