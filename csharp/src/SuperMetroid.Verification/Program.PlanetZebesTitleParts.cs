using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static IntroMotherBrainExplosionPartsVerification;
using static MapSpritesVerification;

/// <summary>Checks calculated Planet Zebes title parts against native OAM and preserves independent art edits.</summary>
internal static class PlanetZebesTitlePartsVerification
{
    /// <summary>Compares the stock title pose, altered part fields, custom counts, bounds, and visible or wrapped OAM origins.</summary>
    internal static void VerifyPlanetZebesTitleParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("PlanetZebesTitle", value.AppendIdentity);
        foreach (var definition in CeresDestructionSpriteDefinitions.Frames.Where(frame => frame.Name == "planet-zebes-title"))
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, definition.Pointer, definition.StockPartCount, definition.Name);
            CheckCeresBlastLoader(definition.Pointer, visual);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = PlanetZebesTitleParts.CalculateIfMatching(definition.Backdrop, source);
            AssertTrue(!ReferenceEquals(source, result), "original PlanetZebesTitle selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original PlanetZebesTitle compiled fields");
            for (int piece = 0; piece < visual.Length; piece++)
            {
                var part = visual[piece];
                SpriteVisualPart[] edits =
                [
                    part with { OffsetX = part.OffsetX + 1 }, part with { OffsetY = part.OffsetY + 1 },
                    part with { TileColumn = (part.TileColumn + 1) % 16 }, part with { TileRow = part.TileRow + 1 },
                    part with { Size = part.Size == 8 ? 16 : 8, TileColumn = Math.Min(14, part.TileColumn) },
                    part with { Priority = 2 }, part with { Palette = 1 },
                    part with { FlipX = !part.FlipX }, part with { FlipY = !part.FlipY },
                ];
                foreach (var edit in edits)
                {
                    var edited = (SpriteVisualPart[])visual.Clone();
                    edited[piece] = edit;
                    CheckCeresBlastLoader(definition.Pointer, edited);
                    var supplied = IntroCinematicSpriteCompiler.Compile(edited, "edited PlanetZebesTitle");
                    AssertTrue(ReferenceEquals(supplied, PlanetZebesTitleParts.CalculateIfMatching(definition.Backdrop, supplied)),
                        "independent PlanetZebesTitle field edit stays supplied");
                }
            }
            CheckCeresBlastLoader(definition.Pointer, visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]]);
            var extra = IntroCinematicSpriteCompiler.Compile(visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]], "custom PlanetZebesTitle");
            AssertTrue(ReferenceEquals(extra, PlanetZebesTitleParts.CalculateIfMatching(definition.Backdrop, extra)), "custom part count preserved");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated PlanetZebesTitle part bounds");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer(); native.BeginFrame();
                DrawImportedSpritemap(rom, native, 0x8c0000 + definition.Pointer, new(120, y), 0x0a00, originIsOnScreen: y == 72);
                native.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                if (y == 72) result.DrawOnScreen(actual, new(120, y), 0x0a00);
                else result.DrawOffScreen(actual, new(120, y), 0x0a00);
                actual.FinalizeFrame();
                AssertTrue(native.ImageEquals(actual)
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native PlanetZebesTitle OAM at visible and wrapped origins");
            }
        }
    }
}
