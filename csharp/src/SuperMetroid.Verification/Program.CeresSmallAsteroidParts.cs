using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies calculated small-asteroid parts preserve edited visual identity and match native OAM at visible and wrapped origins.</summary>
    /// <param name="rom">Address space containing the imported Ceres spritemap used as the native rendering reference.</param>
    private static void VerifyCeresSmallAsteroidParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("CeresStation", value.AppendIdentity);
        foreach (var definition in CeresFlightSpriteDefinitions.Frames.Where(frame => frame.Name == "small-asteroid"))
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, definition.Pointer, definition.StockPartCount, definition.Name);
            CheckCeresStationLoader(definition.Pointer, visual);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = CeresSmallAsteroidParts.CalculateIfMatching(definition.Pointer, source);
            AssertTrue(!ReferenceEquals(source, result), "original CeresStation selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original CeresStation compiled fields");
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
                    CheckCeresStationLoader(definition.Pointer, edited);
                    var supplied = IntroCinematicSpriteCompiler.Compile(edited, "edited CeresStation");
                    var selected = CeresSmallAsteroidParts.CalculateIfMatching(definition.Pointer, supplied);
                    AssertEqual(Identity(supplied), Identity(selected), "edited planet identity");
                    AssertTrue(ReferenceEquals(supplied, selected),
                        "independent CeresStation field edit stays supplied");
                }
            }
            CheckCeresStationLoader(definition.Pointer, visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]]);
            var extra = IntroCinematicSpriteCompiler.Compile(visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]], "custom CeresStation");
            AssertTrue(ReferenceEquals(extra, CeresSmallAsteroidParts.CalculateIfMatching(definition.Pointer, extra)), "custom part count preserved");
            foreach (var group in new[] { (Start: 0, Count: 2), (Start: 2, Count: 2), (Start: 4, Count: 2), (Start: 6, Count: 3), (Start: 9, Count: 2), (Start: 11, Count: 2), (Start: 13, Count: 3) })
            {
                var translated = (SpriteVisualPart[])visual.Clone();
                for (int index = group.Start; index < group.Start + group.Count; index++)
                    translated[index] = translated[index] with { OffsetX = translated[index].OffsetX + 2, OffsetY = translated[index].OffsetY - 1 };
                var supplied = IntroCinematicSpriteCompiler.Compile(translated, "translated small asteroid");
                var selected = CeresSmallAsteroidParts.CalculateIfMatching(definition.Pointer, supplied);
                AssertTrue(!ReferenceEquals(supplied, selected), "translated whole asteroid retains calculated shape");
                AssertEqual(Identity(supplied), Identity(selected), "independent small asteroid placement preserved");
                CheckCeresStationLoader(definition.Pointer, translated);
            }
            foreach (int invalid in new[] { -1, result.PartCount, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated CeresStation part bounds");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer(); native.BeginFrame();
                DrawImportedSpritemap(rom, native, 0x8c0000 + definition.Pointer, 120, y, 0x0a00, originIsOnScreen: y == 72);
                native.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                if (y == 72) result.DrawOnScreen(actual, 120, y, 0x0a00);
                else result.DrawOffScreen(actual, 120, y, 0x0a00);
                actual.FinalizeFrame();
                AssertTrue(native.LowTable.SequenceEqual(actual.LowTable) && native.HighTable.SequenceEqual(actual.HighTable)
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native CeresStation OAM at visible and wrapped origins");
            }
        }
    }
}
