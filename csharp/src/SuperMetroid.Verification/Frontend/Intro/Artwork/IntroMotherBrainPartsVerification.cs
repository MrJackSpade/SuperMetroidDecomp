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

/// <summary>Checks calculated intro Mother Brain portrait parts against
/// native spritemaps while preserving independently edited artwork.</summary>
internal static class IntroMotherBrainPartsVerification
{
    /// <summary>Compares all stock part fields and OAM at visible and wrapped
    /// origins, then checks per-field edits bypass calculated substitution.</summary>
    internal static void VerifyIntroMotherBrainParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("MotherBrain", value.AppendIdentity);
        foreach (var definition in IntroMotherBrainSpriteDefinitions.Frames)
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, CinematicSpritemapSources.Of(definition.Pose), IntroMotherBrainSpriteDefinitions.StockPartCount, definition.Name);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            var calculated = new IntroMotherBrainParts(Enumerable.Range(0, 3).Single(frame => IntroMotherBrainSpritePoses.FramePose(frame) == definition.Pose));
            for (int index = 0; index < visual.Length; index++)
                AssertTrue(source.Part(index).Equals(calculated[index]), $"{definition.Name} part{index}: expected{source.Part(index)},actual{calculated[index]}");
            SpriteComposition result = IntroMotherBrainParts.CalculateIfMatching(definition.Pose, source);
            AssertTrue(!ReferenceEquals(source, result), "original MotherBrain selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original MotherBrain compiled fields");
            for (int piece = 0; piece < visual.Length; piece++)
            {
                var part = visual[piece];
                SpriteVisualPart[] edits =
                [
                    part with { OffsetX = part.OffsetX + 1 }, part with { OffsetY = part.OffsetY + 1 },
                    part with { TileColumn = (part.TileColumn + 1) % 15 }, part with { TileRow = part.TileRow + 1 },
                    part with { Size = part.Size == 8 ? 16 : 8, TileColumn = Math.Min(14, part.TileColumn) },
                    part with { Priority = 2 }, part with { Palette = 1 },
                    part with { FlipX = !part.FlipX }, part with { FlipY = !part.FlipY },
                ];
                foreach (var edit in edits)
                {
                    var edited = (SpriteVisualPart[])visual.Clone();
                    edited[piece] = edit;
                    var supplied = IntroCinematicSpriteCompiler.Compile(edited, "edited MotherBrain");
                    AssertTrue(ReferenceEquals(supplied, IntroMotherBrainParts.CalculateIfMatching(definition.Pose, supplied)),
                        "independent MotherBrain field edit stays supplied");
                }
            }
            var extra = IntroCinematicSpriteCompiler.Compile(visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]], "custom MotherBrain");
            AssertTrue(ReferenceEquals(extra, IntroMotherBrainParts.CalculateIfMatching(definition.Pose, extra)), "custom part count preserved");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated MotherBrain part bounds");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer(); native.BeginFrame();
                DrawImportedSpritemap(rom, native, 0x8c0000 + CinematicSpritemapSources.Of(definition.Pose), new(120, y), 0x0e00, originIsOnScreen: y == 72);
                native.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                if (y == 72) result.DrawOnScreen(actual, new(120, y), 0x0e00);
                else result.DrawOffScreen(actual, new(120, y), 0x0e00);
                actual.FinalizeFrame();
                AssertTrue(native.ImageEquals(actual)
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native MotherBrain OAM at visible and wrapped origins");
            }
        }
    }
}
