using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Intro.Artwork;

/// <summary>Checks calculated introduction egg rocking parts against native OAM.</summary>
internal static class IntroEggRockingPartsVerification
{
    /// <summary>Compares three stock rocking poses, bounds, and visible or wrapped OAM origins.</summary>
    internal static void VerifyIntroEggRockingParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("EggRocking", value.AppendIdentity);
        foreach (var definition in IntroDiscoveryActorSpriteDefinitions.Frames.Take(3))
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = IntroEggRockingParts.CalculateIfMatching(definition.Pose, source);
            AssertTrue(!ReferenceEquals(source, result), "original EggRocking selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original EggRocking compiled fields");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated EggRocking part bounds");
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
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native EggRocking OAM at visible and wrapped origins");
            }
        }
    }
}
