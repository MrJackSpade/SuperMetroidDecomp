using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroMotherBrainExplosionPartsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Verification.Frontend.CeresCinematics.Artwork;

/// <summary>Checks calculated Zebes planet bands against native sprite parts
/// while preserving editable Ceres destruction presentation.</summary>
internal static class ZebesPlanetBandPartsVerification
{
    /// <summary>Compares stock part fields and visible or wrapped OAM, then
    /// checks each independently edited field bypasses calculated substitution.</summary>
    internal static void VerifyZebesPlanetBandParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("ZebesPlanetBand", value.AppendIdentity);
        foreach (var definition in CeresDestructionSpriteDefinitions.Frames.Where(frame => frame.Name == "planet-zebes"))
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name);
            CheckCeresBlastLoader(definition.Pose, visual);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = ZebesPlanetBandParts.CalculateIfMatching(definition.Backdrop, source);
            AssertTrue(!ReferenceEquals(source, result), "original ZebesPlanetBand selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original ZebesPlanetBand compiled fields");
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
                    CheckCeresBlastLoader(definition.Pose, edited);
                    var supplied = IntroCinematicSpriteCompiler.Compile(edited, "edited ZebesPlanetBand");
                    var selected = ZebesPlanetBandParts.CalculateIfMatching(definition.Backdrop, supplied);
                    AssertEqual(Identity(supplied), Identity(selected), "edited planet identity");
                    AssertTrue(ReferenceEquals(supplied, selected),
                        "independent ZebesPlanetBand field edit stays supplied");
                }
            }
            CheckCeresBlastLoader(definition.Pose, visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]]);
            var extra = IntroCinematicSpriteCompiler.Compile(visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]], "custom ZebesPlanetBand");
            AssertTrue(ReferenceEquals(extra, ZebesPlanetBandParts.CalculateIfMatching(definition.Backdrop, extra)), "custom part count preserved");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated ZebesPlanetBand part bounds");
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
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native ZebesPlanetBand OAM at visible and wrapped origins");
            }
        }
    }
}
