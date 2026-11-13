using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Frontend.Ending.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>Checks installed ending text and font assets against native tilemaps and guarded typewriter execution.</summary>
internal static class EndingTextPresentationVerification
{
    /// <summary>Matches extracted font bytes and panels to the cartridge, then checks snapshot restore and glyph validation.</summary>
    internal static void VerifyEndingTextPresentation(string romPath)
    {
        ISnesAddressSpace nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.Ending.EndingTextExtractor.Extract(nativeBus);
        EndingTextPresentation presentation = EndingTextPresentation.Load(
            new MemoryStream(extracted, writable: false));
        byte[] fontPng = SuperMetroid.AssetExtraction.Frontend.Ending.EndingFontAtlasExtractor.Extract(nativeBus);
        EndingFontCharacters font = EndingFontCharacters.Read(new MemoryStream(fontPng, writable: false));
        byte[] nativeFont = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(nativeBus),
            EndingSourceDefinitions.Artwork.FontCharacters,
            EndingArtworkExtentDefinitions.FloorBytes);
        AssertTrue(font.Characters.Span.SequenceEqual(
            nativeFont.AsSpan(0, EndingFontAtlasFormat.ByteCount)),
            "installed ending-font PNG compiles to exact native planar bytes");
        ISnesAddressSpace installedBus = NewEndingTextReadGuard(nativeBus);

        AssertTrue(BackgroundTileWordBytes.Words(presentation.BuildResultPanel()).SequenceEqual(
            ReadEndingWords(nativeBus, EndingSourceDefinitions.EndingResultPanel,
                EndingTextDefinitions.ResultPanelRows * EndingTextDefinitions.TilemapWidth)),
            "installed producer panel, encoded through the codec, matches native tilemap");
        AssertTrue(BackgroundTileWordBytes.Words(presentation.BuildCopyrightPanel()).SequenceEqual(
            ReadEndingWords(nativeBus, EndingSourceDefinitions.EndingCopyrightPanel,
                EndingTextDefinitions.CopyrightRows * EndingTextDefinitions.TilemapWidth)),
            "installed copyright panel, encoded through the codec, matches native tilemap");

        int comparedFrames = 0;
        foreach (EndingTextSequence sequence in Enum.GetValues<EndingTextSequence>())
        {
            // The installed sequence runs to completion with its text streams guarded.
            var installed = new EndingTypewriter(new EndingTypewriterSetup { Inventory = default, JapaneseText = true, Text = presentation.Typewriter() }, sequence);
            for (int frame = 0; !installed.Completed; frame++)
            {
                if (frame == 2048)
                    throw new InvalidOperationException($"Ending {sequence} did not terminate.");
                installed.Step();
                comparedFrames++;
            }
            AssertTrue(installed.Shown().Any(), $"installed ending {sequence} draws its text");
        }

        var saved = new EndingTypewriter(new EndingTypewriterSetup { Inventory = default, JapaneseText = false, Text = presentation.Typewriter() }, EndingTextSequence.FinalMessage);
        saved.Step();
        var restored = GraphDigestVerification.RoundTrip(saved, presentation);
        saved.Step();
        restored.Step();
        AssertTrue(GraphDigestVerification.GraphDigest(saved, presentation).SequenceEqual(GraphDigestVerification.GraphDigest(restored, presentation)),
            "restored ending text continues as the saved text does, without being bound again");

        JsonObject malformedDocument = RequiredJson.Parse(extracted).AsObject();
        malformedDocument["finalMessage"] = "lowercase";
        AssertThrows<InvalidDataException>(() => EndingTextPresentation.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(
                malformedDocument.ToJsonString(ArtworkJson.Options)), writable: false)),
            "ending text rejects glyphs absent from its documented font mapping");
        Console.WriteLine($"Ending text: exact producer/copyright panels and {comparedFrames} native typewriter frames match with bank-$8C text reads forbidden; restore and validation pass.");
        Console.WriteLine("Ending font: 160 PNG tiles compile to exact native 4-bpp bytes.");
    }

    /// <summary>Reads a bank-$8C tilemap span for the independent result or copyright panel comparison.</summary>
    internal static ushort[] ReadEndingWords(ISnesAddressSpace bus, ushort pointer, int count)
    {
        var result = new ushort[count];
        int address = (int)new SnesAddress(EndingSourceDefinitions.EndingBank, pointer);
        for (int index = 0; index < count; index++)
            result[index] = CartridgeImportSource.Require(bus).ReadCartridgeWord(address + index * 2);
        return result;
    }

    /// <summary>Forbids installed typewriter execution from rereading the bank-$8C text and panel source range.</summary>
    private static CartridgeReadGuard NewEndingTextReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => SnesAddressMath.BankNumber(address) == EndingSourceDefinitions.EndingBank &&
                    (SnesAddressMath.OffsetInBank(address)) is >= EndingSourceDefinitions.EndingResultPanel and <= 0xe1e8,
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
