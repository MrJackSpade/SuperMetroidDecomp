using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Frontend.Intro.Artwork;

/// <summary>Checks calculated intro scientist sprites against native OAM and both caret artwork schemas.</summary>
internal static class IntroScientistPartsVerification
{
    /// <summary>Compares stock parts at visible and wrapped origins and preserves independent edits through the caret loader.</summary>
    internal static void VerifyIntroScientistParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("Scientist", value.AppendIdentity);
        foreach (var definition in IntroScientistSpriteDefinitions.Frames)
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name);
            CheckCaretLoader(definition.Pose, visual);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = IntroScientistParts.CalculateIfMatching(definition.Pose, source);
            AssertTrue(!ReferenceEquals(source, result), "original Scientist selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original Scientist compiled fields");
            for (int piece = 0; piece < visual.Length; piece++)
            {
                var part = visual[piece];
                SpriteVisualPart[] edits =
                [
                    part with { OffsetX = part.OffsetX + 1 }, part with { OffsetY = part.OffsetY + 1 },
                    part with { TileColumn = (part.TileColumn + 1) % 16 }, part with { TileRow = part.TileRow + 1 },
                    part with { Size = part.Size == 8 ? 16 : 8, TileColumn = Math.Min(14, part.TileColumn) },
                    part with { Priority = 2 }, part with { Palette = 1 },
                    part with { FlipX = !part.FlipX }, part with { FlipY = !part.FlipY },
                ];
                foreach (var edit in edits)
                {
                    var edited = (SpriteVisualPart[])visual.Clone();
                    edited[piece] = edit;
                    CheckCaretLoader(definition.Pose, edited);
                    var supplied = IntroCinematicSpriteCompiler.Compile(edited, "edited Scientist");
                    AssertTrue(ReferenceEquals(supplied, IntroScientistParts.CalculateIfMatching(definition.Pose, supplied)),
                        "independent Scientist field edit stays supplied");
                }
            }
            CheckCaretLoader(definition.Pose, visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]]);
            var extra = IntroCinematicSpriteCompiler.Compile(visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]], "custom Scientist");
            AssertTrue(ReferenceEquals(extra, IntroScientistParts.CalculateIfMatching(definition.Pose, extra)), "custom part count preserved");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated Scientist part bounds");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer(); native.BeginFrame();
                DrawImportedSpritemap(rom, native, 0x8c0000 + CinematicSpritemapSources.Of(definition.Pose), new(120, y), 0x0c00, originIsOnScreen: y == 72);
                native.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                if (y == 72) result.DrawOnScreen(actual, new(120, y), 0x0c00);
                else result.DrawOffScreen(actual, new(120, y), 0x0c00);
                actual.FinalizeFrame();
                AssertTrue(native.ImageEquals(actual)
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native Scientist OAM at visible and wrapped origins");
            }
        }

        static void CheckCaretLoader(CinematicSpritePose pose, SpriteVisualPart[] visual)
        {
            if (pose != IntroCaretSpriteDefinitions.Visible.Pose) return;
            var expected = new OamBuffer(); expected.BeginFrame();
            IntroCinematicSpriteCompiler.Compile(visual, "caret reference").DrawOnScreen(expected, new(120, 72), 0x0c00);
            expected.FinalizeFrame();
            foreach (IntroCaretSpriteVersion version in Enum.GetValues<IntroCaretSpriteVersion>())
            {
                var frames = new Dictionary<string, SpriteVisualPart[]>();
                if (version == IntroCaretSpriteVersion.Previous)
                {
                    foreach (string name in IntroCaretSpriteDefinitions.PreviousFrameNames) frames.Add(name, []);
                    frames[IntroCaretSpriteDefinitions.PreviousFrameNames[0]] = visual;
                }
                else frames.Add("caret-visible", visual);
                using var json = new MemoryStream();
                IntroCaretSpritePresentation.Write(json, new() { Version = (int)version, Frames = frames });
                json.Position = 0;
                var presentation = IntroCaretSpritePresentation.Load(json);
                var actual = ScreenSpritesFixtures.Blank(); actual.BeginFrame();
                CinematicSheetFixtures.Drawer(presentation).Draw(pose, actual, new(120, 72), 0x0c00);
                actual.FinalizeFrame();
                AssertTrue(expected.ImageEquals(actual)
                    && expected.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount,
                    "both caret schema loaders preserve original and independently edited compositions");
            }
        }
    }
}
