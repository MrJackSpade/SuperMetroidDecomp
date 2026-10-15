namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's typed body programs (#641), one per native bank-$AA list and per segment a goto, call,
/// link or resumed sleep enters, in authored order. Generated from the verified native lists; see
/// <see cref="TorizoAnimations"/>.
/// </summary>
internal enum TorizoBodyProgram
{
    /// <summary><c>InstList_Torizo_SpecialCallable_BlowUpBombTorizosGut</c> at $AA:B0E5.</summary>
    SpecialCallableBlowUpBombTorizosGut,
    /// <summary><c>InstList_Torizo_Callable_BlowUpBombTorizosFace</c> at $AA:B155.</summary>
    CallableBlowUpBombTorizosFace,
    /// <summary><c>InstList_Torizo_DeathSequence_0</c> at $AA:B1C8.</summary>
    DeathSequence0,
    /// <summary><c>InstList_Torizo_DeathSequence_1</c> at $AA:B1D2.</summary>
    DeathSequence1,
    /// <summary><c>InstList_Torizo_DeathSequence_2</c> at $AA:B1E2.</summary>
    DeathSequence2,
    /// <summary><c>InstList_Torizo_BombTorizo_Initial_0</c> at $AA:B879.</summary>
    BombTorizoInitial0,
    /// <summary>The code at $AA:B887 after the preceding list's sleep, which the AI resumes.</summary>
    BombTorizoInitial0Resumed,
    /// <summary><c>InstList_Torizo_BombTorizo_Initial_1</c> at $AA:B8C7.</summary>
    BombTorizoInitial1,
    /// <summary><c>InstList_Torizo_BombTorizo_Initial_2</c> at $AA:B935.</summary>
    BombTorizoInitial2,
    /// <summary><c>InstList_Torizo_FacingLeft_TurningLeft</c> at $AA:B962.</summary>
    FacingLeftTurningLeft,
    /// <summary><c>InstList_Torizo_FacingLeft_Walking_RightLegMoving</c> at $AA:B96C.</summary>
    FacingLeftWalkingRightLegMoving,
    /// <summary><c>InstList_Torizo_FacingLeft_Walking_LeftLegMoving</c> at $AA:B9B6.</summary>
    FacingLeftWalkingLeftLegMoving,
    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_0</c> at $AA:BA04.</summary>
    FacingLeftSpewingChozoOrbsRightFootFwd0,
    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_RightFootFwd_1</c> at $AA:BA26.</summary>
    FacingLeftSpewingChozoOrbsRightFootFwd1,
    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_0</c> at $AA:BA46.</summary>
    FacingLeftSpewingChozoOrbsLeftFootFwd0,
    /// <summary><c>InstList_Torizo_FacingLeft_SpewingChozoOrbs_LeftFootFwd_1</c> at $AA:BA68.</summary>
    FacingLeftSpewingChozoOrbsLeftFootFwd1,
    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_0</c> at $AA:BA88.</summary>
    FacingLeftSonicBoomsRightFootForward0,
    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_RightFootForward_1</c> at $AA:BA90.</summary>
    FacingLeftSonicBoomsRightFootForward1,
    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_0</c> at $AA:BAF2.</summary>
    FacingLeftSonicBoomsLeftFootForward0,
    /// <summary><c>InstList_Torizo_FacingLeft_SonicBooms_LeftFootForward_1</c> at $AA:BAFA.</summary>
    FacingLeftSonicBoomsLeftFootForward1,
    /// <summary><c>InstList_Torizo_FacingLeft_ExplosiveSwipe_RightFootForward</c> at $AA:BB5C.</summary>
    FacingLeftExplosiveSwipeRightFootForward,
    /// <summary><c>InstList_Torizo_FacingLeft_ExplosiveSwipe_LeftFootForward</c> at $AA:BBDE.</summary>
    FacingLeftExplosiveSwipeLeftFootForward,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingForwards_0</c> at $AA:BC60.</summary>
    FacingLeftJumpingForwards0,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingForwards_1</c> at $AA:BC70.</summary>
    FacingLeftJumpingForwards1,
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_0</c> at $AA:BC78.</summary>
    FacingLeftFalling0,
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_1</c> at $AA:BC80.</summary>
    FacingLeftFalling1,
    /// <summary><c>InstList_Torizo_FacingLeft_Falling_2</c> at $AA:BC88.</summary>
    FacingLeftFalling2,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_0</c> at $AA:BC96.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd0,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_1</c> at $AA:BCA6.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd1,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_2</c> at $AA:BCAE.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd2,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_3</c> at $AA:BCB6.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd3,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_LandLeftFootFwd_4</c> at $AA:BCBE.</summary>
    FacingLeftJumpingBackwardLandLeftFootFwd4,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_0</c> at $AA:BCD2.</summary>
    FacingLeftJumpingBackwardRightFootFwd0,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_1</c> at $AA:BCE2.</summary>
    FacingLeftJumpingBackwardRightFootFwd1,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_2</c> at $AA:BCEA.</summary>
    FacingLeftJumpingBackwardRightFootFwd2,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_3</c> at $AA:BCF2.</summary>
    FacingLeftJumpingBackwardRightFootFwd3,
    /// <summary><c>InstList_Torizo_FacingLeft_JumpingBackward_RightFootFwd_4</c> at $AA:BCFA.</summary>
    FacingLeftJumpingBackwardRightFootFwd4,
    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_TurningLeft</c> at $AA:BD0E.</summary>
    FacingLeftFacelessTurningLeft,
    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_Walking_RightLegMoving</c> at $AA:BD18.</summary>
    FacingLeftFacelessWalkingRightLegMoving,
    /// <summary><c>InstList_Torizo_FacingLeft_Faceless_Walking_LeftLegMoving</c> at $AA:BD52.</summary>
    FacingLeftFacelessWalkingLeftLegMoving,
    /// <summary>The unlabeled native list at $AA:BD90.</summary>
    FacingLeftFacelessWalkingLeftLegMovingTrailing,
    /// <summary>The code at $AA:BD9C after the preceding list's sleep, which the AI resumes.</summary>
    FacingLeftFacelessWalkingLeftLegMovingTrailingResumed,
    /// <summary><c>InstList_Torizo_FacingRight_TurningRight</c> at $AA:BDD8.</summary>
    FacingRightTurningRight,
    /// <summary><c>InstList_Torizo_FacingRight_Walking_LeftLegMoving</c> at $AA:BDE2.</summary>
    FacingRightWalkingLeftLegMoving,
    /// <summary><c>InstList_Torizo_FacingRight_Walking_RightLegMoving</c> at $AA:BE30.</summary>
    FacingRightWalkingRightLegMoving,
    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_0</c> at $AA:BE7E.</summary>
    FacingRightSpewingChozoOrbsLeftFootFwd0,
    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_LeftFootFwd_1</c> at $AA:BEA0.</summary>
    FacingRightSpewingChozoOrbsLeftFootFwd1,
    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_0</c> at $AA:BEC0.</summary>
    FacingRightSpewingChozoOrbsRightFootFwd0,
    /// <summary><c>InstList_Torizo_FacingRight_SpewingChozoOrbs_RightFootFwd_1</c> at $AA:BEE2.</summary>
    FacingRightSpewingChozoOrbsRightFootFwd1,
    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_0</c> at $AA:BF02.</summary>
    FacingRightSonicBoomsLeftFootForward0,
    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_LeftFootForward_1</c> at $AA:BF0A.</summary>
    FacingRightSonicBoomsLeftFootForward1,
    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_RightFootForward_0</c> at $AA:BF6C.</summary>
    FacingRightSonicBoomsRightFootForward0,
    /// <summary><c>InstList_Torizo_FacingRight_SonicBooms_RightFootForward_1</c> at $AA:BF74.</summary>
    FacingRightSonicBoomsRightFootForward1,
    /// <summary><c>InstList_Torizo_FacingRight_ExplosiveSwipe_LeftFootForward</c> at $AA:BFD6.</summary>
    FacingRightExplosiveSwipeLeftFootForward,
    /// <summary><c>InstList_Torizo_FacingRight_ExplosiveSwipe_RightFootForward</c> at $AA:C058.</summary>
    FacingRightExplosiveSwipeRightFootForward,
    /// <summary><c>InstList_Torizo_FacingRight_JumpingForwards_0</c> at $AA:C0DA.</summary>
    FacingRightJumpingForwards0,
    /// <summary><c>InstList_Torizo_FacingRight_JumpingForwards_1</c> at $AA:C0EA.</summary>
    FacingRightJumpingForwards1,
    /// <summary><c>InstList_Torizo_FacingRight_Falling_0</c> at $AA:C0F2.</summary>
    FacingRightFalling0,
    /// <summary><c>InstList_Torizo_FacingRight_Falling_1</c> at $AA:C0FA.</summary>
    FacingRightFalling1,
    /// <summary><c>InstList_Torizo_FacingRight_Falling_2</c> at $AA:C102.</summary>
    FacingRightFalling2,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_0</c> at $AA:C110.</summary>
    FacingRightJumpBackwardLandRightFootFwd0,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_1</c> at $AA:C120.</summary>
    FacingRightJumpBackwardLandRightFootFwd1,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_2</c> at $AA:C128.</summary>
    FacingRightJumpBackwardLandRightFootFwd2,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_3</c> at $AA:C130.</summary>
    FacingRightJumpBackwardLandRightFootFwd3,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackward_LandRightFootFwd_4</c> at $AA:C138.</summary>
    FacingRightJumpBackwardLandRightFootFwd4,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_0</c> at $AA:C14C.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd0,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_1</c> at $AA:C15C.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd1,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_2</c> at $AA:C164.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd2,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_3</c> at $AA:C16C.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd3,
    /// <summary><c>InstList_Torizo_FacingRight_JumpBackwards_LandLeftFootFwd_4</c> at $AA:C174.</summary>
    FacingRightJumpBackwardsLandLeftFootFwd4,
    /// <summary><c>InstList_Torizo_FacingRight_Faceless_TurningRight</c> at $AA:C188.</summary>
    FacingRightFacelessTurningRight,
    /// <summary><c>InstList_Torizo_FacingRight_Faceless_Walking_LeftLegMoving</c> at $AA:C192.</summary>
    FacingRightFacelessWalkingLeftLegMoving,
    /// <summary><c>InstList_Torizo_FacingRight_Faceless_Walking_RightLegMoving</c> at $AA:C1CC.</summary>
    FacingRightFacelessWalkingRightLegMoving,
    /// <summary><c>InstList_GoldenTorizo_Initial_0</c> at $AA:C9CB.</summary>
    GoldenInitial0,
    /// <summary>The code at $AA:C9E2 after the preceding list's sleep, which the AI resumes.</summary>
    GoldenInitial0Resumed,
    /// <summary><c>InstList_GoldenTorizo_Initial_1</c> at $AA:C9E6.</summary>
    GoldenInitial1,
    /// <summary><c>InstList_GoldenTorizo_Initial_2</c> at $AA:CA48.</summary>
    GoldenInitial2,
    /// <summary><c>InstList_GoldenTorizo_Initial_3</c> at $AA:CAB6.</summary>
    GoldenInitial3,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_0</c> at $AA:CAFF.</summary>
    GoldenSpewChozoOrbsFaceLeftRightFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_1</c> at $AA:CB1F.</summary>
    GoldenSpewChozoOrbsFaceLeftRightFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_0</c> at $AA:CB41.</summary>
    GoldenSpewChozoOrbsFacingLeftLeftFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_1</c> at $AA:CB61.</summary>
    GoldenSpewChozoOrbsFacingLeftLeftFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_0</c> at $AA:CB83.</summary>
    GoldenSonicBoomsFacingLeftRightFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_1</c> at $AA:CB8B.</summary>
    GoldenSonicBoomsFacingLeftRightFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_0</c> at $AA:CBED.</summary>
    GoldenSonicBoomsFacingLeftLeftFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_1</c> at $AA:CBF5.</summary>
    GoldenSonicBoomsFacingLeftLeftFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_0</c> at $AA:CC57.</summary>
    GoldenSpewChozoOrbFacingLeftLeftFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_1</c> at $AA:CC77.</summary>
    GoldenSpewChozoOrbFacingLeftLeftFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_0</c> at $AA:CC99.</summary>
    GoldenSpewChozoOrbFacingLeftRightFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_RightFootFwd_1</c> at $AA:CCB9.</summary>
    GoldenSpewChozoOrbFacingLeftRightFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_0</c> at $AA:CCDB.</summary>
    GoldenSonicBoomsFacingRightLeftFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_LeftFootFwd_1</c> at $AA:CCE3.</summary>
    GoldenSonicBoomsFacingRightLeftFootFwd1,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_0</c> at $AA:CD45.</summary>
    GoldenSonicBoomsFacingRightRightFootFwd0,
    /// <summary><c>InstList_GoldenTorizo_SonicBooms_FacingRight_RightFootFwd_1</c> at $AA:CD4D.</summary>
    GoldenSonicBoomsFacingRightRightFootFwd1,
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingLeft_LeftFootFwd</c> at $AA:CDAF.</summary>
    GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd,
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingLeft_RightFootFwd</c> at $AA:CDB9.</summary>
    GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd,
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingRight_RightFootFwd</c> at $AA:CDC3.</summary>
    GoldenLandedFromBackwardsJumpFacingRightRightFootFwd,
    /// <summary><c>InstList_GT_LandedFromBackwardsJump_FacingRight_LeftFootFwd</c> at $AA:CDCD.</summary>
    GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd,
    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingLeft_LeftLegFwd</c> at $AA:CDE1.</summary>
    GoldenCaughtSuperFacingLeftLeftLegFwd,
    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingLeft_RightLegFwd</c> at $AA:CE43.</summary>
    GoldenCaughtSuperFacingLeftRightLegFwd,
    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingRight_RightLegFwd</c> at $AA:CEA5.</summary>
    GoldenCaughtSuperFacingRightRightLegFwd,
    /// <summary><c>InstList_GoldenTorizo_CaughtSuper_FacingRight_LeftLegFwd</c> at $AA:CEFF.</summary>
    GoldenCaughtSuperFacingRightLeftLegFwd,
    /// <summary><c>InstList_GoldenTorizo_SitDownAttack_FacingLeft</c> at $AA:CF59.</summary>
    GoldenSitDownAttackFacingLeft,
    /// <summary><c>InstList_GoldenTorizo_SitDownAttack_FacingRight</c> at $AA:CFC5.</summary>
    GoldenSitDownAttackFacingRight,
    /// <summary><c>InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_0</c> at $AA:D031.</summary>
    GoldenReleaseGoldenTorizoEggs0,
    /// <summary><c>InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_1</c> at $AA:D07B.</summary>
    GoldenReleaseGoldenTorizoEggs1,
    /// <summary><c>InstList_GoldenTorizo_ReleaseGoldenTorizoEggs_2</c> at $AA:D087.</summary>
    GoldenReleaseGoldenTorizoEggs2,
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_0</c> at $AA:D10D.</summary>
    GoldenEyeBeamAttack0,
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_1</c> at $AA:D11F.</summary>
    GoldenEyeBeamAttack1,
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_2</c> at $AA:D133.</summary>
    GoldenEyeBeamAttack2,
    /// <summary><c>InstList_Torizo_Stunned_0</c> at $AA:D193.</summary>
    Stunned0,
    /// <summary><c>InstList_Torizo_Stunned_1</c> at $AA:D1A1.</summary>
    Stunned1,
    /// <summary><c>InstList_GoldenTorizo_Dodge_TurningLeft</c> at $AA:D1F1.</summary>
    GoldenDodgeTurningLeft,
    /// <summary><c>InstList_GoldenTorizo_TurningLeft</c> at $AA:D203.</summary>
    GoldenTurningLeft,
    /// <summary><c>InstList_GoldenTorizo_WalkingLeft_RightLegMoving</c> at $AA:D20D.</summary>
    GoldenWalkingLeftRightLegMoving,
    /// <summary><c>InstList_GoldenTorizo_WalkingLeft_LeftLegMoving</c> at $AA:D259.</summary>
    GoldenWalkingLeftLeftLegMoving,
    /// <summary><c>InstList_GoldenTorizo_Dodge_TurningRight</c> at $AA:D2AD.</summary>
    GoldenDodgeTurningRight,
    /// <summary><c>InstList_GoldenTorizo_TurningRight</c> at $AA:D2BF.</summary>
    GoldenTurningRight,
    /// <summary><c>InstList_GoldenTorizo_WalkingRight_LeftLegMoving</c> at $AA:D2C9.</summary>
    GoldenWalkingRightLeftLegMoving,
    /// <summary><c>InstList_GoldenTorizo_WalkingRight_RightLegMoving</c> at $AA:D315.</summary>
    GoldenWalkingRightRightLegMoving,
}
