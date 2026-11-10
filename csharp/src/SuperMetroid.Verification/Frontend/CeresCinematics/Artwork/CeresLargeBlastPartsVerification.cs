using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroMotherBrainExplosionPartsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Frontend.CeresCinematics.Artwork;

/// <summary>Checks the installed Ceres large-blast compositions draw the native OAM.</summary>
internal static class CeresLargeBlastPartsVerification
{
    /// <summary>Compares stock parts and native visible or wrapped OAM.</summary>
    internal static void VerifyCeresLargeBlastParts(ISnesAddressSpace rom)
    {
        foreach (var definition in CeresDestructionSpriteFrameDefinitions.Frames.Where(frame => frame.Name.StartsWith("large-blast-", StringComparison.Ordinal)))
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name);
            CheckCeresBlastLoader(definition.Pose, visual);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer(); native.BeginFrame();
                DrawImportedSpritemap(rom, native, 0x8c0000 + CinematicSpritemapSources.Of(definition.Pose), new(120, y), 0x0a00, originIsOnScreen: y == 72);
                native.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                if (y == 72) source.DrawOnScreen(actual, new(120, y), 0x0a00);
                else source.DrawOffScreen(actual, new(120, y), 0x0a00);
                actual.FinalizeFrame();
                AssertTrue(native.ImageEquals(actual)
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native CeresLargeBlast OAM at visible and wrapped origins");
            }
        }
    }
}
