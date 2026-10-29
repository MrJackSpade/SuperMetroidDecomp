using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Frontend.CeresCinematics.Artwork;

/// <summary>Checks calculated Ceres star geometry against retail OAM and independent supplied edits.</summary>
internal static class CeresStarPointPartsVerification
{
    /// <summary>Checks both reflected layouts, priority edits, translated points, and invalid indexes.</summary>
    internal static void VerifyCeresStarPointParts(ISnesAddressSpace rom)
    {
        foreach (bool reflected in new[] { false, true })
        {
            var visual = IntroCinematicSpriteFrameExtractor.Extract(rom, reflected ? (ushort)0x9478 : (ushort)0x93d1, reflected ? 25 : 33, "stars");
            var composition = IntroCinematicSpriteCompiler.Compile(visual, "stars");
            var indices = reflected ? Enumerable.Range(0, 25)
                : Enumerable.Range(0, 3).Concat(Enumerable.Range(5, 7)).Concat(Enumerable.Range(23, 10));
            var supplied = indices.Select(composition.Part).ToArray();
            var result = CeresStarPointParts.CalculateIfMatching(supplied, reflected ? TileFlip.Horizontal : TileFlip.None);
            AssertTrue(!ReferenceEquals(supplied, result), "original star attributes select calculated view");
            AssertTrue(supplied.SequenceEqual(result), "every original star field");
            for (int index = 0; index < supplied.Length; index++)
            {
                var edited = (CompiledSpritePart[])supplied.Clone();
                var part = edited[index];
                // The priority edit flips the priority tier's low bit; the point edit moves the star one pixel down, wrapping
                // across the signed Y offset range as the cartridge's Y byte does.
                edited[index] = part with { Part = part.Part with { Priority = part.Part.Priority ^ 1 } };
                AssertTrue(ReferenceEquals(edited, CeresStarPointParts.CalculateIfMatching(edited, reflected ? TileFlip.Horizontal : TileFlip.None)), "independent priority edit stays supplied");
                edited[index] = part with { Part = part.Part.At(Offset16<ScreenPixels>.Of(part.Part.Offset.X, unchecked((sbyte)(part.Part.Offset.Y + 1)))) };
                var translated = CeresStarPointParts.CalculateIfMatching(edited, reflected ? TileFlip.Horizontal : TileFlip.None);
                AssertTrue(!ReferenceEquals(edited, translated) && edited.SequenceEqual(translated), "point edits preserve calculated attributes");
            }
            foreach (int invalid in new[] { -1, result.Count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = result[invalid], "star point bounds");
        }
    }
}
