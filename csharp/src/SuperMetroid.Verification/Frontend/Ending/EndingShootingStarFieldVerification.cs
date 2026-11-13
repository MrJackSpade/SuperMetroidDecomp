using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>
/// Checks the square field the post-credits stars fly in (#1627): a star keeps flying up to the field's far edge on both
/// axes, which lies the screen's width away, past the bottom of the screen, and returns to the origin once beyond it.
/// </summary>
internal static class EndingShootingStarFieldVerification
{
    /// <summary>The origin a star returns to.</summary>
    private static readonly Position16<ScreenPixels> Origin = new(EndingShootingStarDefinitions.Origin, EndingShootingStarDefinitions.Origin);

    /// <summary>
    /// Star 1 drifts right and slightly up, star 11 slightly right and down, both from the start; each is placed on and
    /// just past an edge of the field and stepped once.
    /// </summary>
    internal static void VerifyEndingShootingStarField()
    {
        AssertEqual(255, EndingShootingStarDefinitions.FieldFarEdge, "the field's far edge is the screen's last column");
        // On the edge a star keeps flying; one pixel past it the star returns to the origin.
        StepStar(1, new(255, 128), new(255, 127), "a star on the field's right edge keeps flying");
        StepStar(1, new(256, 128), Origin, "a star past the field's right edge returns to the origin");
        StepStar(11, new(128, 255), new(128, 255), "a star below the screen but on the field's bottom edge keeps flying");
        StepStar(11, new(128, 256), Origin, "a star past the field's bottom edge returns to the origin");
        StepStar(1, new(4, 128), new(4, 127), "a star on the field's near edge keeps flying");
        StepStar(1, new(3, 128), Origin, "a star nearer the left than the sprite offset returns to the origin");
    }

    /// <summary>Places star <paramref name="index"/> at rest at <paramref name="start"/>, steps the stars once and checks where it is.</summary>
    private static void StepStar(int index, Position16<ScreenPixels> start, Position16<ScreenPixels> expected, string context)
    {
        var stars = new EndingShootingStars();
        AssertTrue(!stars.stars[index].Delayed, $"star {index} flies from the start");
        stars.stars[index].Position = FixedPosition<ScreenPixels>.FromWhole(start);
        stars.Step();
        AssertEqual(expected, stars.stars[index].Position.Whole, context);
    }
}
