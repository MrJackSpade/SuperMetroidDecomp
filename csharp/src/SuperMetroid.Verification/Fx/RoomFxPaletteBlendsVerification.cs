using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Areas.Ceres.LookupCeresDefaultTintsVerification;
using static SuperMetroid.Verification.Fx.LookupFxBlendSelectorsVerification;
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
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Checks extracted room-FX blend colors, installed CGRAM writes, and the installed Ceres haze tints.</summary>
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
        RoomFxBlendInks catalog = RoomFxPaletteBlendFile.Read(new MemoryStream(RoomFxPaletteBlendExtractor.Extract(rom)));
        // The Ceres haze's tints are scene colours the import installs (#1544).
        CeresHazeTints haze = RetailPresentationFixtureVerification.RetailPresentationFixture().CeresHaze;
        Suite(nameof(VerifyCeresDefaultRed), () => VerifyCeresDefaultRed(rom, haze));
        Suite(nameof(VerifyCeresDefaultGreen), () => VerifyCeresDefaultGreen(rom, haze));
        Suite(nameof(VerifyCeresDefaultBlue), () => VerifyCeresDefaultBlue(rom, haze));
        Suite(nameof(VerifyCeresDefaultTintStructure), () => VerifyCeresDefaultTintStructure(rom));
        Suite(nameof(VerifyFxBlendPageDispatch), () => VerifyFxBlendPageDispatch(rom, catalog));
        foreach (RoomFxPaletteBlend id in RoomFxPaletteBlendDocumentRules.Ids)
        {
            IReadOnlyList<RgbColor> compiled = catalog.Inks(id);
            // The layer-3 pictures' shade of the blend holds its three colours over the blend inks (#1512).
            SheetShade shade = BlendShade(catalog, id);
            AssertEqual(RoomFxNativeSources.PaletteBlendDestinationIndex, shade.Inks[0], $"room-FX blend {id} shades the blend inks");
            for (int index = 0; index < compiled.Count; index++)
                AssertEqual(compiled[index], shade.Colours[index], $"room-FX blend {id} shade colour {index}");

            (RoomFxOverlayState state, CartridgeReadGuard bus) = ConstructBlendLoad(id);
            AssertEqual<RoomFxPaletteBlend?>(id, state.PaletteBlend, $"room-FX blend {id} load names its blend");
            AssertEqual(0, bus.ForbiddenReadAttempts, $"room-FX blend {id} load does not read bank-$89");

            state.ApplyEntry(SelectCompiledBlendRecord(id));
            AssertEqual<RoomFxPaletteBlend?>(id, state.PaletteBlend, $"room-FX blend {id} FX entry names its blend");
            AssertEqual(0, bus.ForbiddenReadAttempts, $"room-FX blend {id} FX entry does not read bank-$89");
        }
        (RoomFxOverlayState emptyState, CartridgeReadGuard emptyBus) = ConstructBlendLoad(RoomFxPaletteBlend.None);
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
        Suite(nameof(VerifyCeresHazeTint), VerifyCeresHazeTint);
        Console.WriteLine("  Room-FX blend palettes: all 24 native words and guarded load/reload paths pass.");
    }

    /// <summary>
    /// Checks the installed Ceres haze tints, which the import records in the scene colours (#1544), their native scanline
    /// colours, and the scene colour reader's channel bound.
    /// </summary>
    private static void VerifyCeresHazeTint()
    {
        SceneColourDocument stock = JsonSerializer.Deserialize<SceneColourDocument>(
            File.ReadAllBytes(Path.Combine(SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation.MapDirectory, SceneColourFiles.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Installed scene colour document is null.");
        AssertEqual(PaletteRgb5.From(SceneColourFiles.StockCeresHazeBlue), stock.CeresHazeWhileRidleyLives,
            "the import records the stock Ceres blue tint");
        AssertEqual(PaletteRgb5.From(SceneColourFiles.StockCeresHazeRed), stock.CeresHazeAfterRidleyDies,
            "the import records the stock Ceres escape tint");
        // Each haze over a black screen in the installed tints, as the display renderer draws its line 120 (#1468, #1596).
        var installedTints = new CeresHazeTints(stock.CeresHazeWhileRidleyLives.ToRgbColor(), stock.CeresHazeAfterRidleyDies.ToRgbColor());
        RgbaPixel stockLine = HazeLine(GameplayEffectCapture.CaptureCeresHaze(installedTints, false));
        AssertEqual(new RgbaPixel(0, 0, 66, 255), stockLine,
            "stock Ceres blue haze retains its native scanline-eight fixed color");
        AssertEqual(new RgbaPixel(66, 0, 0, 255), HazeLine(GameplayEffectCapture.CaptureCeresHaze(installedTints, true)),
            "stock Ceres escape haze retains the matching red fixed color");
        AssertThrows<InvalidDataException>(() => SceneColourFiles.Read(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
            stock with { CeresHazeWhileRidleyLives = stock.CeresHazeWhileRidleyLives with { Green = 32 } }, ArtworkJson.Options),
            writable: false)), "reading an invalid Ceres tint component fails loudly");
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
        RoomEffectDefinition record = SelectCompiledBlendRecord(selection);
        var memory = new TestAddressSpace();
        var bus = NewForbiddenRoomFxPaletteBus(memory);
        var state = RoomFxOverlayState.ForDefinition(new RoomFxSession(), record, randomNumber: 0);
        return new ConstructBlendLoadResult(state, bus);
    }

    /// <summary>The shade the import pre-colours the layer-3 pictures in for <paramref name="selection"/> from <paramref name="catalog"/>.</summary>
    private static SheetShade BlendShade(RoomFxBlendInks catalog, RoomFxPaletteBlend selection) =>
        RoomFxBlendSheetShades.Of(catalog).Single(shade => shade.Shade == RoomFxBlendShading.Of(selection));

    /// <summary>Finds the compiled default FX record whose palette selector matches the requested blend.</summary>
    private static RoomEffectDefinition SelectCompiledBlendRecord(RoomFxPaletteBlend selection) =>
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
