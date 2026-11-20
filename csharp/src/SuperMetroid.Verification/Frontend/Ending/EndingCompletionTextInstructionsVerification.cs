using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks ending completion-text instruction words, sprite prefix frames, and their compositions' OAM against the cartridge.</summary>
internal static class EndingCompletionTextInstructionsVerification
{
    /// <summary>Compares all published completion-text glyphs and their compositions with the cartridge.</summary>
    internal static void VerifyEndingCompletionTextInstructions(ISnesAddressSpace bus)
    {
        // The editable artwork lists the typed texts' steps (#2195), then the digits and the colon.
        CompletionArtwork[] catalog =
        [
            .. EndingCompletionTextSpriteDefinitions.Typings.Select(step =>
                new CompletionArtwork(step.Name, CinematicSpritemapSources.Of(step.Typed), step.StockPartCount)),
            .. EndingCompletionTextSpriteDefinitions.Frames.Select(frame =>
                new CompletionArtwork(frame.Name, CinematicSpritemapSources.Of(frame.Pose), frame.StockPartCount)),
        ];
        string[] expectedKeys =
        [
            "OperationWas.Letter1", "OperationWas.Letter2", "OperationWas.Letter3", "OperationWas.Letter4", "OperationWas.Letter5",
            "OperationWas.Letter6", "OperationWas.Letter7", "OperationWas.Letter8", "OperationWas.Letter9", "OperationWas.Letter10",
            "OperationWas.Letter11", "OperationWas.Letter12", "OperationWas.Letter13", "OperationWas.Letter14", "OperationWas.Letter15",
            "CompletedSuccessfully.Letter1", "CompletedSuccessfully.Letter2", "CompletedSuccessfully.Letter3", "CompletedSuccessfully.Letter4", "CompletedSuccessfully.Letter5",
            "CompletedSuccessfully.Letter6", "CompletedSuccessfully.Letter7", "CompletedSuccessfully.Letter8", "CompletedSuccessfully.Letter9", "CompletedSuccessfully.Letter10",
            "CompletedSuccessfully.Letter11", "CompletedSuccessfully.Letter12", "CompletedSuccessfully.Letter13", "CompletedSuccessfully.Letter14", "CompletedSuccessfully.Letter15",
            "CompletedSuccessfully.Letter16", "CompletedSuccessfully.Letter17", "CompletedSuccessfully.Letter18", "CompletedSuccessfully.Letter19", "CompletedSuccessfully.Letter20",
            "CompletedSuccessfully.Letter21", "ClearTime.Letter1", "ClearTime.Letter2", "ClearTime.Letter3", "ClearTime.Letter4",
            "ClearTime.Letter5", "ClearTime.Letter6", "ClearTime.Letter7", "ClearTime.Letter8", "ClearTime.Letter9",
            "digit-00", "digit-01", "digit-02", "digit-03", "digit-04",
            "digit-05", "digit-06", "digit-07", "digit-08", "digit-09",
            "colon",
        ];
        AssertEqual(expectedKeys.Length, catalog.Length, "completion catalog count");
        int catalogIndex = 0;
        // Original OAM headers independently establish two sprites per revealed
        // letter, so packed maps grow by two header bytes plus five per sprite.
        EndingTypedText[] texts = [EndingTypedText.OperationWas, EndingTypedText.CompletedSuccessfully, EndingTypedText.ClearTime];
        var lines = new[] { new VerifyEndingCompletionTextInstructionsCase(Start:0xeb91, Letters:15), new VerifyEndingCompletionTextInstructionsCase(Start:0xebd7, Letters:21), new VerifyEndingCompletionTextInstructionsCase(Start:0xec35, Letters:9) };
        for (int text = 0; text < lines.Length; text++)
        {
            VerifyEndingCompletionTextInstructionsCase line = lines[text];
            AssertEqual(line.Letters, EndingTextAnimations.Letters(texts[text]), $"{texts[text]} types one step per native prefix");
            for (int letter = 0; letter < line.Letters; letter++)
            {
                int operand = 0x8b0000 | (line.Start + letter * 4 + 2);
                int map = CartridgeImportSource.Require(bus).ReadCartridgeWord(operand);
                int address = 0x8c0000 | map;
                int count = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
                AssertEqual(2 * (letter + 1), count, "original prefix sprite count");
                AssertEqual(new EndingTextTyping(texts[text], letter + 1), EndingCompletionTextSpriteDefinitions.Typings[catalogIndex].Typed,
                    "completion step typed as far as its letter");
                AssertEqual((ushort)map, catalog[catalogIndex].Spritemap, "completion prefix catalog pointer");
                AssertEqual(count, catalog[catalogIndex].StockPartCount, "completion prefix catalog part count");
                catalogIndex++;
            }
        }
        for (int glyph = 0; glyph < 11; glyph++, catalogIndex++)
        {
            int operand = 0x8bec83 + glyph * 8;
            ushort map = CartridgeImportSource.Require(bus).ReadCartridgeWord(operand);
            int header = 0x8c0000 | map;
            int parts = CartridgeImportSource.Require(bus).ReadCartridgeWord(header);
            AssertEqual(map, catalog[catalogIndex].Spritemap, "completion glyph catalog pointer");
            AssertEqual(parts, catalog[catalogIndex].StockPartCount, "completion glyph catalog part count");
        }
        for (int i = 0; i < expectedKeys.Length; i++)
        {
            AssertEqual(expectedKeys[i], catalog[i].Name, "completion published artwork key");
            VerifyEndingCompletionTextComposition(bus, catalog[i]);
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(expectedKeys), "completion catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 56, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = catalog[invalid], "completion catalog bounds");
    }

    /// <summary>One composition of the editable completion text artwork: its name, its spritemap and its stock part count.</summary>
    /// <param name="Name">The composition's name in the editable artwork.</param>
    /// <param name="Spritemap">The bank-$8C spritemap it is read from.</param>
    /// <param name="StockPartCount">How many parts its stock composition holds.</param>
    private readonly record struct CompletionArtwork(string Name, ushort Spritemap, int StockPartCount);

    /// <summary>Checks the extracted composition draws the native OAM at on- and off-screen origins.</summary>
    private static void VerifyEndingCompletionTextComposition(ISnesAddressSpace bus, CompletionArtwork definition)
    {
        SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(bus, definition.Spritemap, definition.StockPartCount, definition.Name);
        SpriteComposition supplied = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
        foreach (ushort y in new ushort[] { 72, 0xfff8 })
        {
            var originalOam = new OamBuffer();
            var compiledOam = new OamBuffer();
            originalOam.BeginFrame(); compiledOam.BeginFrame();
            DrawImportedSpritemap(bus, originalOam, 0x8c0000 | definition.Spritemap, new(120, y), 0x0800, originIsOnScreen: y == 72);
            if (y == 72) supplied.DrawOnScreen(compiledOam, new(120, y), 0x0800);
            else supplied.DrawOffScreen(compiledOam, new(120, y), 0x0800);
            originalOam.FinalizeFrame(); compiledOam.FinalizeFrame();
            AssertTrue(originalOam.ImageEquals(compiledOam),
                "completion text preserves native OAM and clipping");
        }
    }
}
