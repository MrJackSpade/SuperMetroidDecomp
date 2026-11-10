using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Frontend.CeresCinematics.Artwork;

/// <summary>Checks the installed vortex's reflected stars draw as installed.</summary>
internal static class CeresReflectedStarPartsVerification
{
    /// <summary>Verifies the stock vortex OAM after presentation reload.</summary>
    internal static void VerifyCeresReflectedStarParts(ISnesAddressSpace rom)
    {
        var original = IntroCinematicSpriteFrameExtractor.Extract(rom, 0x9478, 25, "stars");
        var vortex = IntroCinematicSpriteFrameExtractor.Extract(rom, 0x8fe7, 36, "vortex-even");
        Check(original, vortex);
        void Check(SpriteVisualPart[] sourceVisual, SpriteVisualPart[] targetVisual)
        {
            var frames = CeresFlightSpriteDefinitions.Frames.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>());
            frames["stars"] = sourceVisual; frames["vortex-even"] = targetVisual;
            using var json = new MemoryStream();
            CeresFlightSpritePresentation.Write(json, new() { Version = CeresFlightSpriteFormat.Version, Frames = frames });
            json.Position = 0;
            var loaded = CeresFlightSpritePresentation.Load(json);
            var reference = IntroCinematicSpriteCompiler.Compile(targetVisual, "reference");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var expectedOam = new OamBuffer(); expectedOam.BeginFrame();
                if (y == 72) reference.DrawOnScreen(expectedOam, new(120, y), 0x0800);
                else reference.DrawOffScreen(expectedOam, new(120, y), 0x0800);
                expectedOam.FinalizeFrame();
                var actual = ScreenSpritesFixtures.Blank(); actual.BeginFrame();
                CinematicSheetFixtures.Drawer(loaded).Draw(CeresFlightSpritePoses.VortexPose(CeresVortexFrame.Even), actual, new(120, y), CeresShades.Drawn(CeresSpriteColouring.SpaceObjects)); actual.FinalizeFrame();
                AssertTrue(expectedOam.ImageEquals(actual)
                    && expectedOam.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "reflection loader draws the stock vortex stars");
            }
        }
    }
}
