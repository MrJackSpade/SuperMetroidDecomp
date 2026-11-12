using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Frontend.Intro.Artwork;

/// <summary>Checks the extracted intro scientist compositions draw the native OAM, and the caret loader draws its composition.</summary>
internal static class IntroScientistPartsVerification
{
    /// <summary>
    /// Compares stock parts at visible and wrapped origins, and the stock caret through its loader. The caret's block is the
    /// caret family's own pose, no scientist-scene baby's (#1654), so it is extracted from the caret's definition.
    /// </summary>
    internal static void VerifyIntroScientistParts(ISnesAddressSpace rom)
    {
        foreach (var definition in IntroScientistSpriteDefinitions.Frames)
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer(); native.BeginFrame();
                DrawImportedSpritemap(rom, native, 0x8c0000 + CinematicSpritemapSources.Of(definition.Pose), new(120, y), 0x0c00, originIsOnScreen: y == 72);
                native.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                if (y == 72) source.DrawOnScreen(actual, new(120, y), 0x0c00);
                else source.DrawOffScreen(actual, new(120, y), 0x0c00);
                actual.FinalizeFrame();
                AssertTrue(native.ImageEquals(actual)
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native Scientist OAM at visible and wrapped origins");
            }
        }
        CheckCaretLoader(rom);
    }

    /// <summary>Draws the caret's stock composition through the caret loader and compares it with the composition drawn directly.</summary>
    private static void CheckCaretLoader(ISnesAddressSpace rom)
    {
        CinematicSpriteFrameDefinition caret = IntroCaretSpriteDefinitions.Visible;
        SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, CinematicSpritemapSources.Of(caret.Pose), caret.StockPartCount, caret.Name);
        var expected = new OamBuffer(); expected.BeginFrame();
        IntroCinematicSpriteCompiler.Compile(visual, "caret reference").DrawOnScreen(expected, new(120, 72), 0x0c00);
        expected.FinalizeFrame();
        var frames = new Dictionary<string, SpriteVisualPart[]> { [caret.Name] = visual };
        using var json = new MemoryStream();
        IntroCaretSpritePresentation.Write(json, new() { Version = IntroCaretSpriteVersion.Current.Revision, Frames = frames });
        json.Position = 0;
        var presentation = IntroCaretSpritePresentation.Load(json);
        var actual = ScreenSpritesFixtures.Blank(); actual.BeginFrame();
        CinematicSheetFixtures.Drawer(presentation).Draw(caret.Pose, actual, new(120, 72), IntroShades.Drawn(IntroSpriteColouring.CrossFadeObjects));
        actual.FinalizeFrame();
        AssertTrue(expected.ImageEquals(actual)
            && expected.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount,
            "the caret loader draws the stock composition");
    }
}
