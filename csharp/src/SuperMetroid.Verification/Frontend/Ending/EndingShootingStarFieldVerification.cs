using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>
/// Checks the square field the post-credits stars fly in (#1627): a star keeps flying up to the field's far edge on both
/// axes, which lies the screen's width away, past the bottom of the screen, and returns to the origin once beyond it.
/// </summary>
internal static class EndingShootingStarFieldVerification
{
    /// <summary>The origin a star returns to.</summary>
    private static readonly Offset16<ScreenPixels> Origin = new(EndingShootingStarDefinitions.Origin, EndingShootingStarDefinitions.Origin);

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
        // Left of or above the screen a star's place is negative, which lies nearer than the sprite offset (#2020).
        StepStar(1, new(-1, 128), Origin, "a star left of the screen returns to the origin");
        StepStar(1, new(128, -1), Origin, "a star above the screen returns to the origin");
    }

    /// <summary>The animation period the cartridge gives every still star, in updates.</summary>
    private const int CartridgeStillStarPeriod = 3840;

    /// <summary>
    /// #1983: every still star's flight, from the origin until it leaves the field and returns, is shorter than the
    /// cartridge's period for it, so that period never elapsed and the star showed its first visible frame for the whole
    /// flight, as the still star does; it starts its second flight at once, still at that frame.
    /// </summary>
    internal static void VerifyEndingStillShootingStars()
    {
        int still = 0;
        for (int index = 0; index < EndingShootingStarDefinitions.Count; index++)
        {
            if (EndingShootingStarDefinitions.Stars[index].Pace.Animates)
                continue;
            still++;
            var stars = new EndingShootingStars();
            AssertTrue(!stars.Stars[index].Delayed, $"still star {index} flies from the start");
            int updates = 0;
            bool returned = false;
            while (!returned && updates < CartridgeStillStarPeriod)
            {
                stars.Step();
                updates++;
                EndingShootingStar star = stars.Stars[index];
                returned = star.Look == EndingShootingStarLook.LeadIn && star.Flight.UpdatesLeft == EndingShootingStarDefinitions.LeadInDuration;
                AssertTrue(star.Look is EndingShootingStarLook.LeadIn or EndingShootingStarLook.FaintSpeck,
                    $"still star {index} shows no picture past its first, update {updates}");
                if (updates > EndingShootingStarDefinitions.LeadInDuration && !returned)
                    AssertEqual(EndingShootingStarLook.FaintSpeck, star.Look, $"still star {index} shows its first visible picture after its lead-in, update {updates}");
            }
            AssertTrue(returned, $"still star {index} returns to the origin within the cartridge's {CartridgeStillStarPeriod}-update period");
            AssertEqual(EndingShootingStarLook.LeadIn, stars.Stars[index].Look, $"still star {index} restarts its lead-in when it returns");
        }
        AssertEqual(12, still, "the post-credits sky holds twelve still stars");
    }

    /// <summary>Places star <paramref name="index"/> at rest at <paramref name="start"/>, steps the stars once and checks where it is.</summary>
    private static void StepStar(int index, Offset16<ScreenPixels> start, Offset16<ScreenPixels> expected, string context)
    {
        var stars = new EndingShootingStars();
        AssertTrue(!stars.Stars[index].Delayed, $"star {index} flies from the start");
        stars.Stars[index].Position = FixedVector<ScreenPixels>.FromWholePixels(start);
        stars.Step();
        AssertEqual(expected, stars.Stars[index].Position.Whole, context);
    }
}
