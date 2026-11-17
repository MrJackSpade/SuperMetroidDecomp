using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The 32 Draygon poses drawn only on the background layer behind the sprites, each as one unshifted tilemap picture; its
/// other 48 poses are sprite compositions among the extended frames. Hitboxes and animation timing are not part of this
/// visual catalog.
/// </summary>
internal static class DraygonBg2FrameDefinitions
{
    /// <summary>Schema revision accepted by the installed Draygon BG2 frame document; revision 3 names each frame for its pose (#1465), 4 for the animation that first shows it (#1623), 5 for what its picture shows (#2069), 6 without the shut jaw's second frame (#2268).</summary>
    internal const int Version = 6;
    /// <summary>Asset document containing the 32 body frames with BG2 tilemap streams.</summary>
    internal const string FileName = "draygon-bg2-frames.json";
    /// <summary>Each selected Draygon BG2 frame has one zero-offset stream.</summary>
    internal const int MaximumComponents = 1;

    /// <summary>The 32 BG2 frames, left-facing then right-facing.</summary>
    internal static ImmutableArray<EnemyFrameName> Frames { get; } =
    [
        new(EnemyFrameId.DraygonLimpArmFacingLeftClawRaised, "draygon_limp_arm_facing_left_claw_raised"),
        new(EnemyFrameId.DraygonLimpArmFacingLeftClawDropping, "draygon_limp_arm_facing_left_claw_dropping"),
        new(EnemyFrameId.DraygonLimpArmFacingLeftClawDropped, "draygon_limp_arm_facing_left_claw_dropped"),
        new(EnemyFrameId.DraygonLimpArmFacingLeftClawCurled, "draygon_limp_arm_facing_left_claw_curled"),
        new(EnemyFrameId.DraygonBodyFacingLeftJawShut, "draygon_body_facing_left_jaw_shut"),
        new(EnemyFrameId.DraygonBodyFacingLeftJawParting, "draygon_body_facing_left_jaw_parting"),
        new(EnemyFrameId.DraygonBodyFacingLeftJawOpen, "draygon_body_facing_left_jaw_open"),
        new(EnemyFrameId.DraygonBodyFacingLeftJawWide, "draygon_body_facing_left_jaw_wide"),
        new(EnemyFrameId.DraygonEyeFacingLeftShut, "draygon_eye_facing_left_shut"),
        new(EnemyFrameId.DraygonEyeFacingLeftOpening, "draygon_eye_facing_left_opening"),
        new(EnemyFrameId.DraygonEyeFacingLeftHalfOpen, "draygon_eye_facing_left_half_open"),
        new(EnemyFrameId.DraygonEyeFacingLeftWide, "draygon_eye_facing_left_wide"),
        new(EnemyFrameId.DraygonEyeFacingLeftOpen, "draygon_eye_facing_left_open"),
        new(EnemyFrameId.DraygonEyeFacingLeftBloodshot, "draygon_eye_facing_left_bloodshot"),
        new(EnemyFrameId.DraygonEyeFacingLeftNarrowing, "draygon_eye_facing_left_narrowing"),
        new(EnemyFrameId.DraygonEyeFacingLeftClosing, "draygon_eye_facing_left_closing"),
        new(EnemyFrameId.DraygonLimpArmFacingRightClawRaised, "draygon_limp_arm_facing_right_claw_raised"),
        new(EnemyFrameId.DraygonLimpArmFacingRightClawDropping, "draygon_limp_arm_facing_right_claw_dropping"),
        new(EnemyFrameId.DraygonLimpArmFacingRightClawDropped, "draygon_limp_arm_facing_right_claw_dropped"),
        new(EnemyFrameId.DraygonLimpArmFacingRightClawCurled, "draygon_limp_arm_facing_right_claw_curled"),
        new(EnemyFrameId.DraygonBodyFacingRightJawShut, "draygon_body_facing_right_jaw_shut"),
        new(EnemyFrameId.DraygonBodyFacingRightJawParting, "draygon_body_facing_right_jaw_parting"),
        new(EnemyFrameId.DraygonBodyFacingRightJawOpen, "draygon_body_facing_right_jaw_open"),
        new(EnemyFrameId.DraygonBodyFacingRightJawWide, "draygon_body_facing_right_jaw_wide"),
        new(EnemyFrameId.DraygonEyeFacingRightShut, "draygon_eye_facing_right_shut"),
        new(EnemyFrameId.DraygonEyeFacingRightOpening, "draygon_eye_facing_right_opening"),
        new(EnemyFrameId.DraygonEyeFacingRightHalfOpen, "draygon_eye_facing_right_half_open"),
        new(EnemyFrameId.DraygonEyeFacingRightWide, "draygon_eye_facing_right_wide"),
        new(EnemyFrameId.DraygonEyeFacingRightOpen, "draygon_eye_facing_right_open"),
        new(EnemyFrameId.DraygonEyeFacingRightBloodshot, "draygon_eye_facing_right_bloodshot"),
        new(EnemyFrameId.DraygonEyeFacingRightNarrowing, "draygon_eye_facing_right_narrowing"),
        new(EnemyFrameId.DraygonEyeFacingRightClosing, "draygon_eye_facing_right_closing"),
    ];
}
