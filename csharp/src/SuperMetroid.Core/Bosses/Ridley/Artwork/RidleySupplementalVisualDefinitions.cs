using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Fixed wing and articulated-tail visual identities selected by Ridley's
/// $A6:DADE/$A6:DB2A hooks. Pose cadence and segment motion remain gameplay state.
/// </summary>
internal static class RidleySupplementalVisualDefinitions
{

    /// <summary>The installed frame each wing-cycle index shows, ten phases facing left then ten facing right.</summary>
    internal static EnemyFrameTable<int> WingFrames { get; } = new()
    {
        { 0, EnemyFrameId.RidleyWingLeft0 },
        { 1, EnemyFrameId.RidleyWingLeft1 },
        { 2, EnemyFrameId.RidleyWingLeft2 },
        { 3, EnemyFrameId.RidleyWingLeft3 },
        { 4, EnemyFrameId.RidleyWingLeft4 },
        { 5, EnemyFrameId.RidleyWingLeft5 },
        { 6, EnemyFrameId.RidleyWingLeft4 },
        { 7, EnemyFrameId.RidleyWingLeft3 },
        { 8, EnemyFrameId.RidleyWingLeft2 },
        { 9, EnemyFrameId.RidleyWingLeft1 },
        { 10, EnemyFrameId.RidleyWingRight0 },
        { 11, EnemyFrameId.RidleyWingRight1 },
        { 12, EnemyFrameId.RidleyWingRight2 },
        { 13, EnemyFrameId.RidleyWingRight3 },
        { 14, EnemyFrameId.RidleyWingRight4 },
        { 15, EnemyFrameId.RidleyWingRight5 },
        { 16, EnemyFrameId.RidleyWingRight4 },
        { 17, EnemyFrameId.RidleyWingRight3 },
        { 18, EnemyFrameId.RidleyWingRight2 },
        { 19, EnemyFrameId.RidleyWingRight1 },
    };

    /// <summary>The installed frame each sixteenth-turn tail-tip sector shows.</summary>
    internal static EnemyFrameTable<int> TailTipFrames { get; } = new()
    {
        { 0, EnemyFrameId.RidleyTailTip0 },
        { 1, EnemyFrameId.RidleyTailTip1 },
        { 2, EnemyFrameId.RidleyTailTip2 },
        { 3, EnemyFrameId.RidleyTailTip3 },
        { 4, EnemyFrameId.RidleyTailTip4 },
        { 5, EnemyFrameId.RidleyTailTip5 },
        { 6, EnemyFrameId.RidleyTailTip6 },
        { 7, EnemyFrameId.RidleyTailTip7 },
        { 8, EnemyFrameId.RidleyTailTip8 },
        { 9, EnemyFrameId.RidleyTailTip9 },
        { 10, EnemyFrameId.RidleyTailTip10 },
        { 11, EnemyFrameId.RidleyTailTip11 },
        { 12, EnemyFrameId.RidleyTailTip12 },
        { 13, EnemyFrameId.RidleyTailTip13 },
        { 14, EnemyFrameId.RidleyTailTip14 },
        { 15, EnemyFrameId.RidleyTailTip15 },
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
