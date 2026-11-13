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

    /// <summary>The installed frame each sixteenth-turn tail-tip sector shows.</summary>
    internal static EnemyFrameTable<int> TailTipFrames { get; } = new()
    {
        { 0, EnemyFrameId.RidleyBreakupTailTipFirst },
        { 1, EnemyFrameId.RidleyBreakupTailTipSecond },
        { 2, EnemyFrameId.RidleyBreakupTailTipThird },
        { 3, EnemyFrameId.RidleyBreakupTailTipFourth },
        { 4, EnemyFrameId.RidleyBreakupTailTipFifth },
        { 5, EnemyFrameId.RidleyBreakupTailTipSixth },
        { 6, EnemyFrameId.RidleyBreakupTailTipSeventh },
        { 7, EnemyFrameId.RidleyBreakupTailTipEighth },
        { 8, EnemyFrameId.RidleyBreakupTailTipNinth },
        { 9, EnemyFrameId.RidleyBreakupTailTipTenth },
        { 10, EnemyFrameId.RidleyBreakupTailTipEleventh },
        { 11, EnemyFrameId.RidleyBreakupTailTipTwelfth },
        { 12, EnemyFrameId.RidleyBreakupTailTipThirteenth },
        { 13, EnemyFrameId.RidleyBreakupTailTipFourteenth },
        { 14, EnemyFrameId.RidleyBreakupTailTipFifteenth },
        { 15, EnemyFrameId.RidleyBreakupTailTipSixteenth },
    };

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
