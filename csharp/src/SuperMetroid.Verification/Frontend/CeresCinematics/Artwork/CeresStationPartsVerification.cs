using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
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

namespace SuperMetroid.Verification.Frontend.CeresCinematics.Artwork;

/// <summary>Checks Ceres station sprite composition and rendering against imported artwork.</summary>
internal static class CeresStationPartsVerification
{
    /// <summary>Verifies stock source identity, bounds, and native OAM at visible and wrapped origins.</summary>
    internal static void VerifyCeresStationParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("CeresStation", value.AppendIdentity);
        foreach (var definition in CeresFlightSpriteDefinitions.Frames.Where(frame => frame.Name == "station-under-attack"))
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name);
            CheckCeresStationLoader(definition.Pose, visual);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = CeresStationParts.CalculateIfMatching(definition.Pose, source);
            AssertTrue(!ReferenceEquals(source, result), "original CeresStation selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original CeresStation compiled fields");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated CeresStation part bounds");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer(); native.BeginFrame();
                DrawImportedSpritemap(rom, native, 0x8c0000 + CinematicSpritemapSources.Of(definition.Pose), new(120, y), 0x0a00, originIsOnScreen: y == 72);
                native.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                if (y == 72) result.DrawOnScreen(actual, new(120, y), 0x0a00);
                else result.DrawOffScreen(actual, new(120, y), 0x0a00);
                actual.FinalizeFrame();
                AssertTrue(native.ImageEquals(actual)
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native CeresStation OAM at visible and wrapped origins");
            }
        }
    }
    /// <summary>Checks that serialized stock station art draws its parts at visible and wrapped origins.</summary>
    internal static void CheckCeresStationLoader(CinematicSpritePose pose, SpriteVisualPart[] visual)
    {
        var definition = CeresFlightSpriteDefinitions.Frames.FirstOrDefault(frame => frame.Pose == pose);
        if (definition.Name is null) return;
        var frames = CeresFlightSpriteDefinitions.Frames.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>());
        frames[definition.Name] = visual;
        using var json = new MemoryStream();
        CeresFlightSpritePresentation.Write(json, new() { Version = CeresFlightSpriteFormat.Version, Frames = frames });
        json.Position = 0;
        var presentation = CeresFlightSpritePresentation.Load(json);
        var reference = IntroCinematicSpriteCompiler.Compile(visual, "Ceres blast reference");
        foreach (ushort y in new ushort[] { 72, 0xfff8 })
        {
            var expected = new OamBuffer(); expected.BeginFrame();
            if (y == 72) reference.DrawOnScreen(expected, new(120, y), 0x0a00);
            else reference.DrawOffScreen(expected, new(120, y), 0x0a00);
            expected.FinalizeFrame();
            var actual = ScreenSpritesFixtures.Blank(); actual.BeginFrame();
            CinematicSheetFixtures.Drawer(presentation).Draw(pose, actual, new(120, y), CeresShades.Drawn(CeresSpriteColouring.Explosions));
            actual.FinalizeFrame();
            AssertTrue(expected.ImageEquals(actual)
                && expected.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount,
                "Ceres station loader draws the stock composition");
        }
    }}
