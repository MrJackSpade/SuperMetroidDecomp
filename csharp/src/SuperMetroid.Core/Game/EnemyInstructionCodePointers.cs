namespace SuperMetroid.Core.Game;

/// <summary>Named 16-bit cartridge code pointers dispatched by the generic enemy runtime.</summary>
internal static class EnemyInstructionCodePointers
{
    /// <summary><c>InitAI_EnemyProjectile_CeresElevatorPad</c> at $86:A2EE.</summary>
    public const ushort InitAI_EnemyProjectile_CeresElevatorPad = 0xa2ee;

    /// <summary><c>Door_RinkaShaft_2</c> at $83:AAC8.</summary>
    public const ushort Door_RinkaShaft_2 = 0xaac8;

    /// <summary><c>Function_Ship_LandingOnZebes_Descending</c> at $A2:A80C.</summary>
    public const ushort Function_Ship_LandingOnZebes_Descending = 0xa80c;

    /// <summary><c>Function_Ship_LandingOnZebes_WaitForShipEntranceToOpen</c> at $A2:A942.</summary>
    public const ushort Function_Ship_LandingOnZebes_WaitForShipEntranceToOpen = 0xa942;

    /// <summary><c>Function_Ship_LandingOnZebes_EjectSamus</c> at $A2:A950.</summary>
    public const ushort Function_Ship_LandingOnZebes_EjectSamus = 0xa950;

    /// <summary><c>Function_Ship_SamusEntered_HandleSaveConfirmation</c> at $A2:AB1F.</summary>
    public const ushort Function_Ship_SamusEntered_HandleSaveConfirmation = 0xab1f;

    /// <summary><c>Function_Ship_SamusExiting_WaitForEntrancePadToOpen</c> at $A2:AB60.</summary>
    public const ushort Function_Ship_SamusExiting_WaitForEntrancePadToOpen = 0xab60;

    /// <summary><c>Function_Ship_SamusExiting_RaiseSamus</c> at $A2:AB6E.</summary>
    public const ushort Function_Ship_SamusExiting_RaiseSamus = 0xab6e;

    /// <summary><c>Function_Ship_Liftoff_FireUpEngines_SpawnDustClouds</c> at $A2:AC1B.</summary>
    public const ushort Function_Ship_Liftoff_FireUpEngines_SpawnDustClouds = 0xac1b;

    /// <summary><c>Function_Ship_Liftoff_SteadyRise</c> at $A2:ACD7.</summary>
    public const ushort Function_Ship_Liftoff_SteadyRise = 0xacd7;

    /// <summary><c>Function_Ship_Liftoff_Accelerating</c> at $A2:AD2D.</summary>
    public const ushort Function_Ship_Liftoff_Accelerating = 0xad2d;

    /// <summary><c>RTL_A288C5</c> at $A2:88C5.</summary>
    public const ushort RTL_A288C5 = 0x88c5;

    /// <summary><c>Instruction_Boyon_88C6</c> at $A2:88C6.</summary>
    public const ushort Instruction_Boyon_88C6 = 0x88c6;

    /// <summary><c>Instruction_Stoke_SetMovingLeft</c> at $A2:8990.</summary>
    public const ushort Instruction_Stoke_SetMovingLeft = 0x8990;

    /// <summary><c>Instruction_Stoke_SetMovingRight</c> at $A2:899D.</summary>
    public const ushort Instruction_Stoke_SetMovingRight = 0x899d;

    /// <summary><c>Instruction_BabyTurtle_LoopOrTurnAroundIfMovedTooFar</c> at $A2:9412.</summary>
    public const ushort Instruction_BabyTurtle_LoopOrTurnAroundIfMovedTooFar = 0x9412;

    /// <summary><c>Instruction_MamaTurtle_RiseToHoverRightwards</c> at $A2:9451.</summary>
    public const ushort Instruction_MamaTurtle_RiseToHoverRightwards = 0x9451;

    /// <summary><c>Instruction_BabyTurtle_LeaveShell</c> at $A2:9485.</summary>
    public const ushort Instruction_BabyTurtle_LeaveShell = 0x9485;

    /// <summary><c>Instruction_MamaTurtle_PlaySpinningSFX</c> at $A2:94D1.</summary>
    public const ushort Instruction_MamaTurtle_PlaySpinningSFX = 0x94d1;

    /// <summary><c>Instruction_KraidArm_SlowArmIfLessThanHalfHealth</c> at $A7:8A8F.</summary>
    public const ushort Instruction_KraidArm_SlowArmIfLessThanHalfHealth = 0x8a8f;

    /// <summary><c>Instruction_Kraid_DecrementYPosition</c> at $A7:B636.</summary>
    public const ushort Instruction_Kraid_DecrementYPosition = 0xb636;

    /// <summary><c>Instruction_Kraid_QueueSFX76_Lib2_Max6</c> at $A7:B64E.</summary>
    public const ushort Instruction_Kraid_QueueSFX76_Lib2_Max6 = 0xb64e;

    /// <summary><c>Instruction_YappingMaw_OffsetSamusUpRight</c> at $A8:A0C7.</summary>
    public const ushort Instruction_YappingMaw_OffsetSamusUpRight = 0xa0c7;

    /// <summary><c>Instruction_YappingMaw_OffsetSamusDownRight</c> at $A8:A0EB.</summary>
    public const ushort Instruction_YappingMaw_OffsetSamusDownRight = 0xa0eb;

    /// <summary><c>Instruction_YappingMaw_OffsetSamusUp</c> at $A8:A10F.</summary>
    public const ushort Instruction_YappingMaw_OffsetSamusUp = 0xa10f;

    /// <summary><c>Instruction_Cacatac_PlaySpikesSFX</c> at $A2:9F2A.</summary>
    public const ushort Instruction_Cacatac_PlaySpikesSFX = 0x9f2a;

    /// <summary><c>Instruction_Owtch_1</c> at $A2:A571.</summary>
    public const ushort Instruction_Owtch_1 = 0xa571;

    /// <summary><c>Instruction_FuneNamihe_QueueSpitSFX</c> at $A8:9625.</summary>
    public const ushort Instruction_FuneNamihe_QueueSpitSFX = 0x9625;

    /// <summary><c>Instruction_Evir_PlaySpitSFX</c> at $A8:878F.</summary>
    public const ushort Instruction_Evir_PlaySpitSFX = 0x878f;

    /// <summary><c>Instruction_Evir_SetInitialRegenerationXOffset</c> at $A8:879B.</summary>
    public const ushort Instruction_Evir_SetInitialRegenerationXOffset = 0x879b;

    /// <summary><c>Instruction_Evir_AdvanceRegenerationXOffset</c> at $A8:87B6.</summary>
    public const ushort Instruction_Evir_AdvanceRegenerationXOffset = 0x87b6;

    /// <summary><c>Instruction_FuneNamihe_FinishActivity</c> at $A8:9695.</summary>
    public const ushort Instruction_FuneNamihe_FinishActivity = 0x9695;

    /// <summary><c>Instruction_FuneNamihe_FinishActivity_duplicate</c> at $A8:96B4.</summary>
    public const ushort Instruction_FuneNamihe_FinishActivity_duplicate = 0x96b4;

    /// <summary><c>Instruction_Yard_DirectionInY</c> at $A3:CC48.</summary>
    public const ushort Instruction_Yard_DirectionInY = 0xcc48;

    /// <summary><c>Instruction_Yard_GoBack4BytesIfHidingOr50PercentChance</c> at $A3:CC78.</summary>
    public const ushort Instruction_Yard_GoBack4BytesIfHidingOr50PercentChance = 0xcc78;

    /// <summary><c>Instruction_Zoa_SetXSpeedTableIndexTo8</c> at $A3:B434.</summary>
    public const ushort Instruction_Zoa_SetXSpeedTableIndexTo8 = 0xb434;

    /// <summary><c>Instruction_Metroid_PlayDrainingSamusSFX</c> at $A3:EAA5.</summary>
    public const ushort Instruction_Metroid_PlayDrainingSamusSFX = 0xeaa5;

    /// <summary><c>Instruction_Metroid_PlayRandomMetroidSFX</c> at $A3:EAB1.</summary>
    public const ushort Instruction_Metroid_PlayRandomMetroidSFX = 0xeab1;

    /// <summary><c>Instruction_Crawlers_FunctionInY</c> at $A3:E660.</summary>
    public const ushort Instruction_Crawlers_FunctionInY = 0xe660;

    /// <summary><c>Instruction_Waver_SetSpinFinishedFlag</c> at $A3:86E3.</summary>
    public const ushort Instruction_Waver_SetSpinFinishedFlag = 0x86e3;

    /// <summary><c>Instruction_Skultera_SetLayerTo2</c> at $A3:90A0.</summary>
    public const ushort Instruction_Skultera_SetLayerTo2 = 0x90a0;

    /// <summary><c>Instruction_Skultera_SetTurnFinishedFlag</c> at $A3:90AA.</summary>
    public const ushort Instruction_Skultera_SetTurnFinishedFlag = 0x90aa;

    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingLeftXMovement_duplicate</c> at $A3:9C81.</summary>
    public const ushort Instruction_Tripper_Kamer2_SetMovingLeftXMovement_duplicate = 0x9c81;

    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingRightXMovement</c> at $A3:9C76.</summary>
    public const ushort Instruction_Tripper_Kamer2_SetMovingRightXMovement = 0x9c76;

    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingRightXMovement_duplicate</c> at $A3:9C8C.</summary>
    public const ushort Instruction_Tripper_Kamer2_SetMovingRightXMovement_duplicate = 0x9c8c;

    /// <summary><c>Instruction_Alcoon_SpawnAlcoonFireballHorizontally</c> at $A8:DF33.</summary>
    public const ushort Instruction_Alcoon_SpawnAlcoonFireballHorizontally = 0xdf33;

    /// <summary><c>Instruction_Kihunter_SetIdlingInstListsFacingForwards</c> at $A8:F526.</summary>
    public const ushort Instruction_Kihunter_SetIdlingInstListsFacingForwards = 0xf526;

    /// <summary><c>Instruction_Kihunter_SetFunctionToHop</c> at $A8:F5E4.</summary>
    public const ushort Instruction_Kihunter_SetFunctionToHop = 0xf5e4;

    /// <summary><c>Instruction_Kihunter_FireAcidSpitLeft</c> at $A8:F6D2.</summary>
    public const ushort Instruction_Kihunter_FireAcidSpitLeft = 0xf6d2;

    /// <summary><c>Instruction_Kihunter_FireAcidSpitRight</c> at $A8:F6D8.</summary>
    public const ushort Instruction_Kihunter_FireAcidSpitRight = 0xf6d8;

    /// <summary><c>Instruction_Ridley_QueueRoarSFX</c> at $A6:E4BE.</summary>
    public const ushort Instruction_Ridley_QueueRoarSFX = 0xe4be;

    /// <summary><c>Instruction_Hibashi_ActivityFrame0</c> at $A6:8E13.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame0 = 0x8e13;

    /// <summary><c>Instruction_Hibashi_ActivityFrame3</c> at $A6:8E55.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame3 = 0x8e55;

    /// <summary><c>Instruction_Hibashi_ActivityFrame4</c> at $A6:8E69.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame4 = 0x8e69;

    /// <summary><c>Instruction_Hibashi_ActivityFrame6</c> at $A6:8E91.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame6 = 0x8e91;

    /// <summary><c>Instruction_Hibashi_ActivityFrame9</c> at $A6:8ECD.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame9 = 0x8ecd;

    /// <summary><c>Instruction_Hibashi_ActivityFrameE</c> at $A6:8F31.</summary>
    public const ushort Instruction_Hibashi_ActivityFrameE = 0x8f31;

    /// <summary><c>Instruction_Hibashi_ActivityFrame12</c> at $A6:8F81.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame12 = 0x8f81;

    /// <summary><c>Instruction_Hibashi_ActivityFrame13</c> at $A6:8F95.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame13 = 0x8f95;

    /// <summary><c>Instruction_Hibashi_ActivityFrame14</c> at $A6:8FA9.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame14 = 0x8fa9;

    /// <summary><c>Instruction_Hibashi_FinishActivity</c> at $A6:8FD1.</summary>
    public const ushort Instruction_Hibashi_FinishActivity = 0x8fd1;

    /// <summary><c>Instruction_MiniKraid_PlayCrySFX</c> at $A6:9BB2.</summary>
    public const ushort Instruction_MiniKraid_PlayCrySFX = 0x9bb2;

    /// <summary><c>Instruction_MiniKraid_FireSpitRight</c> at $A6:9C02.</summary>
    public const ushort Instruction_MiniKraid_FireSpitRight = 0x9c02;

    /// <summary><c>Instruction_PirateWalking_FunctionInY</c> at $B2:FCB8.</summary>
    public const ushort Instruction_PirateWalking_FunctionInY = 0xfcb8;

    /// <summary><c>Instruction_PirateWalking_FireLaserLeftWithYOffsetInY</c> at $B2:FC68.</summary>
    public const ushort Instruction_PirateWalking_FireLaserLeftWithYOffsetInY = 0xfc68;

    /// <summary><c>Instruction_PirateWalking_FireLaserRightWithYOffsetInY</c> at $B2:FC90.</summary>
    public const ushort Instruction_PirateWalking_FireLaserRightWithYOffsetInY = 0xfc90;

    /// <summary><c>Instruction_PirateWalking_ChooseAMovement</c> at $B2:FCC8.</summary>
    public const ushort Instruction_PirateWalking_ChooseAMovement = 0xfcc8;

    /// <summary><c>Instruction_Ridley_ResetRoarFlag</c> at $A6:E4CA.</summary>
    public const ushort Instruction_Ridley_ResetRoarFlag = 0xe4ca;

    /// <summary><c>Inst_Ridley_GotoYAndSetTimerTo8IfNotNorfairOrSamusLowEnergy</c> at $A6:E4D2.</summary>
    public const ushort Inst_Ridley_GotoYAndSetTimerTo8IfNotNorfairOrSamusLowEnergy = 0xe4d2;

    /// <summary><c>Inst_RidleyCeres_UpdateSamusPrevPosition_HeldYDisplacement</c> at $A6:E501.</summary>
    public const ushort Inst_RidleyCeres_UpdateSamusPrevPosition_HeldYDisplacement = 0xe501;

    /// <summary><c>Instruction_Ridley_CalculateFireballAngleAndXYSpeeds</c> at $A6:E84D.</summary>
    public const ushort Instruction_Ridley_CalculateFireballAngleAndXYSpeeds = 0xe84d;

    /// <summary><c>FireTrailsFireball</c> at $A6:E909.</summary>
    public const ushort FireTrailsFireball = 0xe909;

    /// <summary><c>Instruction_RidleyCeres_SetRidleyMainAI_SetVerticalSpeed</c> at $A6:E969.</summary>
    public const ushort Instruction_RidleyCeres_SetRidleyMainAI_SetVerticalSpeed = 0xe969;

    /// <summary><c>Instruction_Ridley_SetRidleyMainAI_SetVerticalSpeed</c> at $A6:E976.</summary>
    public const ushort Instruction_Ridley_SetRidleyMainAI_SetVerticalSpeed = 0xe976;

    /// <summary><c>Instruction_CeresSteam_SetToTangibleAndVisible</c> at $A6:F135.</summary>
    public const ushort Instruction_CeresSteam_SetToTangibleAndVisible = 0xf135;

    /// <summary><c>Instruction_CeresDoor_GotoYIfAreaBossIsAlive</c> at $A6:F66A.</summary>
    public const ushort Instruction_CeresDoor_GotoYIfAreaBossIsAlive = 0xf66a;

    /// <summary><c>Instruction_CeresDoor_GotoYIfCeresRidleyHasNotEscaped</c> at $A6:F678.</summary>
    public const ushort Instruction_CeresDoor_GotoYIfCeresRidleyHasNotEscaped = 0xf678;

    /// <summary><c>Instruction_CeresDoor_SetDrawnByRidleyFlag</c> at $A6:F69F.</summary>
    public const ushort Instruction_CeresDoor_SetDrawnByRidleyFlag = 0xf69f;

    /// <summary><c>Instruction_CeresDoor_SetAsInvisible</c> at $A6:F6A6.</summary>
    public const ushort Instruction_CeresDoor_SetAsInvisible = 0xf6a6;

    /// <summary><c>Instruction_CeresDoor_SetAsVisible_ClearDrawnByRidleyFlag</c> at $A6:F6B0.</summary>
    public const ushort Instruction_CeresDoor_SetAsVisible_ClearDrawnByRidleyFlag = 0xf6b0;

    /// <summary><c>Instruction_CeresDoor_QueueOpeningSFX</c> at $A6:F6BD.</summary>
    public const ushort Instruction_CeresDoor_QueueOpeningSFX = 0xf6bd;

    /// <summary><c>Instruction_GotoLatchedOn</c> at $A9:F936.</summary>
    public const ushort Instruction_GotoLatchedOn = 0xf936;

    /// <summary><c>Instruction_BabyMetroid_GotoRemorse</c> at $A9:F990.</summary>
    public const ushort Instruction_BabyMetroid_GotoRemorse = 0xf990;

    /// <summary><c>Instruction_BabyMetroid_GotoY_OrPlayRemorseSFX</c> at $A9:F994.</summary>
    public const ushort Instruction_BabyMetroid_GotoY_OrPlayRemorseSFX = 0xf994;

    /// <summary><c>Instruction_Stoke_SpawnFireball</c> at $A2:897E.</summary>
    public const ushort Instruction_Stoke_SpawnFireball = 0x897e;

    /// <summary><c>Instruction_BabyTurtle_Crawl</c> at $A2:9381.</summary>
    public const ushort Instruction_BabyTurtle_Crawl = 0x9381;

    /// <summary><c>Instruction_MamaTurtle_EnterShell</c> at $A2:9447.</summary>
    public const ushort Instruction_MamaTurtle_EnterShell = 0x9447;

    /// <summary><c>Instruction_MamaTurtle_RiseToHoverLeftwards</c> at $A2:946B.</summary>
    public const ushort Instruction_MamaTurtle_RiseToHoverLeftwards = 0x946b;

    /// <summary><c>Instruction_BabyTurtle_LeftShell</c> at $A2:94A1.</summary>
    public const ushort Instruction_BabyTurtle_LeftShell = 0x94a1;

    /// <summary><c>Instruction_BabyTurtle_Set_Spinning_Stoppable</c> at $A2:94C7.</summary>
    public const ushort Instruction_BabyTurtle_Set_Spinning_Stoppable = 0x94c7;

    /// <summary><c>Instruction_Kraid_NOP_A7B633</c> at $A7:B633.</summary>
    public const ushort Instruction_Kraid_NOP_A7B633 = 0xb633;

    /// <summary><c>Instruction_Kraid_IncrementYPosition_SetScreenShaking</c> at $A7:B63C.</summary>
    public const ushort Instruction_Kraid_IncrementYPosition_SetScreenShaking = 0xb63c;

    /// <summary><c>Instruction_Kraid_XPositionMinus3</c> at $A7:B65A.</summary>
    public const ushort Instruction_Kraid_XPositionMinus3 = 0xb65a;

    /// <summary><c>Instruction_Kraid_XPositionMinus3_duplicate</c> at $A7:B667.</summary>
    public const ushort Instruction_Kraid_XPositionMinus3_duplicate = 0xb667;

    /// <summary><c>Instruction_Kraid_XPositionPlus3</c> at $A7:B674.</summary>
    public const ushort Instruction_Kraid_XPositionPlus3 = 0xb674;

    /// <summary><c>UNUSED_Instruction_Kraid_MoveRight_A7B683</c> at $A7:B683.</summary>
    public const ushort UNUSED_Instruction_Kraid_MoveRight_A7B683 = 0xb683;

    /// <summary><c>Instruction_CommonA7_CallFunctionInY</c> at $A7:808A.</summary>
    public const ushort Instruction_CommonA7_CallFunctionInY = 0x808a;

    /// <summary><c>Instruction_Beetom_Nothing</c> at $A8:B75E.</summary>
    public const ushort Instruction_Beetom_Nothing = 0xb75e;

    /// <summary><c>Instruction_YappingMaw_OffsetSamusUpLeft</c> at $A8:A0D9.</summary>
    public const ushort Instruction_YappingMaw_OffsetSamusUpLeft = 0xa0d9;

    /// <summary><c>Instruction_YappingMaw_OffsetSamusDownLeft</c> at $A8:A0FD.</summary>
    public const ushort Instruction_YappingMaw_OffsetSamusDownLeft = 0xa0fd;

    /// <summary><c>Instruction_YappingMaw_OffsetSamusDown</c> at $A8:A121.</summary>
    public const ushort Instruction_YappingMaw_OffsetSamusDown = 0xa121;

    /// <summary><c>Instruction_YappingMaw_QueueSFXIfOnScreen</c> at $A8:A133.</summary>
    public const ushort Instruction_YappingMaw_QueueSFXIfOnScreen = 0xa133;

    /// <summary><c>Instruction_Cacatac_SetFunction_MovingLeftRight</c> at $A2:A095.</summary>
    public const ushort Instruction_Cacatac_SetFunction_MovingLeftRight = 0xa095;

    /// <summary><c>Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY</c> at $A2:A0A7.</summary>
    public const ushort Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY = 0xa0a7;

    /// <summary><c>Instruction_Owtch_0</c> at $A2:A56D.</summary>
    public const ushort Instruction_Owtch_0 = 0xa56d;

    /// <summary><c>Instruction_Evir_FinishRegeneration</c> at $A8:87CB.</summary>
    public const ushort Instruction_Evir_FinishRegeneration = 0x87cb;

    /// <summary><c>Instruction_Namihe_SpawnFireball_FacingLeft</c> at $A8:9631.</summary>
    public const ushort Instruction_Namihe_SpawnFireball_FacingLeft = 0x9631;

    /// <summary><c>Instruction_Namihe_SpawnFireball_FacingRight</c> at $A8:964A.</summary>
    public const ushort Instruction_Namihe_SpawnFireball_FacingRight = 0x964a;

    /// <summary><c>Instruction_Fune_SpawnFireball_FacingLeft</c> at $A8:9663.</summary>
    public const ushort Instruction_Fune_SpawnFireball_FacingLeft = 0x9663;

    /// <summary><c>Instruction_Fune_SpawnFireball_FacingRight</c> at $A8:967C.</summary>
    public const ushort Instruction_Fune_SpawnFireball_FacingRight = 0x967c;

    /// <summary><c>Instruction_Yard_MovementFunctionInY</c> at $A3:CC36.</summary>
    public const ushort Instruction_Yard_MovementFunctionInY = 0xcc36;

    /// <summary><c>Instruction_Yard_HidingInstListInY</c> at $A3:CC3F.</summary>
    public const ushort Instruction_Yard_HidingInstListInY = 0xcc3f;

    /// <summary><c>Instruction_Yard_MoveByPixelsInY</c> at $A3:CC5F.</summary>
    public const ushort Instruction_Yard_MoveByPixelsInY = 0xcc5f;

    /// <summary><c>Instruction_Sidehopper_QueueSoundInY_Lib2_Max3</c> at $A3:AA68.</summary>
    public const ushort Instruction_Sidehopper_QueueSoundInY_Lib2_Max3 = 0xaa68;

    /// <summary><c>Instruction_Hopper_ReadyToHop</c> at $A3:AAFE.</summary>
    public const ushort Instruction_Hopper_ReadyToHop = 0xaafe;

    /// <summary><c>Instruction_Zoa_SetXSpeedTableIndexTo4</c> at $A3:B429.</summary>
    public const ushort Instruction_Zoa_SetXSpeedTableIndexTo4 = 0xb429;

    /// <summary><c>Instruction_Zoa_SetXSpeedTableIndexToC</c> at $A3:B43F.</summary>
    public const ushort Instruction_Zoa_SetXSpeedTableIndexToC = 0xb43f;

    /// <summary><c>Instruction_HZoomer_FunctionInY</c> at $A3:DFC2.</summary>
    public const ushort Instruction_HZoomer_FunctionInY = 0xdfc2;

    /// <summary><c>Instruction_Skree_SetAttackReadyFlag</c> at $A3:C6A4.</summary>
    public const ushort Instruction_Skree_SetAttackReadyFlag = 0xc6a4;

    /// <summary><c>Instruction_Metaree_SetAttackReadyFlag</c> at $A3:8956.</summary>
    public const ushort Instruction_Metaree_SetAttackReadyFlag = 0x8956;

    /// <summary><c>Instruction_Skultera_SetLayerTo6</c> at $A3:9096.</summary>
    public const ushort Instruction_Skultera_SetLayerTo6 = 0x9096;

    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingLeftXMovement</c> at $A3:9C6B.</summary>
    public const ushort Instruction_Tripper_Kamer2_SetMovingLeftXMovement = 0x9c6b;

    /// <summary><c>Instruction_Alcoon_SpawnAlcoonFireballUpward</c> at $A8:DF1C.</summary>
    public const ushort Instruction_Alcoon_SpawnAlcoonFireballUpward = 0xdf1c;

    /// <summary><c>Instruction_Alcoon_SpawnAlcoonFireballDownward</c> at $A8:DF39.</summary>
    public const ushort Instruction_Alcoon_SpawnAlcoonFireballDownward = 0xdf39;

    /// <summary><c>Instruction_Alcoon_StartWalking</c> at $A8:DF3F.</summary>
    public const ushort Instruction_Alcoon_StartWalking = 0xdf3f;

    /// <summary><c>Instruction_Alcoon_DecrementStepCounter_MoveHorizontally</c> at $A8:DF63.</summary>
    public const ushort Instruction_Alcoon_DecrementStepCounter_MoveHorizontally = 0xdf63;

    /// <summary><c>Instruction_Alcoon_MoveHorizontally_TurnIfWallCollision</c> at $A8:DF71.</summary>
    public const ushort Instruction_Alcoon_MoveHorizontally_TurnIfWallCollision = 0xdf71;

    /// <summary><c>Instruction_Kihunter_SetFunctionTo_Wingless_Thinking</c> at $A8:F67F.</summary>
    public const ushort Instruction_Kihunter_SetFunctionTo_Wingless_Thinking = 0xf67f;

    /// <summary><c>Instruction_Spark_SetAsIntangible</c> at $A8:E61D.</summary>
    public const ushort Instruction_Spark_SetAsIntangible = 0xe61d;

    /// <summary><c>Instruction_Spark_SetAsTangible</c> at $A8:E62A.</summary>
    public const ushort Instruction_Spark_SetAsTangible = 0xe62a;

    /// <summary><c>Instruction_Hibashi_PlaySFX</c> at $A6:8DAF.</summary>
    public const ushort Instruction_Hibashi_PlaySFX = 0x8daf;

    /// <summary><c>Instruction_Hibashi_ActivityFrame1</c> at $A6:8E2D.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame1 = 0x8e2d;

    /// <summary><c>Instruction_Hibashi_ActivityFrame2</c> at $A6:8E41.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame2 = 0x8e41;

    /// <summary><c>Instruction_Hibashi_ActivityFrame5</c> at $A6:8E7D.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame5 = 0x8e7d;

    /// <summary><c>Instruction_Hibashi_ActivityFrame7</c> at $A6:8EA5.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame7 = 0x8ea5;

    /// <summary><c>Instruction_Hibashi_ActivityFrame8</c> at $A6:8EB9.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame8 = 0x8eb9;

    /// <summary><c>Instruction_Hibashi_ActivityFrameA</c> at $A6:8EE1.</summary>
    public const ushort Instruction_Hibashi_ActivityFrameA = 0x8ee1;

    /// <summary><c>Instruction_Hibashi_ActivityFrameB</c> at $A6:8EF5.</summary>
    public const ushort Instruction_Hibashi_ActivityFrameB = 0x8ef5;

    /// <summary><c>Instruction_Hibashi_ActivityFrameC</c> at $A6:8F09.</summary>
    public const ushort Instruction_Hibashi_ActivityFrameC = 0x8f09;

    /// <summary><c>Instruction_Hibashi_ActivityFrameD</c> at $A6:8F1D.</summary>
    public const ushort Instruction_Hibashi_ActivityFrameD = 0x8f1d;

    /// <summary><c>Instruction_Hibashi_ActivityFrameF</c> at $A6:8F45.</summary>
    public const ushort Instruction_Hibashi_ActivityFrameF = 0x8f45;

    /// <summary><c>Instruction_Hibashi_ActivityFrame10</c> at $A6:8F59.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame10 = 0x8f59;

    /// <summary><c>Instruction_Hibashi_ActivityFrame11</c> at $A6:8F6D.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame11 = 0x8f6d;

    /// <summary><c>Instruction_Hibashi_ActivityFrame15</c> at $A6:8FBD.</summary>
    public const ushort Instruction_Hibashi_ActivityFrame15 = 0x8fbd;

    /// <summary><c>Instruction_MiniKraid_Move</c> at $A6:9B26.</summary>
    public const ushort Instruction_MiniKraid_Move = 0x9b26;

    /// <summary><c>Instruction_MiniKraid_ChooseAction</c> at $A6:9B74.</summary>
    public const ushort Instruction_MiniKraid_ChooseAction = 0x9b74;

    /// <summary><c>Instruction_MiniKraid_FireSpitLeft</c> at $A6:9BC4.</summary>
    public const ushort Instruction_MiniKraid_FireSpitLeft = 0x9bc4;

    /// <summary><c>Instruction_SidehopperCorpse_EndHop</c> at $A9:ECD0.</summary>
    public const ushort Instruction_SidehopperCorpse_EndHop = 0xecd0;

    /// <summary><c>Instruction_BabyMetroid_GotoNormal</c> at $A9:F920.</summary>
    public const ushort Instruction_BabyMetroid_GotoNormal = 0xf920;

}
