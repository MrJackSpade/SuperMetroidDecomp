using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks calculated intro egg-effect parts, preserves custom visual edits and part counts, and compares visible and wrapped output with native OAM.</summary>
    /// <param name="rom">Retail address space used to extract egg-effect frames and build the native OAM reference.</param>
    private static void VerifyIntroEggEffectParts(ISnesAddressSpace rom)
    {
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("egg-effect", value.AppendIdentity);
        foreach (var definition in IntroEggEffectSpriteDefinitions.Frames)
        {
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(rom, definition.Pointer, 1, definition.Name);
            SpriteComposition source = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            SpriteComposition result = IntroEggEffectParts.CalculateIfMatching(definition.Pointer, source);
            AssertTrue(!ReferenceEquals(source, result), "original egg effect selects calculated parts");
            AssertEqual(Identity(source), Identity(result), "all original egg-effect compiled fields");
            var part = visual[0];
            SpriteVisualPart[] edits =
            [
                part with { OffsetX = part.OffsetX + 1 }, part with { OffsetY = part.OffsetY + 1 },
                part with { TileColumn = (part.TileColumn + 1) % 16 }, part with { TileRow = part.TileRow + 1 },
                part with { Size = 16, TileColumn = Math.Min(14, part.TileColumn) },
                part with { Priority = 2 }, part with { Palette = 1 },
                part with { FlipX = true }, part with { FlipY = true },
            ];
            foreach (var edit in edits)
            {
                var supplied = IntroCinematicSpriteCompiler.Compile([edit], "edited egg effect");
                AssertTrue(ReferenceEquals(supplied, IntroEggEffectParts.CalculateIfMatching(definition.Pointer, supplied)),
                    "independent egg-effect field edit stays supplied");
            }
            var extra = IntroCinematicSpriteCompiler.Compile([part, part], "two-part effect");
            AssertTrue(ReferenceEquals(extra, IntroEggEffectParts.CalculateIfMatching(definition.Pointer, extra)), "custom part count preserved");
            foreach (int invalid in new[] { -1, 1, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => result.Part(invalid), "calculated egg part bounds");
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
                    && native.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "native egg-effect OAM at visible and wrapped origins");
            }
        }
    }
}
