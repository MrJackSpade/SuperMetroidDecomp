namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's typed body programs (#641), one per native bank-$AA list and per segment a goto, call,
/// link or resumed sleep enters, in authored order. Generated from the verified native lists; see
/// <see cref="TorizoAnimations"/>.
/// </summary>
internal enum TorizoBodyProgram
{
    /// <summary><c>InstList_Torizo_SpecialCallable_BlowUpBombTorizosGut</c>.</summary>
    SpecialCallableBlowUpBombTorizosGut,
    /// <summary><c>InstList_Torizo_Callable_BlowUpBombTorizosFace</c>.</summary>
    CallableBlowUpBombTorizosFace,
    /// <summary><c>InstList_Torizo_DeathSequence_0</c>.</summary>
    DeathSequence0,
    /// <summary><c>InstList_Torizo_DeathSequence_1</c>.</summary>
    DeathSequence1,
    /// <summary><c>InstList_Torizo_DeathSequence_2</c>.</summary>
    DeathSequence2,
    /// <summary><c>InstList_Torizo_BombTorizo_Initial_0</c>.</summary>
    BombTorizoInitial0,
    /// <summary>The code after the preceding list's sleep, which the AI resumes.</summary>
    BombTorizoInitial0Resumed,
    /// <summary><c>InstList_Torizo_BombTorizo_Initial_1</c>.</summary>
    BombTorizoInitial1,
    /// <summary><c>InstList_Torizo_BombTorizo_Initial_2</c>.</summary>
    BombTorizoInitial2,
    /// <summary><c>InstList_Torizo_FacingLeft_TurningLeft</c>.</summary>
    FacingLeftTurningLeft,
    /// <summary><c>InstList_Torizo_FacingLeft_Walking_RightLegMoving</c>.</summary>
    FacingLeftWalkingRightLegMoving,
    /// <summary><c>InstList_Torizo_FacingLeft_Walking_LeftLegMoving</c>.</summary>
    FacingLeftWalkingLeftLegMoving,
    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_0</c>.</summary>
    FacingLeftSpewingChozoOrbsRightFootFwd0,
    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_1</c>.</summary>
    FacingLeftSpewingChozoOrbsRightFootFwd1,
    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_0</c>.</summary>
    FacingLeftSpewingChozoOrbsLeftFootFwd0,
    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_1</c>.</summary>
    FacingLeftSpewingChozoOrbsLeftFootFwd1,
    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_0</c>.</summary>
    FacingLeftSonicBoomsRightFootForward0,
    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_1</c>.</summary>
    FacingLeftSonicBoomsRightFootForward1,
    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_0</c>.</summary>
    FacingLeftSonicBoomsLeftFootForward0,
    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_1</c>.</summary>
    FacingLeftSonicBoomsLeftFootForward1,
    /// <summary><c>InstList_Torizo_FacingLeft_ExplosiveSwipe_RightFootForward</c>.</summary>
    FacingLeftExplosiveSwipeRightFootForward,
    /// <summary><c>InstList_Torizo_FacingLeft_ExplosiveSwipe_LeftFootForward</c>.</summary>
    FacingLeftExplosiveSwipeLeftFootForward,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingForwards_0</c>.</summary>
    FacingLeftJumpingForwards0,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingForwards_1</c>.</summary>
    FacingLeftJumpingForwards1,
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_0</c>.</summary>
    FacingLeftFalling0,
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_1</c>.</summary>
    FacingLeftFalling1,
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_2</c>.</summary>
    FacingLeftFalling2,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0</c>.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd0,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_1</c>.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd1,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_2</c>.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd2,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_3</c>.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd3,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_4</c>.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd4,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0</c>.</summary>
    FacingLeftJumpingBackwardRightFootFwd0,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_1</c>.</summary>
    FacingLeftJumpingBackwardRightFootFwd1,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_2</c>.</summary>
    FacingLeftJumpingBackwardRightFootFwd2,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_3</c>.</summary>
    FacingLeftJumpingBackwardRightFootFwd3,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_4</c>.</summary>
    FacingLeftJumpingBackwardRightFootFwd4,
    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_TurningLeft</c>.</summary>
    FacingLeftFacelessTurningLeft,
    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_Walking_RightLegMoving</c>.</summary>
    FacingLeftFacelessWalkingRightLegMoving,
    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_Walking_LeftLegMoving</c>.</summary>
    FacingLeftFacelessWalkingLeftLegMoving,
    /// <summary>The unlabeled native list.</summary>
    FacingLeftFacelessWalkingLeftLegMovingTrailing,
    /// <summary>The code after the preceding list's sleep, which the AI resumes.</summary>
    FacingLeftFacelessWalkingLeftLegMovingTrailingResumed,
    /// <summary><c>InstList_Torizo_FacingRight_TurningRight</c>.</summary>
    FacingRightTurningRight,
    /// <summary><c>InstList_Torizo_FacingRight_Walking_LeftLegMoving</c>.</summary>
    FacingRightWalkingLeftLegMoving,
    /// <summary><c>InstList_Torizo_FacingRight_Walking_RightLegMoving</c>.</summary>
    FacingRightWalkingRightLegMoving,
    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_0</c>.</summary>
    FacingRightSpewingChozoOrbsLeftFootFwd0,
    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_1</c>.</summary>
    FacingRightSpewingChozoOrbsLeftFootFwd1,
    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_0</c>.</summary>
    FacingRightSpewingChozoOrbsRightFootFwd0,
    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_1</c>.</summary>
    FacingRightSpewingChozoOrbsRightFootFwd1,
    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_0</c>.</summary>
    FacingRightSonicBoomsLeftFootForward0,
    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_1</c>.</summary>
    FacingRightSonicBoomsLeftFootForward1,
    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_RightFootForward_0</c>.</summary>
    FacingRightSonicBoomsRightFootForward0,
    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_RightFootForward_1</c>.</summary>
    FacingRightSonicBoomsRightFootForward1,
    /// <summary><c>InstList_Torizo_FacingRight_ExplosiveSwipe_LeftFootForward</c>.</summary>
    FacingRightExplosiveSwipeLeftFootForward,
    /// <summary><c>InstList_Torizo_FacingRight_ExplosiveSwipe_RightFootForward</c>.</summary>
    FacingRightExplosiveSwipeRightFootForward,
    /// <summary><c>InstList_Torizo_FacingRight_JumpingForwards_0</c>.</summary>
    FacingRightJumpingForwards0,
    /// <summary><c>InstList_Torizo_FacingRight_JumpingForwards_1</c>.</summary>
    FacingRightJumpingForwards1,
    /// <summary><c>InstList_Torizo_FacingRight_Falling_0</c>.</summary>
    FacingRightFalling0,
    /// <summary><c>InstList_Torizo_FacingRight_Falling_1</c>.</summary>
    FacingRightFalling1,
    /// <summary><c>InstList_Torizo_FacingRight_Falling_2</c>.</summary>
    FacingRightFalling2,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0</c>.</summary>
    FacingRightJumpBackwardLandRightFootFwd0,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_1</c>.</summary>
    FacingRightJumpBackwardLandRightFootFwd1,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_2</c>.</summary>
    FacingRightJumpBackwardLandRightFootFwd2,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_3</c>.</summary>
    FacingRightJumpBackwardLandRightFootFwd3,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_4</c>.</summary>
    FacingRightJumpBackwardLandRightFootFwd4,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0</c>.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd0,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_1</c>.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd1,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_2</c>.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd2,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_3</c>.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd3,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_4</c>.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd4,
    /// <summary><c>InstList_Torizo_FacingRight_Faceless_TurningRight</c>.</summary>
    FacingRightFacelessTurningRight,
    /// <summary><c>InstList_Torizo_FacingRight_Faceless_Walking_LeftLegMoving</c>.</summary>
    FacingRightFacelessWalkingLeftLegMoving,
    /// <summary><c>InstList_Torizo_FacingRight_Faceless_Walking_RightLegMoving</c>.</summary>
    FacingRightFacelessWalkingRightLegMoving,
    /// <summary><c>InstList_GoldenTorizo_Initial_0</c>.</summary>
    GoldenInitial0,
    /// <summary>The code after the preceding list's sleep, which the AI resumes.</summary>
    GoldenInitial0Resumed,
    /// <summary><c>InstList_GoldenTorizo_Initial_1</c>.</summary>
    GoldenInitial1,
    /// <summary><c>InstList_GoldenTorizo_Initial_2</c>.</summary>
    GoldenInitial2,
    /// <summary><c>InstList_GoldenTorizo_Initial_3</c>.</summary>
    GoldenInitial3,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_0</c>.</summary>
    GoldenSpewChozoOrbsFaceLeftRightFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_1</c>.</summary>
    GoldenSpewChozoOrbsFaceLeftRightFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_0</c>.</summary>
    GoldenSpewChozoOrbsFacingLeftLeftFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_1</c>.</summary>
    GoldenSpewChozoOrbsFacingLeftLeftFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_0</c>.</summary>
    GoldenSonicBoomsFacingLeftRightFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_1</c>.</summary>
    GoldenSonicBoomsFacingLeftRightFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_0</c>.</summary>
    GoldenSonicBoomsFacingLeftLeftFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_1</c>.</summary>
    GoldenSonicBoomsFacingLeftLeftFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_0</c>.</summary>
    GoldenSpewChozoOrbFacingLeftLeftFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_1</c>.</summary>
    GoldenSpewChozoOrbFacingLeftLeftFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_0</c>.</summary>
    GoldenSpewChozoOrbFacingLeftRightFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_1</c>.</summary>
    GoldenSpewChozoOrbFacingLeftRightFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_0</c>.</summary>
    GoldenSonicBoomsFacingRightLeftFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_1</c>.</summary>
    GoldenSonicBoomsFacingRightLeftFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_0</c>.</summary>
    GoldenSonicBoomsFacingRightRightFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_1</c>.</summary>
    GoldenSonicBoomsFacingRightRightFootFwd1,
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingLeft_LeftFootFwd</c>.</summary>
    GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd,
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingLeft_RightFootFwd</c>.</summary>
    GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd,
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingRight_RightFootFwd</c>.</summary>
    GoldenLandedFromBackwardsJumpFacingRightRightFootFwd,
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingRight_LeftFootFwd</c>.</summary>
    GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd,
    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingLeft_LeftLegFwd</c>.</summary>
    GoldenCaughtSuperFacingLeftLeftLegFwd,
    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingLeft_RightLegFwd</c>.</summary>
    GoldenCaughtSuperFacingLeftRightLegFwd,
    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingRight_RightLegFwd</c>.</summary>
    GoldenCaughtSuperFacingRightRightLegFwd,
    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingRight_LeftLegFwd</c>.</summary>
    GoldenCaughtSuperFacingRightLeftLegFwd,
    /// <summary><c>InstList_GoldenTorizo_SitDownAttack_FacingLeft</c>.</summary>
    GoldenSitDownAttackFacingLeft,
    /// <summary><c>InstList_GoldenTorizo_SitDownAttack_FacingRight</c>.</summary>
    GoldenSitDownAttackFacingRight,
    /// <summary><c>InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_0</c>.</summary>
    GoldenReleaseGoldenTorizoEggs0,
    /// <summary><c>InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_1</c>.</summary>
    GoldenReleaseGoldenTorizoEggs1,
    /// <summary><c>InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_2</c>.</summary>
    GoldenReleaseGoldenTorizoEggs2,
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_0</c>.</summary>
    GoldenEyeBeamAttack0,
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_1</c>.</summary>
    GoldenEyeBeamAttack1,
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_2</c>.</summary>
    GoldenEyeBeamAttack2,
    /// <summary><c>InstList_Torizo_Stunned_0</c>.</summary>
    Stunned0,
    /// <summary><c>InstList_Torizo_Stunned_1</c>.</summary>
    Stunned1,
    /// <summary><c>InstList_GoldenTorizo_Dodge_TurningLeft</c>.</summary>
    GoldenDodgeTurningLeft,
    /// <summary><c>InstList_GoldenTorizo_TurningLeft</c>.</summary>
    GoldenTurningLeft,
    /// <summary><c>InstList_GoldenTorizo_WalkingLeft_RightLegMoving</c>.</summary>
    GoldenWalkingLeftRightLegMoving,
    /// <summary><c>InstList_GoldenTorizo_WalkingLeft_LeftLegMoving</c>.</summary>
    GoldenWalkingLeftLeftLegMoving,
    /// <summary><c>InstList_GoldenTorizo_Dodge_TurningRight</c>.</summary>
    GoldenDodgeTurningRight,
    /// <summary><c>InstList_GoldenTorizo_TurningRight</c>.</summary>
    GoldenTurningRight,
    /// <summary><c>InstList_GoldenTorizo_WalkingRight_LeftLegMoving</c>.</summary>
    GoldenWalkingRightLeftLegMoving,
    /// <summary><c>InstList_GoldenTorizo_WalkingRight_RightLegMoving</c>.</summary>
    GoldenWalkingRightRightLegMoving,
}
