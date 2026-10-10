using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies calculated Ceres vortex compositions preserve source identity, shared-part edits, loader shape, and native OAM output.</summary>
    /// <param name="rom">Address space containing the retail vortex spritemaps and tile data used as the comparison source.</param>
    private static void VerifyCeresVortexParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("CeresStation", value.AppendIdentity);
        foreach (var definition in CeresFlightSpriteDefinitions.Frames.Where(frame => frame.Name.StartsWith("vortex-", StringComparison.Ordinal)))
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, definition.Pointer, definition.StockPartCount, definition.Name);
            CheckCeresStationLoader(definition.Pointer, visual);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = CeresVortexParts.CalculateIfMatching(definition.Pointer, source);
            AssertTrue(!ReferenceEquals(source, result), "original CeresStation selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original CeresStation compiled fields");
            for (int piece = 0; piece < visual.Length; piece++)
            {
                var part = visual[piece];
                SpriteVisualPart[] edits =
                [
                    part with { OffsetX = part.OffsetX + 1 }, part with { OffsetY = part.OffsetY + 1 },
                    part with { TileColumn = (part.TileColumn + 1) % 15 }, part with { TileRow = (part.TileRow + 1) % (part.Size == 16 ? 31 : 32) },
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
                    var selected = CeresVortexParts.CalculateIfMatching(definition.Pointer, supplied);
                    AssertEqual(Identity(supplied), Identity(selected), "edited planet identity");
                    bool odd = definition.Pointer == CeresFlightSpriteDefinitions.VortexOdd;
                    int tip = odd ? 3 : 0, core = odd ? 12 : 18;
                    bool sharedPart = (piece >= tip && piece < tip + 2) || (piece >= core && piece < core + 11);
                    AssertTrue(sharedPart == ReferenceEquals(supplied, selected),
                        "independent CeresStation field edit stays supplied");
                }
            }
            CheckCeresStationLoader(definition.Pointer, visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]]);
            var extra = IntroCinematicSpriteCompiler.Compile(visual.Length == 1 ? [visual[0], visual[0]] : [visual[0]], "custom CeresStation");
            AssertTrue(ReferenceEquals(extra, CeresVortexParts.CalculateIfMatching(definition.Pointer, extra)), "custom part count preserved");
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
