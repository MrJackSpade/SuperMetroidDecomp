using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies calculated intro Rinka parts preserve native composition identity, reject unrelated edits, and reproduce OAM at visible and wrapped origins.</summary>
    private static void VerifyIntroRinkaParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("Rinka", value.AppendIdentity);
        foreach (var definition in IntroRinkaSpriteDefinitions.Frames)
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, definition.Pointer, 4, definition.Name);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = IntroRinkaParts.CalculateIfMatching(definition.Pointer, source);
            AssertTrue(!ReferenceEquals(source, result), "original Rinka selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original Rinka compiled fields");
            for (int piece = 0; piece < visual.Length; piece++)
            {
                var part = visual[piece];
                SpriteVisualPart[] edits =
                [
                    part with { OffsetX = part.OffsetX + 1 }, part with { OffsetY = part.OffsetY + 1 },
                    part with { TileColumn = (part.TileColumn + 1) % 16 }, part with { TileRow = part.TileRow + 1 },
                    part with { Size = 16, TileColumn = Math.Min(14, part.TileColumn) },
                    part with { Priority = 2 }, part with { Palette = 1 },
                    part with { FlipX = !part.FlipX }, part with { FlipY = !part.FlipY },
                ];
                foreach (var edit in edits)
                {
                    var edited = (SpriteVisualPart[])visual.Clone();
                    edited[piece] = edit;
                    var supplied = IntroCinematicSpriteCompiler.Compile(edited, "edited Rinka");
                    AssertTrue(ReferenceEquals(supplied, IntroRinkaParts.CalculateIfMatching(definition.Pointer, supplied)),
                        "independent Rinka field edit stays supplied");
                }
            }
            var extra = IntroCinematicSpriteCompiler.Compile([visual[0]], "one-part Rinka");
            AssertTrue(ReferenceEquals(extra, IntroRinkaParts.CalculateIfMatching(definition.Pointer, extra)), "custom part count preserved");
            foreach (int invalid in new[] { -1, 4, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated Rinka part bounds");
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
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native Rinka OAM at visible and wrapped origins");
            }
        }
    }
}
