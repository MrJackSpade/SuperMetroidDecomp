using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies calculated Ceres asteroid compositions preserve source identity, translation, and native OAM output for both animations.</summary>
    /// <param name="rom">Address space containing the retail sprite lists and tile data used as the comparison source.</param>
    private static void VerifyCeresLargeAsteroidParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("CeresLargeAsteroid", value.AppendIdentity);
        foreach (var definition in new[] { new { Pointer = (ushort)0x909d, StockPartCount = 19, Name = "under-attack" }, new { Pointer = (ushort)0x94f7, StockPartCount = 19, Name = "approach" } })
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, definition.Pointer, definition.StockPartCount, definition.Name);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = CeresLargeAsteroidParts.CalculateIfMatching(definition.Pointer, source);
            AssertTrue(!ReferenceEquals(source, result), "original CeresLargeAsteroid selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original CeresLargeAsteroid compiled fields");
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
                    var supplied = IntroCinematicSpriteCompiler.Compile(edited, "edited CeresLargeAsteroid");
                    AssertTrue(ReferenceEquals(supplied, CeresLargeAsteroidParts.CalculateIfMatching(definition.Pointer, supplied)),
                        "independent CeresLargeAsteroid field edit stays supplied");
                }
            }
            foreach (var group in new[] { (Start: 0, End: 6, Anchor: 5), (Start: 6, End: 13, Anchor: 12), (Start: 13, End: 19, Anchor: 18) })
            {
                var translated = (SpriteVisualPart[])visual.Clone();
                for (int index = group.Start; index < group.End; index++)
                    translated[index] = translated[index] with { OffsetX = translated[index].OffsetX + 3, OffsetY = translated[index].OffsetY - 2 };
                var translatedSource = IntroCinematicSpriteCompiler.Compile(translated, "translated asteroid");
                var translatedResult = CeresLargeAsteroidParts.CalculateIfMatching(definition.Pointer, translatedSource);
                AssertTrue(!ReferenceEquals(translatedSource, translatedResult), "whole asteroid translation keeps calculated shape");
                AssertEqual(Identity(translatedSource), Identity(translatedResult), "independent asteroid anchor preserved");
                var edge = (SpriteVisualPart[])visual.Clone();
                edge[group.Anchor] = edge[group.Anchor] with { OffsetX = 255 };
                var edgeSource = IntroCinematicSpriteCompiler.Compile(edge, "custom boundary anchor");
                AssertTrue(ReferenceEquals(edgeSource, CeresLargeAsteroidParts.CalculateIfMatching(definition.Pointer, edgeSource)),
                    "custom anchor whose inferred shape exceeds X bounds stays supplied");
            }
            var extra = IntroCinematicSpriteCompiler.Compile(visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]], "custom CeresLargeAsteroid");
            AssertTrue(ReferenceEquals(extra, CeresLargeAsteroidParts.CalculateIfMatching(definition.Pointer, extra)), "custom part count preserved");
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated CeresLargeAsteroid part bounds");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer(); native.BeginFrame();
                DrawImportedSpritemap(rom, native, 0x8c0000 + definition.Pointer, 120, y, 0x0e00, originIsOnScreen: y == 72);
                native.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                if (y == 72) result.DrawOnScreen(actual, 120, y, 0x0e00);
                else result.DrawOffScreen(actual, 120, y, 0x0e00);
                actual.FinalizeFrame();
                AssertTrue(native.LowTable.SequenceEqual(actual.LowTable) && native.HighTable.SequenceEqual(actual.HighTable)
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native CeresLargeAsteroid OAM at visible and wrapped origins");
            }
        }
    }
}
