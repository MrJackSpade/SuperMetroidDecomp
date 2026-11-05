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
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.CeresCinematics.Artwork;

/// <summary>Checks compiled Ceres station-blast sprite parts against imported data and OAM.</summary>
internal static class CeresStationBlastPartsVerification
{
    /// <summary>Compares stock frames and native OAM at visible and wrapped origins.</summary>
    internal static void VerifyCeresStationBlastParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("CeresStationBlast", value.AppendIdentity);
        foreach (var definition in CeresDestructionSpriteFrameDefinitions.Frames.Where(frame => frame.Name.StartsWith("station-blast-", StringComparison.Ordinal)))
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name);
            CheckCeresBlastLoader(definition.Pose, visual);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = CeresStationBlastParts.CalculateIfMatching(definition.Pose, source);
            AssertTrue(!ReferenceEquals(source, result), "original CeresStationBlast selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original CeresStationBlast compiled fields");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated CeresStationBlast part bounds");
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
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native CeresStationBlast OAM at visible and wrapped origins");
            }
        }
    }
}
