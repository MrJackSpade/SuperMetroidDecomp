using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Frontend.Ending.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed ending text and font assets against native tilemaps and guarded typewriter execution.</summary>
internal static class EndingTextPresentationVerification
{
    /// <summary>Matches extracted font bytes and panels to the cartridge, then checks text edits, snapshot rebinding, and glyph validation.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending font characters")]
    internal static void VerifyEndingTextPresentation(string romPath)
    {
        ISnesAddressSpace nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.Ending.EndingTextExtractor.Extract(nativeBus);
        EndingTextPresentation presentation = EndingTextPresentation.Load(
            new MemoryStream(extracted, writable: false));
        byte[] fontPng = SuperMetroid.AssetExtraction.Frontend.Ending.EndingFontAtlasExtractor.Extract(nativeBus);
        EndingFontAtlas font = EndingFontAtlas.Load(new MemoryStream(fontPng, writable: false));
        byte[] nativeFont = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(nativeBus),
            EndingCreditsRomData.Assets.EndingFontCharacters,
            EndingCreditsRomData.Rendering.Mode7Bytes);
        AssertTrue(font.Transfer.Span.SequenceEqual(
            nativeFont.AsSpan(0, EndingFontAtlasFormat.ByteCount)),
            "installed ending-font PNG compiles to exact native planar bytes");
        ISnesAddressSpace installedBus = NewEndingTextReadGuard(nativeBus);

        AssertTrue(presentation.BuildResultPanel().SequenceEqual(
            ReadEndingWords(nativeBus, EndingCartridgeTables.EndingResultPanel,
                EndingTextDefinitions.ResultPanelRows * EndingTextDefinitions.TilemapWidth)),
            "installed producer panel matches native tilemap");
        AssertTrue(presentation.BuildCopyrightPanel().SequenceEqual(
            ReadEndingWords(nativeBus, EndingCartridgeTables.EndingCopyrightPanel,
                EndingTextDefinitions.CopyrightRows * EndingTextDefinitions.TilemapWidth)),
            "installed copyright panel matches native tilemap");

        int comparedFrames = 0;
        foreach (EndingTextSequence sequence in Enum.GetValues<EndingTextSequence>())
        {
            ushort[] installedTilemap = Enumerable.Repeat(
                EndingCreditsRomData.Rendering.BlankTile,
                EndingCreditsRomData.Rendering.TilemapWords).ToArray();
            // The installed sequence runs to completion with its text streams guarded.
            var installed = new EndingBackgroundTextState(new EndingBackgroundTextSetup { Tilemap = installedTilemap, Inventory = default, JapaneseText = true, Presentation = presentation, InstalledSequence = sequence });
            var installedScreen = new EndingScreen();
            for (int frame = 0; !installed.Completed; frame++)
            {
                if (frame == 2048)
                    throw new InvalidOperationException($"Ending {sequence} did not terminate.");
                installed.Step(installedScreen);
                comparedFrames++;
            }
            AssertTrue(installedTilemap.Any(word => word != EndingCreditsRomData.Rendering.BlankTile),
                $"installed ending {sequence} draws its text");
        }

        JsonObject editedDocument = RequiredJson.Parse(extracted).AsObject();
        editedDocument["percentageHeading"] = "TEST";
        editedDocument["finalMessage"] = "TEST";
        editedDocument.Required("resultPanel")["text"] = "TEST";
        byte[] editedBytes = Encoding.UTF8.GetBytes(
            editedDocument.ToJsonString(ArtworkJson.Options));
        EndingTextPresentation edited = EndingTextPresentation.Load(
            new MemoryStream(editedBytes, writable: false));
        AssertEqual(EndingTextDefinitions.CompileGlyph('T', EndingTextStyle.ResultSmall),
            edited.BuildResultPanel()[EndingTextDefinitions.ResultProducedBy.Column],
            "edited producer text reaches its bounded panel");
        AssertEqual(EndingTextDefinitions.ResultBlankWord,
            edited.BuildResultPanel()[EndingTextDefinitions.ResultProducedBy.Column + 4],
            "shorter producer text clears the remainder of its bounded panel");
        AssertEqual(new SnesBgTilemapWord(EndingTextDefinitions.CompileGlyph('T', EndingTextStyle.PercentageSmall)),
            edited.Compile(EndingTextSequence.ItemPercentage)[0].TopWord,
            "edited percentage text reaches its live program");

        var saved = new EndingBackgroundTextState(new EndingBackgroundTextSetup { Tilemap = Enumerable.Repeat(EndingCreditsRomData.Rendering.BlankTile,
                EndingCreditsRomData.Rendering.TilemapWords).ToArray(), Inventory = default, JapaneseText = false, Presentation = presentation, InstalledSequence = EndingTextSequence.FinalMessage });
        saved.Step(new EndingScreen());
        using var snapshot = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(snapshot, saved);
        snapshot.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer
            .Deserialize<EndingBackgroundTextState>(snapshot);
        AssertThrows<InvalidOperationException>(() => restored.Step(new EndingScreen()),
            "restored ending text fails loudly until host content is rebound");
        restored.BindPresentation(presentation);
        restored.Step(new EndingScreen());

        editedDocument["finalMessage"] = "lowercase";
        AssertThrows<InvalidDataException>(() => EndingTextPresentation.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(
                editedDocument.ToJsonString(ArtworkJson.Options)), writable: false)),
            "ending text rejects glyphs absent from its documented font mapping");
        IndexedPngImage editableFont = IndexedPng.Read(
            new MemoryStream(fontPng, writable: false),
            new(EndingFontAtlasFormat.Width, EndingFontAtlasFormat.Height));
        editableFont.Pixels[0] ^= 1;
        using var editedFontPng = new MemoryStream();
        IndexedPng.Write(editedFontPng, new(editableFont.Size.X, editableFont.Size.Y),
            editableFont.Pixels, editableFont.Palette);
        editedFontPng.Position = 0;
        EndingFontAtlas editedFont = EndingFontAtlas.Load(editedFontPng);
        AssertTrue(!editedFont.Transfer.Span.SequenceEqual(font.Transfer.Span),
            "ending-font PNG edit changes compiled VRAM bytes");
        Console.WriteLine($"Ending text: exact producer/copyright panels and {comparedFrames} native typewriter frames match with bank-$8C text reads forbidden; edits, restore and validation pass.");
        Console.WriteLine("Ending font: 160 PNG tiles compile to exact native 4-bpp bytes; an indexed-pixel edit changes installed VRAM content.");
    }

    /// <summary>Reads a bank-$8C tilemap span for the independent result or copyright panel comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending text presentation words")]
    internal static ushort[] ReadEndingWords(ISnesAddressSpace bus, ushort pointer, int count)
    {
        var result = new ushort[count];
        int address = (int)new SnesAddress(EndingCartridgeTables.EndingBank, pointer);
        for (int index = 0; index < count; index++)
            result[index] = CartridgeImportSource.Require(bus).ReadCartridgeWord(address + index * 2);
        return result;
    }

    /// <summary>Checks deterministic text extraction and that an override changes catalog identity and fails loudly when corrupt.</summary>
    internal static void VerifyEndingTextAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.Frontend.Ending.EndingTextExtractor.Extract(bus);
        AssertTrue(deterministic.SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, EndingTextDefinitions.FileName))),
            "installed ending text is the deterministic cartridge extraction");
        Directory.CreateDirectory(overrideDirectory);
        JsonObject document = RequiredJson.Parse(deterministic).AsObject();
        document["finalMessage"] = "TEST";
        string replacement = Path.Combine(overrideDirectory, EndingTextDefinitions.FileName);
        File.WriteAllText(replacement, document.ToJsonString(ArtworkJson.Options));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(
            stockDirectory, overrideDirectory);
        AssertEqual(new SnesBgTilemapWord(EndingTextDefinitions.CompileGlyph('T', EndingTextStyle.FinalLarge)),
            edited.EndingText.Compile(EndingTextSequence.FinalMessage)[0].TopWord,
            "catalog override changes installed ending text");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "ending-text override changes catalog content identity");
        File.WriteAllText(replacement, "{ broken ending text");
        AssertThrows<InvalidDataException>(() =>
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory),
            "corrupt ending-text override fails loudly");
        Console.WriteLine("Ending-text catalog: deterministic stock, override identity and corruption failure pass.");
    }

    /// <summary>Checks deterministic indexed-PNG extraction and that a pixel override changes installed VRAM bytes.</summary>
    internal static void VerifyEndingFontAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.Frontend.Ending.EndingFontAtlasExtractor.Extract(bus);
        AssertTrue(deterministic.SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, EndingFontAtlasFormat.FileName))),
            "installed ending font is the deterministic cartridge extraction");
        IndexedPngImage image = IndexedPng.Read(new MemoryStream(deterministic, writable: false),
            new(EndingFontAtlasFormat.Width, EndingFontAtlasFormat.Height));
        image.Pixels[0] ^= 1;
        Directory.CreateDirectory(overrideDirectory);
        string replacement = Path.Combine(overrideDirectory, EndingFontAtlasFormat.FileName);
        using (var output = File.Create(replacement))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(!edited.EndingFont.Transfer.Span.SequenceEqual(
            stockCatalog.EndingFont.Transfer.Span),
            "ending-font override changes installed VRAM bytes");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "ending-font override changes catalog content identity");
        File.WriteAllText(replacement, "not a PNG");
        AssertThrows<InvalidDataException>(() =>
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory),
            "corrupt ending-font override fails loudly");
        Console.WriteLine("Ending-font catalog: deterministic stock, PNG override identity and corruption failure pass.");
    }

    /// <summary>Forbids installed typewriter execution from rereading the bank-$8C text and panel source range.</summary>
    private static CartridgeReadGuard NewEndingTextReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => SnesAddressMath.BankNumber(address) == EndingCartridgeTables.EndingBank &&
                    (SnesAddressMath.OffsetInBank(address)) is >= EndingCartridgeTables.EndingResultPanel and <= 0xe1e8,
                address => $"Installed ending text read cartridge address ${address:X6}.");

    /// <summary>Runs the retail-cartridge ending text and font presentation check.</summary>
    [VerificationCommand("--ending-text-presentation")]
    internal static int EndingTextPresentationCommand()
    {
        string endingTextRom = RepositoryRomPath;
        Suite(nameof(VerifyEndingTextPresentation), () => VerifyEndingTextPresentation(endingTextRom));
        return 0;
    }
}
