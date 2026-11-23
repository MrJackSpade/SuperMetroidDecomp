using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Named background-layer pictures of the 42 Crocomire fight-body frames drawn
/// partly as sprites and partly on the background layer. Their sprite components
/// are authored among the extended frames; hitboxes and animation timing remain
/// gameplay data.
/// </summary>
internal static partial class CrocomireBg2FrameDefinitions
{
    /// <summary>Schema version for the selected mixed-frame BG2 artwork file; version 3 names each frame for its pose (#1465), 4 for the animation that first shows it (#1623), 6 for what its picture shows (#2069), 7 for the mouth its striding bodies show (#2268).</summary>
    internal const int Version = 7;
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
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthShut, "crocomire_body_far_foot_forward_arm_upright_mouth_shut"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthParted, "crocomire_body_far_foot_forward_arm_upright_mouth_parted"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthGaping, "crocomire_body_far_foot_forward_arm_upright_mouth_gaping"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmBentMouthShut, "crocomire_body_near_foot_forward_arm_bent_mouth_shut"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmSweepingMouthGaping, "crocomire_body_near_foot_forward_arm_sweeping_mouth_gaping"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmLiftingMouthGaping, "crocomire_body_near_foot_forward_arm_lifting_mouth_gaping"),
        new(EnemyFrameId.CrocomireBodyNearFootForwardArmUprightMouthGaping, "crocomire_body_near_foot_forward_arm_upright_mouth_gaping"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmBentMouthGaping, "crocomire_body_far_foot_forward_arm_bent_mouth_gaping"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmSquareMouthGaping, "crocomire_body_far_foot_forward_arm_square_mouth_gaping"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmReachingMouthGaping, "crocomire_body_far_foot_forward_arm_reaching_mouth_gaping"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmSweepingMouthGaping, "crocomire_body_far_foot_forward_arm_sweeping_mouth_gaping"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmLiftingMouthParted, "crocomire_body_far_foot_forward_arm_lifting_mouth_parted"),
        new(EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthShutting, "crocomire_body_far_foot_forward_arm_upright_mouth_shutting"),
        new(EnemyFrameId.CrocomireBodyLungingArmBent, "crocomire_body_lunging_arm_bent"),
        new(EnemyFrameId.CrocomireBodyLungingArmRaised, "crocomire_body_lunging_arm_raised"),
        new(EnemyFrameId.CrocomireBodyLungingArmOutstretched, "crocomire_body_lunging_arm_outstretched"),
        new(EnemyFrameId.CrocomireBodyLungingArmLowering, "crocomire_body_lunging_arm_lowering"),
        new(EnemyFrameId.CrocomireBodyLungingArmReturning, "crocomire_body_lunging_arm_returning"),
    ];
}
