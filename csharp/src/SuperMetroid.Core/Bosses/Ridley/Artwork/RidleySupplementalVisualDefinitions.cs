using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Fixed wing and articulated-tail visual identities selected by Ridley's
/// wing and tail draws. Pose cadence and segment motion remain gameplay state.
/// </summary>
internal static class RidleySupplementalVisualDefinitions
{

    /// <summary>The installed frame each wing-cycle index shows, ten phases facing left then ten facing right.</summary>
    internal static EnemyFrameTable<int> WingFrames { get; } = new()
    {
        { 0, EnemyFrameId.RidleyWingFacingLeftFirst },
        { 1, EnemyFrameId.RidleyWingFacingLeftSecond },
        { 2, EnemyFrameId.RidleyWingFacingLeftThird },
        { 3, EnemyFrameId.RidleyWingFacingLeftFourth },
        { 4, EnemyFrameId.RidleyWingFacingLeftFifth },
        { 5, EnemyFrameId.RidleyWingFacingLeftSixth },
        { 6, EnemyFrameId.RidleyWingFacingLeftFifth },
        { 7, EnemyFrameId.RidleyWingFacingLeftFourth },
        { 8, EnemyFrameId.RidleyWingFacingLeftThird },
        { 9, EnemyFrameId.RidleyWingFacingLeftSecond },
        { 10, EnemyFrameId.RidleyWingFacingRightFirst },
        { 11, EnemyFrameId.RidleyWingFacingRightSecond },
        { 12, EnemyFrameId.RidleyWingFacingRightThird },
        { 13, EnemyFrameId.RidleyWingFacingRightFourth },
        { 14, EnemyFrameId.RidleyWingFacingRightFifth },
        { 15, EnemyFrameId.RidleyWingFacingRightSixth },
        { 16, EnemyFrameId.RidleyWingFacingRightFifth },
        { 17, EnemyFrameId.RidleyWingFacingRightFourth },
        { 18, EnemyFrameId.RidleyWingFacingRightThird },
        { 19, EnemyFrameId.RidleyWingFacingRightSecond },
    };

    /// <summary>
    /// The installed frame the drawn tail tip shows for each sixteenth of the turn its direction can point into, in turn
    /// order: each frame's sixteenth is centred on it, so the first opens half a frame before straight down.
    /// </summary>
    internal static TurnArcs<EnemyFrameId> TailTipFrames { get; } = new(TrigonometryAngle.Wrapped(-8),
        EnemyFrameId.RidleyBreakupTailTipFirst, EnemyFrameId.RidleyBreakupTailTipSecond, EnemyFrameId.RidleyBreakupTailTipThird,
        EnemyFrameId.RidleyBreakupTailTipFourth, EnemyFrameId.RidleyBreakupTailTipFifth, EnemyFrameId.RidleyBreakupTailTipSixth,
        EnemyFrameId.RidleyBreakupTailTipSeventh, EnemyFrameId.RidleyBreakupTailTipEighth, EnemyFrameId.RidleyBreakupTailTipNinth,
        EnemyFrameId.RidleyBreakupTailTipTenth, EnemyFrameId.RidleyBreakupTailTipEleventh, EnemyFrameId.RidleyBreakupTailTipTwelfth,
        EnemyFrameId.RidleyBreakupTailTipThirteenth, EnemyFrameId.RidleyBreakupTailTipFourteenth, EnemyFrameId.RidleyBreakupTailTipFifteenth,
        EnemyFrameId.RidleyBreakupTailTipSixteenth);

    /// <summary>The installed frame each of the six drawn tail links shows, large to small.</summary>
    internal static EnemyFrameTable<int> SegmentFrames { get; } = new()
    {
        { 0, EnemyFrameId.RidleyTailSegmentSmall },
        { 1, EnemyFrameId.RidleyTailSegmentSmall },
        { 2, EnemyFrameId.RidleyTailSegmentMedium },
        { 3, EnemyFrameId.RidleyTailSegmentMedium },
        { 4, EnemyFrameId.RidleyTailSegmentLarge },
        { 5, EnemyFrameId.RidleyTailSegmentLarge },
    };
}
