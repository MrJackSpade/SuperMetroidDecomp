using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks ending completion-text instruction words, sprite prefix frames, and OAM parts against the cartridge.</summary>
internal static class EndingCompletionTextInstructionsVerification
{
    /// <summary>Compares all published completion-text glyphs and their compositions with the cartridge.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending completion text instructions")]
    internal static void VerifyEndingCompletionTextInstructions(ISnesAddressSpace bus)
    {
        var catalog = EndingCompletionTextSpriteDefinitions.Frames;
        string[] expectedKeys =
        [
            "operation-00", "operation-01", "operation-02", "operation-03", "operation-04",
            "operation-05", "operation-06", "operation-07", "operation-08", "operation-09",
            "operation-10", "operation-11", "operation-12", "operation-13", "operation-14",
            "completed-00", "completed-01", "completed-02", "completed-03", "completed-04",
            "completed-05", "completed-06", "completed-07", "completed-08", "completed-09",
            "completed-10", "completed-11", "completed-12", "completed-13", "completed-14",
            "completed-15", "completed-16", "completed-17", "completed-18", "completed-19",
            "completed-20", "clear-time-00", "clear-time-01", "clear-time-02", "clear-time-03",
            "clear-time-04", "clear-time-05", "clear-time-06", "clear-time-07", "clear-time-08",
            "digit-00", "digit-01", "digit-02", "digit-03", "digit-04",
            "digit-05", "digit-06", "digit-07", "digit-08", "digit-09",
            "colon-00",
        ];
        AssertEqual(expectedKeys.Length, catalog.Count, "completion catalog count");
        int catalogIndex = 0;
        // Original OAM headers independently establish two sprites per revealed
        // letter, so packed maps grow by two header bytes plus five per sprite.
        foreach (var line in new[] { new VerifyEndingCompletionTextInstructionsCase(Start:0xeb91, Letters:15), new VerifyEndingCompletionTextInstructionsCase(Start:0xebd7, Letters:21), new VerifyEndingCompletionTextInstructionsCase(Start:0xec35, Letters:9) })
        for (int letter = 0; letter < line.Letters; letter++)
        {
            int operand = 0x8b0000 | (line.Start + letter * 4 + 2);
            int map = CartridgeImportSource.Require(bus).ReadCartridgeWord(operand);
            int address = 0x8c0000 | map;
            int count = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
            AssertEqual(2 * (letter + 1), count, "original prefix sprite count");
            AssertEqual((ushort)map, CinematicSpritemapSources.Of(catalog[catalogIndex].Pose), "completion prefix catalog pointer");
            AssertEqual(count, catalog[catalogIndex].StockPartCount, "completion prefix catalog part count");
            catalogIndex++;
        }
        for (int glyph = 0; glyph < 11; glyph++, catalogIndex++)
        {
            int operand = 0x8bec83 + glyph * 8;
            ushort map = CartridgeImportSource.Require(bus).ReadCartridgeWord(operand);
            int header = 0x8c0000 | map;
            int parts = CartridgeImportSource.Require(bus).ReadCartridgeWord(header);
            AssertEqual(map, CinematicSpritemapSources.Of(catalog[catalogIndex].Pose), "completion glyph catalog pointer");
            AssertEqual(parts, catalog[catalogIndex].StockPartCount, "completion glyph catalog part count");
        }
        for (int i = 0; i < expectedKeys.Length; i++)
        {
            AssertEqual(expectedKeys[i], catalog[i].Name, "completion published artwork key");
            VerifyEndingCompletionTextParts(bus, i, catalog[i]);
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(expectedKeys), "completion catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 56, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = catalog[invalid], "completion catalog bounds");
    }

    /// <summary>Checks calculated stock composition, on- and off-screen OAM, and retention of independent artwork edits.</summary>
    private static void VerifyEndingCompletionTextParts(ISnesAddressSpace bus, int frame, CinematicSpriteFrameDefinition definition)
    {
        SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(bus, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name);
        SpriteComposition supplied = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
        var parts = new EndingCompletionTextParts(frame);
        SpriteComposition calculated = supplied.CalculateIfMatching(parts);
        AssertTrue(!ReferenceEquals(supplied, calculated), "original completion text uses calculated layout");
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("completion-text", value.AppendIdentity);
        AssertEqual(Identity(supplied), Identity(calculated), "all original completion text fields and order");
        foreach (ushort y in new ushort[] { 72, 0xfff8 })
        {
            var originalOam = new OamBuffer();
            var calculatedOam = new OamBuffer();
            originalOam.BeginFrame(); calculatedOam.BeginFrame();
            DrawImportedSpritemap(bus, originalOam, 0x8c0000 | CinematicSpritemapSources.Of(definition.Pose), new(120, y), 0x0800, originIsOnScreen: y == 72);
            if (y == 72) calculated.DrawOnScreen(calculatedOam, new(120, y), 0x0800);
            else calculated.DrawOffScreen(calculatedOam, new(120, y), 0x0800);
            originalOam.FinalizeFrame(); calculatedOam.FinalizeFrame();
            AssertTrue(originalOam.ImageEquals(calculatedOam),
                "completion text preserves native OAM and clipping");
        }
        SpriteVisualPart first = visual[0];
        foreach (SpriteVisualPart edit in new[]
        {
            first with { OffsetX = first.OffsetX + 1 }, first with { OffsetY = first.OffsetY + 1 },
            first with { TileColumn = (first.TileColumn + 1) % 16 }, first with { TileRow = first.TileRow + 1 },
            first with { Size = 16, TileColumn = Math.Min(14, first.TileColumn) },
            first with { Priority = 2 }, first with { Palette = 3 },
            first with { FlipX = !first.FlipX }, first with { FlipY = !first.FlipY },
        })
        {
            visual[0] = edit;
            SpriteComposition edited = IntroCinematicSpriteCompiler.Compile(visual, "edited completion text");
            AssertTrue(ReferenceEquals(edited, edited.CalculateIfMatching(parts)), "independent completion artwork edits stay supplied");
        }
        foreach (int invalid in new[] { int.MinValue, -1, parts.Count, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = parts[invalid], "completion part bounds");
    }
}
