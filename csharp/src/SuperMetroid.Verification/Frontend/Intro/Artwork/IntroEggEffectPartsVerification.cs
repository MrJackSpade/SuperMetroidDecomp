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

/// <summary>Checks calculated intro egg-effect sprites against imported composition and native OAM.</summary>
internal static class IntroEggEffectPartsVerification
{
    /// <summary>Checks original-frame identity and visible or wrapped OAM output.</summary>
    internal static void VerifyIntroEggEffectParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("egg-effect", value.AppendIdentity);
        foreach (var definition in IntroEggEffectSpriteDefinitions.Frames)
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, CinematicSpritemapSources.Of(definition.Pose), 1, definition.Name);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = IntroEggEffectParts.CalculateIfMatching(definition.Pose, source);
            AssertTrue(!ReferenceEquals(source, result), "original egg effect selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original egg-effect compiled fields");
            foreach (int invalid in new[] { -1, 1, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated egg part bounds");
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
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native egg-effect OAM at visible and wrapped origins");
            }
        }
    }
}
