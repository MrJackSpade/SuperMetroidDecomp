using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Fixed walking/wall/ninja-Pirate collision data, separate from editable sprite art.</summary>
/// <remarks>
/// Each of the Pirates' frames lists its own hitboxes, placed from the Pirate's origin and drawn per frame: no frame's
/// hitboxes mirror another's facing, and frames that show the same hitboxes name one shared list (#2557). A Pirate's touch and shots
/// take the reaction of the first hitbox they overlap, in each frame's listed order.
/// </remarks>
internal static class SpacePirateCollisionDefinitions
{
    /// <summary>How long a Pirate that reflects a shot stays invincible, in updates.</summary>
    internal const int ReflectInvincibility = 10;

    /// <summary>A Pirate's touch: Samus takes contact damage.</summary>
    private static readonly EnemyReaction Touch = SpacePirateReactions.Touch;
    /// <summary>A shot that hurts a walking or wall Pirate, or a silver ninja.</summary>
    private static readonly EnemyReaction Shot = SpacePirateReactions.Shot;
    /// <summary>A shot at a gold ninja where it can be hurt.</summary>
    private static readonly EnemyReaction GoldVulnerableShot = SpacePirateReactions.GoldNinjaVulnerableHitboxShot;
    /// <summary>A shot at a gold ninja where it cannot be hurt.</summary>
    private static readonly EnemyReaction GoldInvincibleShot = SpacePirateReactions.GoldNinjaInvincibleHitboxShot;

    /// <summary>The hitboxes a Pirate facing left shows while a leg passes the other: head and legs.</summary>
    private static readonly EnemyCollisionHitbox[] LeftLegPassingHitboxes =
    [
        new(-12, -16, 1, 3, Touch, Shot), new(-7, 0, 6, 30, Touch, Shot),
    ];

    /// <summary>The hitboxes a walking Pirate facing left shows while planting its far leg or swinging its near leg back.</summary>
    private static readonly EnemyCollisionHitbox[] LeftLegPlantingHitboxes =
    [
        new(-12, -16, 1, 3, Touch, GoldInvincibleShot), new(-5, 0, 8, 30, Touch, Shot),
    ];

    /// <summary>The hitboxes a Pirate facing left shows while lifting its near leg.</summary>
    private static readonly EnemyCollisionHitbox[] LeftLegLiftingHitboxes =
    [
        new(-12, -16, 1, 3, Touch, Shot), new(-5, 0, 8, 30, Touch, Shot),
    ];

    /// <summary>The hitboxes a ninja shows crouched or with its claws low, in either facing.</summary>
    private static readonly EnemyCollisionHitbox[] CrouchHitboxes =
    [
        new(-7, -14, 6, 5, Touch, GoldInvincibleShot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot),
    ];

    /// <summary>The hitboxes a ninja shows with a claw raised high, in either facing.</summary>
    private static readonly EnemyCollisionHitbox[] ClawRaisedHighHitboxes =
    [
        new(-7, -15, 6, 4, Touch, GoldInvincibleShot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot),
    ];

    /// <summary>The hitboxes a ninja shows with a claw overhead or standing, in either facing.</summary>
    private static readonly EnemyCollisionHitbox[] ClawOverheadHitboxes =
    [
        new(-7, -16, 6, 3, Touch, GoldInvincibleShot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot),
    ];

    /// <summary>The hitboxes a ninja shows upright with its claws lowered, in either facing.</summary>
    private static readonly EnemyCollisionHitbox[] UprightClawsLoweredHitboxes =
    [
        new(-7, -12, 6, 7, Touch, GoldInvincibleShot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot),
    ];

    /// <summary>The hitboxes a Pirate facing right shows through most of its stride.</summary>
    private static readonly EnemyCollisionHitbox[] RightStrideHitboxes =
    [
        new(-2, -16, 11, 3, Touch, GoldVulnerableShot), new(-7, 0, 6, 30, Touch, Shot),
    ];

    /// <summary>The hitboxes a Pirate facing right shows while its near leg passes the other.</summary>
    private static readonly EnemyCollisionHitbox[] RightLegPassingHitboxes =
    [
        new(-2, -16, 11, 3, Touch, GoldVulnerableShot), new(-6, 0, 7, 30, Touch, Shot),
    ];

    /// <summary>The hitboxes a walking Pirate shows standing or with its claws raised, in either facing.</summary>
    private static readonly EnemyCollisionHitbox[] ClawsUpHitboxes =
    [
        new(-7, -16, 6, 3, Touch, Shot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot),
    ];

    /// <summary>The hitboxes a ninja shows crouched to leap, in either facing.</summary>
    private static readonly EnemyCollisionHitbox[] LeapCrouchHitboxes =
    [
        new(-7, -14, 6, 5, Touch, Shot), new(-7, 3, 6, 33, Touch, Shot),
    ];

    /// <summary>The walking, wall and ninja frames' hitboxes, and the common empty frame's point hitbox.</summary>
    internal static EnemyFrameCollision<EnemyCollisionHitbox> Frames { get; } = new()
    {
        { EnemyFrameId.Empty, [new(0, 0, 0, 0, EnemyReaction.OrdinaryTouch, EnemyReaction.OrdinaryShot)] },
        { EnemyFrameId.WallPirateOnLeftWallLegsHanging, [new(-17, -8, 0, 30, Touch, Shot), new(-18, -19, 6, 0, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnLeftWallKneesBending, [new(-16, -5, -1, 25, Touch, Shot), new(-18, -19, 6, 0, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnLeftWallKneesTucked, [new(-15, -6, 0, 23, Touch, Shot), new(-18, -19, 6, 0, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnLeftWallLegsPushing, [new(-18, -19, 6, 0, Touch, Shot), new(-15, 0, -1, 30, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnLeftWallLegsStretched, [new(-18, -19, 6, 0, Touch, Shot), new(-14, 0, 4, 30, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnLeftWallClawsTogether, [new(-15, -21, 14, 4, Touch, Shot), new(-17, -8, 0, 30, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnLeftWallClawsSpread, [new(-15, -21, 15, 1, Touch, Shot), new(-14, 0, 4, 30, Touch, Shot)] },
        { EnemyFrameId.WallPirateReachingForRightWall, [new(-10, -21, 19, 22, Touch, Shot)] },
        { EnemyFrameId.WallPirateCurledLeapingRight, [new(-8, -19, 18, 16, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnRightWallLegsHanging, [new(-1, 0, 15, 30, Touch, Shot), new(-9, -23, 17, 0, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnRightWallKneesBending, [new(0, 0, 15, 25, Touch, Shot), new(-9, -19, 16, 0, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnRightWallKneesTucked, [new(-2, 0, 15, 23, Touch, Shot), new(-9, -19, 17, 0, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnRightWallLegsPushing, [new(-9, -19, 16, 0, Touch, Shot), new(-2, 0, 15, 30, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnRightWallLegsStretched, [new(-9, -19, 17, 0, Touch, Shot), new(-7, 0, 15, 30, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnRightWallClawsTogether, [new(-15, -19, 15, 0, Touch, Shot), new(-7, 2, 15, 32, Touch, Shot)] },
        { EnemyFrameId.WallPirateOnRightWallClawsSpread, [new(-15, -19, 14, 3, Touch, Shot), new(-7, 2, 15, 32, Touch, Shot)] },
        { EnemyFrameId.WallPirateReachingForLeftWall, [new(-20, -19, 10, 25, Touch, Shot)] },
        { EnemyFrameId.WallPirateCurledLeapingLeft, [new(-20, -19, 6, 16, Touch, Shot)] },
        { EnemyFrameId.WalkingPirateWalkingLeftFarLegBack, [new(-12, -16, 1, 3, Touch, GoldVulnerableShot), new(-11, 0, 8, 30, Touch, Shot)] },
        { EnemyFrameId.WalkingPirateWalkingLeftFarLegLifting, [new(-12, -16, 1, 3, Touch, Shot), new(-11, 0, 8, 30, Touch, Shot)] },
        { EnemyFrameId.WalkingPirateWalkingLeftFarLegPassing, LeftLegPassingHitboxes },
        { EnemyFrameId.WalkingPirateWalkingLeftFarLegPlanting, LeftLegPlantingHitboxes },
        { EnemyFrameId.WalkingPirateWalkingLeftNearLegBack, LeftLegPlantingHitboxes },
        { EnemyFrameId.WalkingPirateWalkingLeftNearLegLifting, LeftLegLiftingHitboxes },
        { EnemyFrameId.WalkingPirateWalkingLeftNearLegPassing, LeftLegPassingHitboxes },
        { EnemyFrameId.WalkingPirateWalkingLeftNearLegPlanting, [new(-12, -16, 1, 3, Touch, GoldVulnerableShot), new(-7, 0, 6, 30, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateCrouchFacingLeft, CrouchHitboxes },
        { EnemyFrameId.NinjaPirateClawsReadyFacingLeft, CrouchHitboxes },
        { EnemyFrameId.NinjaPirateClawDrawnBackFacingLeft, CrouchHitboxes },
        { EnemyFrameId.NinjaPirateClawRaisedHighFacingLeft, ClawRaisedHighHitboxes },
        { EnemyFrameId.NinjaPirateClawRaisedOverheadFacingLeft, ClawOverheadHitboxes },
        { EnemyFrameId.NinjaPirateClawSwungForwardFacingLeft, CrouchHitboxes },
        { EnemyFrameId.NinjaPirateClawsSweptDownFacingLeft, [new(-7, -13, 6, 6, Touch, GoldInvincibleShot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateUprightClawsLoweredFacingLeft, UprightClawsLoweredHitboxes },
        { EnemyFrameId.WalkingPirateWalkingRightFarLegBack, RightStrideHitboxes },
        { EnemyFrameId.WalkingPirateWalkingRightFarLegLifting, RightStrideHitboxes },
        { EnemyFrameId.WalkingPirateWalkingRightFarLegPassing, RightStrideHitboxes },
        { EnemyFrameId.WalkingPirateWalkingRightFarLegPlanting, RightStrideHitboxes },
        { EnemyFrameId.WalkingPirateWalkingRightNearLegBack, [new(-2, -16, 11, 3, Touch, GoldVulnerableShot), new(-8, -1, 5, 30, Touch, Shot)] },
        { EnemyFrameId.WalkingPirateWalkingRightNearLegLifting, RightStrideHitboxes },
        { EnemyFrameId.WalkingPirateWalkingRightNearLegPassing, RightLegPassingHitboxes },
        { EnemyFrameId.WalkingPirateWalkingRightNearLegPlanting, RightLegPassingHitboxes },
        { EnemyFrameId.NinjaPirateCrouchFacingRight, CrouchHitboxes },
        { EnemyFrameId.NinjaPirateClawsReadyFacingRight, CrouchHitboxes },
        { EnemyFrameId.NinjaPirateClawDrawnBackFacingRight, CrouchHitboxes },
        { EnemyFrameId.NinjaPirateClawRaisedHighFacingRight, ClawRaisedHighHitboxes },
        { EnemyFrameId.NinjaPirateClawRaisedOverheadFacingRight, ClawOverheadHitboxes },
        { EnemyFrameId.NinjaPirateClawSwungForwardFacingRight, CrouchHitboxes },
        { EnemyFrameId.NinjaPirateClawsSweptDownFacingRight, [new(-7, -14, 6, 5, Touch, GoldVulnerableShot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateUprightClawsLoweredFacingRight, UprightClawsLoweredHitboxes },
        { EnemyFrameId.WalkingPirateStandingFacingLeft, ClawsUpHitboxes },
        { EnemyFrameId.WalkingPirateClawsLiftingFacingLeft, ClawsUpHitboxes },
        { EnemyFrameId.WalkingPirateClawsForwardFacingLeft, ClawsUpHitboxes },
        { EnemyFrameId.WalkingPirateClawsRaisedFacingLeft, ClawsUpHitboxes },
        { EnemyFrameId.WalkingPirateClawsRaisedLeaningFacingLeft, [new(-8, -15, 5, 4, Touch, Shot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.WalkingPirateFiringFacingLeft, [new(-9, -13, 4, 6, Touch, Shot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.WalkingPirateStandingFacingRight, ClawsUpHitboxes },
        { EnemyFrameId.WalkingPirateClawsLiftingFacingRight, ClawsUpHitboxes },
        { EnemyFrameId.WalkingPirateClawsForwardFacingRight, ClawsUpHitboxes },
        { EnemyFrameId.WalkingPirateClawsRaisedFacingRight, ClawsUpHitboxes },
        { EnemyFrameId.WalkingPirateClawsRaisedLeaningFacingRight, [new(-6, -15, 7, 4, Touch, Shot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.WalkingPirateFiringFacingRight, [new(-5, -13, 8, 6, Touch, Shot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.WalkingPirateLookingAtViewerFacingLeft, [new(-12, -31, 1, -12, Touch, Shot), new(-7, -16, 6, 3, Touch, Shot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.WalkingPirateLookingAwayFacingLeft, [new(-12, -30, 1, -11, Touch, Shot), new(-7, -16, 6, 3, Touch, Shot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.WalkingPirateLookingAtViewerFacingRight, [new(-2, -31, 11, -12, Touch, Shot), new(-7, -16, 6, 3, Touch, Shot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.WalkingPirateLookingAwayFacingRight, [new(-2, -30, 11, -11, Touch, Shot), new(-7, -16, 6, 3, Touch, Shot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.WalkingPirateFacingViewer, [new(-13, -18, 10, 31, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingLeftTurned0, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingLeftTurned45, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingLeftTurned90, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingLeftTurned135, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingLeftTurned180, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingLeftTurned225, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingLeftTurned270, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingLeftTurned315, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingRightTurned0, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingRightTurned45, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingRightTurned90, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingRightTurned135, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingRightTurned180, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingRightTurned225, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingRightTurned270, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateSpinningFacingRightTurned315, [new(-11, -13, 10, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateRunningLeftFarLegBack, [new(-12, -18, 1, 1, Touch, GoldVulnerableShot), new(-7, -2, 6, 28, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateRunningLeftFarLegLifting, [new(-12, -16, 1, 3, Touch, Shot), new(-8, 0, 5, 30, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateRunningLeftFarLegPassing, [new(-12, -15, 1, 4, Touch, Shot), new(-7, 0, 6, 30, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateRunningLeftFarLegPlanting, [new(-12, -17, 1, 2, Touch, GoldInvincibleShot), new(-8, 0, 5, 30, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateRunningLeftNearLegBack, [new(-12, -18, 1, 1, Touch, GoldInvincibleShot), new(-5, -2, 8, 28, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateRunningLeftNearLegLifting, LeftLegLiftingHitboxes },
        { EnemyFrameId.NinjaPirateRunningLeftNearLegPassing, LeftLegPassingHitboxes },
        { EnemyFrameId.NinjaPirateRunningLeftNearLegPlanting, [new(-12, -18, 1, 1, Touch, GoldVulnerableShot), new(-7, 0, 6, 30, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateRunningRightFarLegBack, [new(-2, -18, 11, 1, Touch, GoldVulnerableShot), new(-7, -2, 6, 28, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateRunningRightFarLegLifting, RightStrideHitboxes },
        { EnemyFrameId.NinjaPirateRunningRightFarLegPassing, [new(-2, -15, 11, 4, Touch, GoldVulnerableShot), new(-7, 0, 6, 30, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateRunningRightFarLegPlanting, [new(-2, -17, 11, 2, Touch, GoldVulnerableShot), new(-7, 0, 6, 30, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateRunningRightNearLegBack, [new(-2, -18, 11, 1, Touch, GoldVulnerableShot), new(-8, -1, 5, 29, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateRunningRightNearLegLifting, RightStrideHitboxes },
        { EnemyFrameId.NinjaPirateRunningRightNearLegPassing, RightLegPassingHitboxes },
        { EnemyFrameId.NinjaPirateRunningRightNearLegPlanting, [new(-2, -18, 11, 1, Touch, GoldVulnerableShot), new(-6, 0, 7, 30, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateReadyFacingLeftFeetClose, [new(-7, -16, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateReadyFacingLeftFeetApart, [new(-7, -16, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateReadyFacingLeftFeetWide, [new(-7, -16, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateReadyFacingRightFeetClose, [new(-7, -16, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateReadyFacingRightFeetApart, [new(-7, -16, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateReadyFacingRightFeetWide, [new(-7, -16, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.WalkingPirateFlinchLeft, [new(-7, -11, 6, 31, Touch, Shot), new(-18, -10, -7, 10, Touch, GoldVulnerableShot)] },
        { EnemyFrameId.NinjaPirateFlinchFacingLeft, [new(-7, -11, 6, 31, Touch, GoldInvincibleShot), new(-18, -10, -7, 10, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.WalkingPirateFlinchRight, [new(-7, -11, 6, 31, Touch, Shot), new(6, -11, 17, 9, Touch, GoldVulnerableShot)] },
        { EnemyFrameId.NinjaPirateFlinchFacingRight, [new(-7, -11, 6, 31, Touch, GoldInvincibleShot), new(6, -11, 17, 9, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateTuckedJumpFacingLeft, [new(-7, -19, 6, 16, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateDiveKickFacingLeft, [new(-7, -19, 6, 16, Touch, Shot), new(-33, 3, -7, 16, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateTuckedJumpFacingRight, [new(-7, -19, 6, 16, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateDiveKickFacingRight, [new(-7, -19, 6, 16, Touch, Shot), new(6, 3, 31, 16, Touch, Shot)] },
        { EnemyFrameId.NinjaPirateKickChamberedFacingLeft, [new(-2, -19, 11, 30, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateKickExtendedFacingLeft, [new(-12, -18, 1, 31, Touch, GoldInvincibleShot), new(-39, -4, -2, 31, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateKickChamberedFacingRight, [new(-12, -19, 1, 30, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateKickExtendedFacingRight, [new(-2, -18, 11, 31, Touch, GoldInvincibleShot), new(11, -5, 37, 31, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateLeapCrouchFacingLeft, LeapCrouchHitboxes },
        { EnemyFrameId.NinjaPirateLeapCrouchFacingRight, LeapCrouchHitboxes },
        { EnemyFrameId.NinjaPirateKickExtendingFacingLeft, [new(-5, -19, 8, 30, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateKickExtendingFacingRight, [new(-9, -19, 4, 30, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateLookingAtViewerFacingLeft, [new(-12, -31, 1, -12, Touch, GoldInvincibleShot), new(-7, -16, 6, 3, Touch, GoldInvincibleShot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateStandingFacingLeft, ClawOverheadHitboxes },
        { EnemyFrameId.NinjaPirateLookingAwayFacingLeft, [new(-12, -30, 1, -11, Touch, GoldInvincibleShot), new(-7, -16, 6, 3, Touch, GoldInvincibleShot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateLookingAtViewerFacingRight, [new(-2, -31, 11, -12, Touch, GoldInvincibleShot), new(-7, -16, 6, 3, Touch, GoldInvincibleShot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateStandingFacingRight, ClawOverheadHitboxes },
        { EnemyFrameId.NinjaPirateLookingAwayFacingRight, [new(-2, -30, 11, -11, Touch, GoldInvincibleShot), new(-7, -16, 6, 3, Touch, GoldInvincibleShot), new(-7, 3, 6, 33, Touch, GoldInvincibleShot)] },
        { EnemyFrameId.NinjaPirateFacingViewer, [new(-7, -18, 6, 31, Touch, GoldInvincibleShot)] },
    };
}
