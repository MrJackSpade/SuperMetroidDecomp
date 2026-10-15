using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Frontend.Intro.Artwork;

/// <summary>Checks calculated intro Confused Baby sprites against imported parts and native OAM.</summary>
internal static class IntroConfusedBabyPartsVerification
{
    /// <summary>Preserves independent visual edits and compares stock drawing at visible and wrapped origins.</summary>
    internal static void VerifyIntroConfusedBabyParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("ConfusedBaby", value.AppendIdentity);
        foreach (var definition in IntroDiscoveryActorSpriteDefinitions.Frames.Skip(16).Take(3))
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, definition.Pointer, definition.StockPartCount, definition.Name);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = IntroConfusedBabyParts.CalculateIfMatching(definition.Pointer, source);
            AssertTrue(!ReferenceEquals(source, result), "original ConfusedBaby selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original ConfusedBaby compiled fields");
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
                    var supplied = IntroCinematicSpriteCompiler.Compile(edited, "edited ConfusedBaby");
                    AssertTrue(ReferenceEquals(supplied, IntroConfusedBabyParts.CalculateIfMatching(definition.Pointer, supplied)),
                        "independent ConfusedBaby field edit stays supplied");
                }
            }
            var extra = IntroCinematicSpriteCompiler.Compile(visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]], "custom ConfusedBaby");
            AssertTrue(ReferenceEquals(extra, IntroConfusedBabyParts.CalculateIfMatching(definition.Pointer, extra)), "custom part count preserved");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated ConfusedBaby part bounds");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer(); native.BeginFrame();
                DrawImportedSpritemap(rom, native, 0x8c0000 + definition.Pointer, new(120, y), 0x0e00, originIsOnScreen: y == 72);
                native.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                if (y == 72) result.DrawOnScreen(actual, new(120, y), 0x0e00);
                else result.DrawOffScreen(actual, new(120, y), 0x0e00);
                actual.FinalizeFrame();
                AssertTrue(native.ImageEquals(actual)
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native ConfusedBaby OAM at visible and wrapped origins");
            }
        }
    }
}
