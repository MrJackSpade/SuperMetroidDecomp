using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Named background-layer pictures of the 42 Crocomire fight-body frames drawn
/// partly as sprites and partly on the background layer. Their sprite components
/// are authored among the extended frames; hitboxes and animation timing remain
/// gameplay data.
/// </summary>
internal static class CrocomireBg2FrameDefinitions
{
    /// <summary>Schema version for the selected mixed-frame BG2 artwork file; version 3 names each frame for its pose (#1465), 4 for the animation that first shows it (#1623), 6 for what its picture shows (#2069).</summary>
    internal const int Version = 6;
    /// <summary>Installed BG2 visual resource for Crocomire's mixed fight frames.</summary>
    internal const string FileName = "crocomire-bg2-frames.json";

    /// <summary>The 42 body frames that carry BG2 components, in the compiled frame order.</summary>
    internal static ImmutableArray<EnemyFrameName> Frames { get; } =
    [
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmBent, "crocomire_body_near_foot_forward_arm_bent"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmSquare, "crocomire_body_near_foot_forward_arm_square"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmReaching, "crocomire_body_near_foot_forward_arm_reaching"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmSweeping, "crocomire_body_near_foot_forward_arm_sweeping"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmLifting, "crocomire_body_near_foot_forward_arm_lifting"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmUpright, "crocomire_body_near_foot_forward_arm_upright"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmBent, "crocomire_body_far_foot_forward_arm_bent"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmSquare, "crocomire_body_far_foot_forward_arm_square"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmReaching, "crocomire_body_far_foot_forward_arm_reaching"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmSweeping, "crocomire_body_far_foot_forward_arm_sweeping"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmLifting, "crocomire_body_far_foot_forward_arm_lifting"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmUpright, "crocomire_body_far_foot_forward_arm_upright"),
        new(EnemyFrameId.CrocomireBodyArmUprightMouthOpen, "crocomire_body_arm_upright_mouth_open"),
        new(EnemyFrameId.CrocomireBodyArmUprightMouthOpenLess, "crocomire_body_arm_upright_mouth_open_less"),
        new(EnemyFrameId.CrocomireBodyArmUprightMouthClosing, "crocomire_body_arm_upright_mouth_closing"),
        new(EnemyFrameId.CrocomireBodyArmUprightMouthHalfClosed, "crocomire_body_arm_upright_mouth_half_closed"),
        new(EnemyFrameId.CrocomireBodyArmUprightMouthMostlyClosed, "crocomire_body_arm_upright_mouth_mostly_closed"),
        new(EnemyFrameId.CrocomireBodyArmUprightMouthNearlyShut, "crocomire_body_arm_upright_mouth_nearly_shut"),
        new(EnemyFrameId.CrocomireBodyArmUprightMouthShut, "crocomire_body_arm_upright_mouth_shut"),
        new(EnemyFrameId.CrocomireBodySwingingArmBent, "crocomire_body_swinging_arm_bent"),
        new(EnemyFrameId.CrocomireBodySwingingArmRaised, "crocomire_body_swinging_arm_raised"),
        new(EnemyFrameId.CrocomireBodySwingingArmOutstretched, "crocomire_body_swinging_arm_outstretched"),
        new(EnemyFrameId.CrocomireBodySwingingArmLowering, "crocomire_body_swinging_arm_lowering"),
        new(EnemyFrameId.CrocomireBodySwingingArmReturning, "crocomire_body_swinging_arm_returning"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmUprightTwinB, "crocomire_body_far_foot_forward_arm_upright_twin_b"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmUprightTwinC, "crocomire_body_far_foot_forward_arm_upright_twin_c"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmUprightTwinD, "crocomire_body_far_foot_forward_arm_upright_twin_d"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmBentTwin, "crocomire_body_near_foot_forward_arm_bent_twin"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmSweepingTwin, "crocomire_body_near_foot_forward_arm_sweeping_twin"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmLiftingTwin, "crocomire_body_near_foot_forward_arm_lifting_twin"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmUprightTwin, "crocomire_body_near_foot_forward_arm_upright_twin"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmBentTwin, "crocomire_body_far_foot_forward_arm_bent_twin"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmSquareTwin, "crocomire_body_far_foot_forward_arm_square_twin"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmReachingTwin, "crocomire_body_far_foot_forward_arm_reaching_twin"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmSweepingTwin, "crocomire_body_far_foot_forward_arm_sweeping_twin"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmLiftingTwin, "crocomire_body_far_foot_forward_arm_lifting_twin"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmUprightTwin, "crocomire_body_far_foot_forward_arm_upright_twin"),
        new(EnemyFrameId.CrocomireBodyLungingArmBent, "crocomire_body_lunging_arm_bent"),
        new(EnemyFrameId.CrocomireBodyLungingArmRaised, "crocomire_body_lunging_arm_raised"),
        new(EnemyFrameId.CrocomireBodyLungingArmOutstretched, "crocomire_body_lunging_arm_outstretched"),
        new(EnemyFrameId.CrocomireBodyLungingArmLowering, "crocomire_body_lunging_arm_lowering"),
        new(EnemyFrameId.CrocomireBodyLungingArmReturning, "crocomire_body_lunging_arm_returning"),
    ];
}
