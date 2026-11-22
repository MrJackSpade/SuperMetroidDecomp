using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>The poses Draygon draws on his body layer behind the sprites (#1466), left-facing then right-facing; his other poses are sprite compositions.</summary>
internal static partial class DraygonBodyLayerFrameDefinitions
{
    /// <summary>The frames Draygon draws on his body layer, each a frame of his installed body sheet.</summary>
    internal static IReadOnlySet<EnemyFrameId> Frames { get; } = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.DraygonLimpArmFacingLeftClawRaised,
        EnemyFrameId.DraygonLimpArmFacingLeftClawDropping,
        EnemyFrameId.DraygonLimpArmFacingLeftClawDropped,
        EnemyFrameId.DraygonLimpArmFacingLeftClawCurled,
        EnemyFrameId.DraygonBodyFacingLeftJawShut,
        EnemyFrameId.DraygonBodyFacingLeftJawParting,
        EnemyFrameId.DraygonBodyFacingLeftJawOpen,
        EnemyFrameId.DraygonBodyFacingLeftJawWide,
        EnemyFrameId.DraygonEyeFacingLeftShut,
        EnemyFrameId.DraygonEyeFacingLeftOpening,
        EnemyFrameId.DraygonEyeFacingLeftHalfOpen,
        EnemyFrameId.DraygonEyeFacingLeftWide,
        EnemyFrameId.DraygonEyeFacingLeftOpen,
        EnemyFrameId.DraygonEyeFacingLeftBloodshot,
        EnemyFrameId.DraygonEyeFacingLeftNarrowing,
        EnemyFrameId.DraygonEyeFacingLeftClosing,
        EnemyFrameId.DraygonLimpArmFacingRightClawRaised,
        EnemyFrameId.DraygonLimpArmFacingRightClawDropping,
        EnemyFrameId.DraygonLimpArmFacingRightClawDropped,
        EnemyFrameId.DraygonLimpArmFacingRightClawCurled,
        EnemyFrameId.DraygonBodyFacingRightJawShut,
        EnemyFrameId.DraygonBodyFacingRightJawParting,
        EnemyFrameId.DraygonBodyFacingRightJawOpen,
        EnemyFrameId.DraygonBodyFacingRightJawWide,
        EnemyFrameId.DraygonEyeFacingRightShut,
        EnemyFrameId.DraygonEyeFacingRightOpening,
        EnemyFrameId.DraygonEyeFacingRightHalfOpen,
        EnemyFrameId.DraygonEyeFacingRightWide,
        EnemyFrameId.DraygonEyeFacingRightOpen,
        EnemyFrameId.DraygonEyeFacingRightBloodshot,
        EnemyFrameId.DraygonEyeFacingRightNarrowing,
        EnemyFrameId.DraygonEyeFacingRightClosing,
    };
}
