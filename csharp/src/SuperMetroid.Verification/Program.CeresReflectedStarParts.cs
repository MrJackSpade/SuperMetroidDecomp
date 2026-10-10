using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies the reflected Ceres star parts, their independent edit behavior, and serialized draw output against the native even-vortex parts.</summary>
    /// <param name="rom">Address space containing the native Ceres star and even-vortex sprite frames used as reference data.</param>
    private static void VerifyCeresReflectedStarParts(ISnesAddressSpace rom)
    {
        var original = IntroCinematicSpriteFrameExtractor.Extract(rom, 0x9478, 25, "stars");
        var vortex = IntroCinematicSpriteFrameExtractor.Extract(rom, 0x8fe7, 36, "vortex-even");
        var source = IntroCinematicSpriteCompiler.Compile(original, "stars");
        var expected = IntroCinematicSpriteCompiler.Compile(vortex, "vortex");
        var stars = Enumerable.Range(2, 16).Concat(Enumerable.Range(29, 7)).Select(expected.Part).ToArray();
        var calculated = CeresReflectedStarParts.CalculateIfMatching(source, stars);
        AssertTrue(!ReferenceEquals(stars, calculated), "original23 stars select reflection view");
        for (int index = 0; index < 23; index++) AssertEqual(stars[index], calculated[index], "every original reflected star field");
        foreach (int index in new[] { -1, 23, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = calculated[index], "reflected star bounds");
        Check(original, vortex);
        for (int index = 0; index < 23; index++)
        {
            var editedSource = (SpriteVisualPart[])original.Clone();
            editedSource[index + 2] = editedSource[index + 2] with { OffsetX = editedSource[index + 2].OffsetX + 1 };
            var compiledSource = IntroCinematicSpriteCompiler.Compile(editedSource, "edited source");
            AssertTrue(ReferenceEquals(stars, CeresReflectedStarParts.CalculateIfMatching(compiledSource, stars)), "independent source edit does not change target");
            Check(editedSource, vortex);
            int targetIndex = index < 16 ? index + 2 : index + 13;
            var editedTarget = (SpriteVisualPart[])vortex.Clone();
            editedTarget[targetIndex] = editedTarget[targetIndex] with { OffsetX = editedTarget[targetIndex].OffsetX - 1 };
            var compiledTarget = IntroCinematicSpriteCompiler.Compile(editedTarget, "edited target");
            var editedStars = Enumerable.Range(2, 16).Concat(Enumerable.Range(29, 7)).Select(compiledTarget.Part).ToArray();
            AssertTrue(!ReferenceEquals(editedStars, CeresReflectedStarParts.CalculateIfMatching(compiledSource, editedStars)), "matching paired edits retain reflection");
            Check(editedSource, editedTarget);
        }
        void Check(SpriteVisualPart[] sourceVisual, SpriteVisualPart[] targetVisual)
        {
            var frames = CeresFlightSpriteDefinitions.Frames.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>());
            frames["stars"] = sourceVisual; frames["vortex-even"] = targetVisual;
            using var json = new MemoryStream();
            CeresFlightSpritePresentation.Write(json, new() { Version = CeresFlightSpriteFormat.Version, Frames = frames });
            json.Position = 0;
            var loaded = CeresFlightSpritePresentation.Load(json);
            var reference = IntroCinematicSpriteCompiler.Compile(targetVisual, "reference");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var expectedOam = new OamBuffer(); expectedOam.BeginFrame();
                if (y == 72) reference.DrawOnScreen(expectedOam, 120, y, 0x0800);
                else reference.DrawOffScreen(expectedOam, 120, y, 0x0800);
                expectedOam.FinalizeFrame();
                var actual = new OamBuffer(); actual.BeginFrame();
                loaded.Draw(0x8fe7, actual, 120, y, 0x0800, y == 72); actual.FinalizeFrame();
                AssertTrue(expectedOam.LowTable.SequenceEqual(actual.LowTable) && expectedOam.HighTable.SequenceEqual(actual.HighTable)
                    && expectedOam.LastFinalizedSpriteCount == actual.LastFinalizedSpriteCount, "reflection loader preserves independent and paired edits");
            }
        }
    }
}
