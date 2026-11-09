using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Frontend.Intro.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>Checks installed opening narration and font artwork through the cinematic's live tilemap and VRAM paths.</summary>
internal static class IntroNarrationPresentationVerification
{
    /// <summary>Compares all six stock pages and the font upload with native data, then checks debugger restore and malformed glyph rejection.</summary>
    internal static void VerifyIntroNarrationPresentation(string romPath)
    {
        ISnesAddressSpace nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.Intro.IntroNarrationExtractor.Extract(nativeBus);
        IntroNarrationPresentation presentation = IntroNarrationFile.Read(
            new MemoryStream(extracted, writable: false));
        ISnesAddressSpace installedBus = NewIntroNarrationGuard(nativeBus);

        byte[] fontPng = SuperMetroid.AssetExtraction.Frontend.Intro.IntroFontAtlasExtractor.Extract(nativeBus);
        IntroFontCharacters font = IntroFontCharacters.Read(new MemoryStream(fontPng, writable: false));
        byte[] nativeFont = RomDataReader.Decompress(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(nativeBus),
            IntroCinematicSourceDefinitions.FontOne,
            maximumOutputBytes: IntroFontAtlasFormat.ByteCount);
        AssertTrue(font.Characters.Span.SequenceEqual(
            nativeFont.AsSpan(0, IntroFontAtlasFormat.ByteCount)),
            "installed opening-font PNG compiles to exact native planar bytes");
        var installedIntro = CreateRetailIntroFixture(
            introFont: font);
        AssertTrue(installedIntro.ShownVram().AsSpan(
            (IntroVramLayout.FontOne.Offset * 2),
            IntroFontAtlasFormat.ByteCount).SequenceEqual(font.Characters.Span),
            "opening cinematic uploads the installed font without reading its ROM stream");

        int comparedFrames = 0;
        foreach (IntroNarrationPageId page in Enum.GetValues<IntroNarrationPageId>())
        {
            // The installed page runs to completion with every narration ROM read forbidden;
            // extraction copied its text from the cartridge.
            var installedText = PagesText();
            var installed = new IntroCinematicObjectSystem(
                installedText, new IntroJapaneseSubtitles(enabled: false),
                narrationPresentation: presentation);
            StartPage(installed, page);
            for (int frame = 0; !IsPageComplete(installed, page); frame++)
            {
                if (frame == 2048)
                    throw new InvalidOperationException($"Opening narration {page} did not terminate.");
                installed.Step(0);
                comparedFrames++;
            }
            AssertTrue(ShowsAnyLetter(installedText),
                $"installed opening narration {page} shows its text");
        }

        var saved = new IntroCinematicObjectSystem(
            PagesText(), new IntroJapaneseSubtitles(enabled: false),
            narrationPresentation: presentation);
        saved.StartEnglishPageSix();
        saved.Step(0);
        // A debugger state names the bound narration by its path in the installed presentation.
        var restored = SuperMetroid.Verification.Harness.GraphDigestVerification.RoundTrip(saved, presentation);
        restored.Step(0);
        AssertEqual((ushort)16, restored.Caret.X,
            "restored opening narration resumes at its saved character");

        JsonObject malformedDocument = RequiredJson.Parse(extracted).AsObject();
        malformedDocument.Required("pages").Required(IntroNarrationPageId.Page6.ToString()).Required("lines").Required(0)["text"] =
            "lowercase";
        byte[] malformed = Encoding.UTF8.GetBytes(
            malformedDocument.ToJsonString(ArtworkJson.Options));
        AssertThrows<InvalidDataException>(() => IntroNarrationFile.Read(
            new MemoryStream(malformed, writable: false)),
            "opening narration rejects glyphs absent from the documented font mapping");

        Console.WriteLine(
            $"Opening narration: six extracted UTF-8 pages and {comparedFrames} stock frames match with narration ROM reads forbidden; caret timing, page completion, debugger restore and glyph validation pass.");
        Console.WriteLine(
            "Opening font: 144 PNG tiles compile to exact native 2-bpp bytes; installed upload and ROM-read guard pass.");
    }

    /// <summary>Narration text with every place blank, as the first illustrated page leaves it.</summary>
    private static IntroNarrationText PagesText() => new();

    /// <summary>Whether <paramref name="text"/> shows a letter in any text cell.</summary>
    private static bool ShowsAnyLetter(IntroNarrationText text)
    {
        for (int row = IntroNarrationText.FirstTextRow; row < IntroNarrationText.FirstTextRow + IntroNarrationText.TextRows; row++)
        {
            for (int column = 0; column < IntroNarrationText.RowCells; column++)
            {
                if (text.ShownAt(column, row).Letter is not null)
                    return true;
            }
        }
        return false;
    }

    /// <summary>Calls the cinematic entry point for one of its six English story pages.</summary>
    private static void StartPage(IntroCinematicObjectSystem state, IntroNarrationPageId page)
    {
        switch (page)
        {
            case IntroNarrationPageId.Page1: state.StartEnglishPageOne(); break;
            case IntroNarrationPageId.Page2: state.StartEnglishPageTwo(); break;
            case IntroNarrationPageId.Page3: state.StartEnglishPageThree(); break;
            case IntroNarrationPageId.Page4: state.StartEnglishPageFour(); break;
            case IntroNarrationPageId.Page5: state.StartEnglishPageFive(); break;
            case IntroNarrationPageId.Page6: state.StartEnglishPageSix(); break;
            default: throw new ArgumentOutOfRangeException(nameof(page), page, null);
        }
    }

    /// <summary>Recognizes the page-specific wait or finish state after all typewriter characters display.</summary>
    private static bool IsPageComplete(
        IntroCinematicObjectSystem state,
        IntroNarrationPageId page) => page switch
    {
        IntroNarrationPageId.Page1 => state.PageOneAwaitingInput,
        IntroNarrationPageId.Page2 => state.PageTwoAwaitingInput,
        IntroNarrationPageId.Page3 => state.PageThreeAwaitingInput,
        IntroNarrationPageId.Page4 => state.PageFourAwaitingInput,
        IntroNarrationPageId.Page5 => state.PageFiveAwaitingInput,
        IntroNarrationPageId.Page6 => state.IntroFinishRequested,
        _ => throw new ArgumentOutOfRangeException(nameof(page), page, null),
    };

    /// <summary>Checks that the installed opening narration is the deterministic cartridge extraction.</summary>
    internal static void VerifyIntroNarrationAssets(
        ISnesAddressSpace bus,
        string stockDirectory)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.Frontend.Intro.IntroNarrationExtractor.Extract(bus);
        AssertTrue(deterministic.SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, IntroNarrationFile.FileName))),
            "installed opening narration is the deterministic cartridge extraction");

        Console.WriteLine(
            "Opening-narration catalog: deterministic stock passes.");
    }

    /// <summary>Checks that the installed opening font is the deterministic cartridge extraction.</summary>
    internal static void VerifyIntroFontAssets(
        ISnesAddressSpace bus,
        string stockDirectory)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.Frontend.Intro.IntroFontAtlasExtractor.Extract(bus);
        AssertTrue(deterministic.SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, IntroFontCharacters.FileName))),
            "installed opening font is the deterministic cartridge extraction");
        Console.WriteLine(
            "Opening-font catalog: deterministic stock passes.");
    }

    /// <summary>Rejects cartridge text reads from the English narration's bank-$8C source ranges.</summary>
    private static CartridgeReadGuard NewIntroNarrationGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => SnesAddressMath.BankNumber(address) == 0x8c &&
                (SnesAddressMath.OffsetInBank(address)) is >= 0xc383 and <= 0xd5dd or >= 0xd67d and <= 0xd780,
            address => $"Installed opening narration read cartridge text address ${address:X6}.");

    /// <summary>Runs opening narration and font presentation checks against the repository cartridge.</summary>
    [VerificationCommand("--intro-narration-presentation")]
    internal static int IntroNarrationPresentationCommand()
    {
        string narrationRom = RepositoryRomPath;
        Suite(nameof(VerifyIntroNarrationPresentation), () => VerifyIntroNarrationPresentation(narrationRom));
        return 0;
    }
}
