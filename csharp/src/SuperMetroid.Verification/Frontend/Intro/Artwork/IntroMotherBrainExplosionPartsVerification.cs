using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Intro.Artwork;

/// <summary>Checks the extracted Mother Brain explosion compositions draw the native OAM, and the Ceres alias frames load them.</summary>
internal static class IntroMotherBrainExplosionPartsVerification
{
    /// <summary>Compares stock parts and the stock alias loader at visible and wrapped origins.</summary>
    internal static void VerifyIntroMotherBrainExplosionParts(ISnesAddressSpace rom)
    {
        foreach (var definition in IntroMotherBrainExplosionSpriteDefinitions.Frames)
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
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native MotherBrainExplosion OAM at visible and wrapped origins");
            }
        }
    }
    /// <summary>Checks that a Ceres alias frame loads and draws the stock blast composition at both onscreen and wrapped origins.</summary>
    internal static void CheckCeresBlastLoader(CinematicSpritePose pose, SpriteVisualPart[] visual)
    {
        var definition = CeresDestructionSpriteFrameDefinitions.Frames.FirstOrDefault(frame => frame.Pose == pose);
        if (definition.Name is null) return;
        var frames = CeresDestructionSpriteFrameDefinitions.Frames.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>());
        frames[definition.Name] = visual;
        using var json = new MemoryStream();
        CeresDestructionSpritePresentation.Write(json, new() { Version = CeresDestructionSpriteFormat.Version, Frames = frames });
        json.Position = 0;
        var presentation = CeresDestructionSpritePresentation.Load(json);
        var reference = IntroCinematicSpriteCompiler.Compile(visual, "Ceres blast reference");
        foreach (ushort y in new ushort[] { 72, 0xfff8 })
        {
            var expected = new OamBuffer(); expected.BeginFrame();
            if (y == 72) reference.DrawOnScreen(expected, new(120, y), 0x0a00);
            else reference.DrawOffScreen(expected, new(120, y), 0x0a00);
            expected.FinalizeFrame();
            var actual = ScreenSpritesFixtures.Blank(); actual.BeginFrame();
            CinematicSheetFixtures.Drawer(presentation).Draw(pose, actual, new(120, y), IntroShades.Drawn(IntroSpriteColouring.Explosions));
            actual.FinalizeFrame();
            AssertTrue(expected.ImageEquals(actual)
                && expected.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount,
                "Ceres alias loader draws the stock blast composition");
        }
    }}
