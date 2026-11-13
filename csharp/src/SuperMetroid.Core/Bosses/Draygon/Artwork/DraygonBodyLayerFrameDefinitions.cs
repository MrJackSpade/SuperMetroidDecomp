using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>The 34 poses Draygon draws on his body layer behind the sprites (#1466), left-facing then right-facing; his other poses are sprite compositions.</summary>
internal static class DraygonBodyLayerFrameDefinitions
{
    /// <summary>The frames Draygon draws on his body layer, each a frame of his installed body sheet.</summary>
    internal static IReadOnlySet<EnemyFrameId> Frames { get; } = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.DraygonArmsFacingLeftDyingFirst,
        EnemyFrameId.DraygonArmsFacingLeftDyingSecond,
        EnemyFrameId.DraygonArmsFacingLeftDyingThird,
        EnemyFrameId.DraygonArmsFacingLeftDyingFourth,
        EnemyFrameId.DraygonBodyFacingLeftFireGoopFirst,
        EnemyFrameId.DraygonBodyFacingLeftFireGoopSecond,
        EnemyFrameId.DraygonBodyFacingLeftFireGoopThird,
        EnemyFrameId.DraygonBodyFacingLeftFireGoopFourth,
        EnemyFrameId.DraygonEyeFacingLeftIdleFirst,
        EnemyFrameId.DraygonEyeFacingLeftIdleSecond,
        EnemyFrameId.DraygonEyeFacingLeftIdleThird,
        EnemyFrameId.DraygonEyeFacingLeftDyingLoopFifth,
        EnemyFrameId.DraygonEyeFacingLeftIdleFourth,
        EnemyFrameId.DraygonEyeFacingLeftDyingLoopThird,
        EnemyFrameId.DraygonEyeFacingLeftIdleFifth,
        EnemyFrameId.DraygonEyeFacingLeftIdleSixth,
        EnemyFrameId.DraygonBodyFacingLeftReset,
        EnemyFrameId.DraygonArmsFacingRightDyingFirst,
        EnemyFrameId.DraygonArmsFacingRightDyingSecond,
        EnemyFrameId.DraygonArmsFacingRightDyingThird,
        EnemyFrameId.DraygonArmsFacingRightDyingFourth,
        EnemyFrameId.DraygonBodyFacingRightFireGoopFirst,
        EnemyFrameId.DraygonBodyFacingRightFireGoopSecond,
        EnemyFrameId.DraygonBodyFacingRightFireGoopThird,
        EnemyFrameId.DraygonBodyFacingRightFireGoopFourth,
        EnemyFrameId.DraygonEyeFacingRightIdleFirst,
        EnemyFrameId.DraygonEyeFacingRightIdleSecond,
        EnemyFrameId.DraygonEyeFacingRightIdleThird,
        EnemyFrameId.DraygonEyeFacingRightDyingLoopFifth,
        EnemyFrameId.DraygonEyeFacingRightIdleFourth,
        EnemyFrameId.DraygonEyeFacingRightDyingLoopThird,
        EnemyFrameId.DraygonEyeFacingRightIdleFifth,
        EnemyFrameId.DraygonEyeFacingRightIdleSixth,
        EnemyFrameId.DraygonBodyFacingRightReset,
    };
}
