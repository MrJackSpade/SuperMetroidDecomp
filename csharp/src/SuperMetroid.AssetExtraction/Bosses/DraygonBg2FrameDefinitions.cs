using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The 34 Draygon poses drawn only on the background layer behind the sprites, each as one unshifted tilemap picture; its
/// other 48 poses are sprite compositions among the extended frames. Hitboxes and animation timing are not part of this
/// visual catalog.
/// </summary>
internal static class DraygonBg2FrameDefinitions
{
    /// <summary>Schema revision accepted by the installed Draygon BG2 frame document; revision 3 names each frame for its pose (#1465), 4 for the animation that first shows it (#1623).</summary>
    internal const int Version = 4;
    /// <summary>Asset document containing the 34 body frames with BG2 tilemap streams.</summary>
    internal const string FileName = "draygon-bg2-frames.json";
    /// <summary>Each selected Draygon BG2 frame has one zero-offset stream.</summary>
    internal const int MaximumComponents = 1;

    /// <summary>The 34 BG2 frames, left-facing then right-facing.</summary>
    internal static ImmutableArray<EnemyFrameName> Frames { get; } =
    [
        new(EnemyFrameId.DraygonArmsFacingLeftDyingFirst, "draygon_arms_facing_left_dying_first"),
        new(EnemyFrameId.DraygonArmsFacingLeftDyingSecond, "draygon_arms_facing_left_dying_second"),
        new(EnemyFrameId.DraygonArmsFacingLeftDyingThird, "draygon_arms_facing_left_dying_third"),
        new(EnemyFrameId.DraygonArmsFacingLeftDyingFourth, "draygon_arms_facing_left_dying_fourth"),
        new(EnemyFrameId.DraygonBodyFacingLeftFireGoopFirst, "draygon_body_facing_left_fire_goop_first"),
        new(EnemyFrameId.DraygonBodyFacingLeftFireGoopSecond, "draygon_body_facing_left_fire_goop_second"),
        new(EnemyFrameId.DraygonBodyFacingLeftFireGoopThird, "draygon_body_facing_left_fire_goop_third"),
        new(EnemyFrameId.DraygonBodyFacingLeftFireGoopFourth, "draygon_body_facing_left_fire_goop_fourth"),
        new(EnemyFrameId.DraygonEyeFacingLeftIdleFirst, "draygon_eye_facing_left_idle_first"),
        new(EnemyFrameId.DraygonEyeFacingLeftIdleSecond, "draygon_eye_facing_left_idle_second"),
        new(EnemyFrameId.DraygonEyeFacingLeftIdleThird, "draygon_eye_facing_left_idle_third"),
        new(EnemyFrameId.DraygonEyeFacingLeftDyingLoopFifth, "draygon_eye_facing_left_dying_loop_fifth"),
        new(EnemyFrameId.DraygonEyeFacingLeftIdleFourth, "draygon_eye_facing_left_idle_fourth"),
        new(EnemyFrameId.DraygonEyeFacingLeftDyingLoopThird, "draygon_eye_facing_left_dying_loop_third"),
        new(EnemyFrameId.DraygonEyeFacingLeftIdleFifth, "draygon_eye_facing_left_idle_fifth"),
        new(EnemyFrameId.DraygonEyeFacingLeftIdleSixth, "draygon_eye_facing_left_idle_sixth"),
        new(EnemyFrameId.DraygonBodyFacingLeftReset, "draygon_body_facing_left_reset"),
        new(EnemyFrameId.DraygonArmsFacingRightDyingFirst, "draygon_arms_facing_right_dying_first"),
        new(EnemyFrameId.DraygonArmsFacingRightDyingSecond, "draygon_arms_facing_right_dying_second"),
        new(EnemyFrameId.DraygonArmsFacingRightDyingThird, "draygon_arms_facing_right_dying_third"),
        new(EnemyFrameId.DraygonArmsFacingRightDyingFourth, "draygon_arms_facing_right_dying_fourth"),
        new(EnemyFrameId.DraygonBodyFacingRightFireGoopFirst, "draygon_body_facing_right_fire_goop_first"),
        new(EnemyFrameId.DraygonBodyFacingRightFireGoopSecond, "draygon_body_facing_right_fire_goop_second"),
        new(EnemyFrameId.DraygonBodyFacingRightFireGoopThird, "draygon_body_facing_right_fire_goop_third"),
        new(EnemyFrameId.DraygonBodyFacingRightFireGoopFourth, "draygon_body_facing_right_fire_goop_fourth"),
        new(EnemyFrameId.DraygonEyeFacingRightIdleFirst, "draygon_eye_facing_right_idle_first"),
        new(EnemyFrameId.DraygonEyeFacingRightIdleSecond, "draygon_eye_facing_right_idle_second"),
        new(EnemyFrameId.DraygonEyeFacingRightIdleThird, "draygon_eye_facing_right_idle_third"),
        new(EnemyFrameId.DraygonEyeFacingRightDyingLoopFifth, "draygon_eye_facing_right_dying_loop_fifth"),
        new(EnemyFrameId.DraygonEyeFacingRightIdleFourth, "draygon_eye_facing_right_idle_fourth"),
        new(EnemyFrameId.DraygonEyeFacingRightDyingLoopThird, "draygon_eye_facing_right_dying_loop_third"),
        new(EnemyFrameId.DraygonEyeFacingRightIdleFifth, "draygon_eye_facing_right_idle_fifth"),
        new(EnemyFrameId.DraygonEyeFacingRightIdleSixth, "draygon_eye_facing_right_idle_sixth"),
        new(EnemyFrameId.DraygonBodyFacingRightReset, "draygon_body_facing_right_reset"),
    ];
}
