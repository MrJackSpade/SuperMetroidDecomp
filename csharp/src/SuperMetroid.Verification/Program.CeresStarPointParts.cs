using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresStarPointParts(ISnesAddressSpace rom)
    {
        foreach (bool reflected in new[] { false, true })
        {
            var visual = IntroCinematicSpriteFrameExtractor.Extract(rom, reflected ? (ushort)0x9478 : (ushort)0x93d1, reflected ? 25 : 33, "stars");
            var composition = IntroCinematicSpriteCompiler.Compile(visual, "stars");
            var indices = reflected ? Enumerable.Range(0, 25)
                : Enumerable.Range(0, 3).Concat(Enumerable.Range(5, 7)).Concat(Enumerable.Range(23, 10));
            var supplied = indices.Select(composition.Part).ToArray();
            var result = CeresStarPointParts.CalculateIfMatching(supplied, reflected);
            AssertTrue(!ReferenceEquals(supplied, result), "original star attributes select calculated view");
            AssertTrue(supplied.SequenceEqual(result), "every original star field");
            for (int index = 0; index < supplied.Length; index++)
            {
                var edited = (CompiledSpritePart[])supplied.Clone();
                var part = edited[index];
                edited[index] = part with { Attributes = part.Attributes with { Raw = (ushort)(part.Attributes.Raw ^ 0x1000) } };
                AssertTrue(ReferenceEquals(edited, CeresStarPointParts.CalculateIfMatching(edited, reflected)), "independent priority edit stays supplied");
                edited[index] = part with { Y = unchecked((byte)(part.Y + 1)) };
                var translated = CeresStarPointParts.CalculateIfMatching(edited, reflected);
                AssertTrue(!ReferenceEquals(edited, translated) && edited.SequenceEqual(translated), "point edits preserve calculated attributes");
            }
            foreach (int invalid in new[] { -1, result.Count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = result[invalid], "star point bounds");
        }
    }
}