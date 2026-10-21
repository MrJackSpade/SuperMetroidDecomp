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
        { 0, EnemyFrameId.RidleySupplementA6Dd4a },
        { 1, EnemyFrameId.RidleySupplementA6Dd6a },
        { 2, EnemyFrameId.RidleySupplementA6Dd85 },
        { 3, EnemyFrameId.RidleySupplementA6Dd96 },
        { 4, EnemyFrameId.RidleySupplementA6Dda7 },
        { 5, EnemyFrameId.RidleySupplementA6Ddc2 },
        { 6, EnemyFrameId.RidleySupplementA6Dda7 },
        { 7, EnemyFrameId.RidleySupplementA6Dd96 },
        { 8, EnemyFrameId.RidleySupplementA6Dd85 },
        { 9, EnemyFrameId.RidleySupplementA6Dd6a },
        { 10, EnemyFrameId.RidleySupplementA6Dde2 },
        { 11, EnemyFrameId.RidleySupplementA6De02 },
        { 12, EnemyFrameId.RidleySupplementA6De1d },
        { 13, EnemyFrameId.RidleySupplementA6De2e },
        { 14, EnemyFrameId.RidleySupplementA6De3f },
        { 15, EnemyFrameId.RidleySupplementA6De5a },
        { 16, EnemyFrameId.RidleySupplementA6De3f },
        { 17, EnemyFrameId.RidleySupplementA6De2e },
        { 18, EnemyFrameId.RidleySupplementA6De1d },
        { 19, EnemyFrameId.RidleySupplementA6De02 },
    };

    /// <summary>The installed frame each sixteenth-turn tail-tip sector shows.</summary>
    internal static EnemyFrameTable<int> TailTipFrames { get; } = new()
    {
        { 0, EnemyFrameId.RidleySupplementA6Dd2e },
        { 1, EnemyFrameId.RidleySupplementA6Dd27 },
        { 2, EnemyFrameId.RidleySupplementA6Dd20 },
        { 3, EnemyFrameId.RidleySupplementA6Dd19 },
        { 4, EnemyFrameId.RidleySupplementA6Dd12 },
        { 5, EnemyFrameId.RidleySupplementA6Dd0b },
        { 6, EnemyFrameId.RidleySupplementA6Dd04 },
        { 7, EnemyFrameId.RidleySupplementA6Dcfd },
        { 8, EnemyFrameId.RidleySupplementA6Dcf6 },
        { 9, EnemyFrameId.RidleySupplementA6Dcef },
        { 10, EnemyFrameId.RidleySupplementA6Dce8 },
        { 11, EnemyFrameId.RidleySupplementA6Dce1 },
        { 12, EnemyFrameId.RidleySupplementA6Dcda },
        { 13, EnemyFrameId.RidleySupplementA6Dd43 },
        { 14, EnemyFrameId.RidleySupplementA6Dd3c },
        { 15, EnemyFrameId.RidleySupplementA6Dd35 },
    };

    /// <summary>The installed frame each of the six drawn tail links shows, large to small.</summary>
    internal static EnemyFrameTable<int> SegmentFrames { get; } = new()
    {
        { 0, EnemyFrameId.RidleySupplementA6Dc90 },
        { 1, EnemyFrameId.RidleySupplementA6Dc90 },
        { 2, EnemyFrameId.RidleySupplementA6Dc97 },
        { 3, EnemyFrameId.RidleySupplementA6Dc97 },
        { 4, EnemyFrameId.RidleySupplementA6Dc9e },
        { 5, EnemyFrameId.RidleySupplementA6Dc9e },
    };
}
