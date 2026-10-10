namespace SuperMetroid.Core.Game;

/// <summary>Boyon's private bank-$A2 animation instructions.</summary>
internal enum BoyonInstruction : ushort
{
    /// <summary><c>RTL_A288C5</c> at $A2:88C5.</summary>
    Return = 0x88c5,

    /// <summary><c>Instruction_Boyon_88C6</c> at $A2:88C6.</summary>
    StartBounce = 0x88c6,
}

/// <summary>Stoke's private bank-$A2 animation instructions.</summary>
internal enum StokeInstruction : ushort
{
    /// <summary><c>Instruction_Stoke_SpawnFireball</c> at $A2:897E.</summary>
    SpawnFireball = 0x897e,

    /// <summary><c>Instruction_Stoke_SetMovingLeft</c> at $A2:8990.</summary>
    SetMovingLeft = 0x8990,

    /// <summary><c>Instruction_Stoke_SetMovingRight</c> at $A2:899D.</summary>
    SetMovingRight = 0x899d,
}

/// <summary>Cacatac's private bank-$A2 animation instructions.</summary>
internal enum CacatacInstruction : ushort
{
    /// <summary><c>Instruction_Cacatac_PlaySpikesSFX</c> at $A2:9F2A.</summary>
    PlaySpikesSFX = 0x9f2a,

    /// <summary><c>Instruction_Cacatac_SetFunction_MovingLeftRight</c> at $A2:A095.</summary>
    SetFunctionMovingLeftRight = 0xa095,

    /// <summary><c>Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY</c> at $A2:A0A7.</summary>
    SpawnSpikeProjectileWithParameterInY = 0xa0a7,
}

/// <summary>Owtch's private bank-$A2 animation instructions.</summary>
internal enum OwtchInstruction : ushort
{
    /// <summary><c>Instruction_Owtch_0</c> at $A2:A56D.</summary>
    SetMovingLeft = 0xa56d,

    /// <summary><c>Instruction_Owtch_1</c> at $A2:A571.</summary>
    SetMovingRight = 0xa571,
}

/// <summary>Waver's private bank-$A3 animation instruction.</summary>
internal enum WaverInstruction : ushort
{
    /// <summary><c>Instruction_Waver_SetSpinFinishedFlag</c> at $A3:86E3.</summary>
    SetSpinFinishedFlag = 0x86e3,
}

/// <summary>Metaree's private bank-$A3 animation instruction.</summary>
internal enum MetareeInstruction : ushort
{
    /// <summary><c>Instruction_Metaree_SetAttackReadyFlag</c> at $A3:8956.</summary>
    SetAttackReadyFlag = 0x8956,
}

/// <summary>Skultera's private bank-$A3 animation instructions.</summary>
internal enum SkulteraInstruction : ushort
{
    /// <summary><c>Instruction_Skultera_SetLayerTo6</c> at $A3:9096.</summary>
    SetLayerTo6 = 0x9096,

    /// <summary><c>Instruction_Skultera_SetLayerTo2</c> at $A3:90A0.</summary>
    SetLayerTo2 = 0x90a0,

    /// <summary><c>Instruction_Skultera_SetTurnFinishedFlag</c> at $A3:90AA.</summary>
    SetTurnFinishedFlag = 0x90aa,
}

/// <summary>The Tripper/Kamer platform family's private bank-$A3 animation instructions.</summary>
internal enum PlatformInstruction : ushort
{
    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingLeftXMovement</c> at $A3:9C6B.</summary>
    SetMovingLeftXMovement = 0x9c6b,

    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingRightXMovement</c> at $A3:9C76.</summary>
    SetMovingRightXMovement = 0x9c76,

    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingLeftXMovement_duplicate</c> at $A3:9C81.</summary>
    SetMovingLeftXMovementDuplicate = 0x9c81,

    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingRightXMovement_duplicate</c> at $A3:9C8C.</summary>
    SetMovingRightXMovementDuplicate = 0x9c8c,
}

/// <summary>The hopper family's private bank-$A3 animation instructions.</summary>
internal enum HopperInstruction : ushort
{
    /// <summary><c>Instruction_Sidehopper_QueueSoundInY_Lib2_Max3</c> at $A3:AA68.</summary>
    SidehopperQueueSoundInY = 0xaa68,

    /// <summary><c>Instruction_Hopper_ReadyToHop</c> at $A3:AAFE.</summary>
    ReadyToHop = 0xaafe,
}

/// <summary>Zoa's private bank-$A3 animation instructions.</summary>
internal enum ZoaInstruction : ushort
{
    /// <summary><c>Instruction_Zoa_SetXSpeedTableIndexTo4</c> at $A3:B429.</summary>
    SetXSpeedTableIndexTo4 = 0xb429,

    /// <summary><c>Instruction_Zoa_SetXSpeedTableIndexTo8</c> at $A3:B434.</summary>
    SetXSpeedTableIndexTo8 = 0xb434,

    /// <summary><c>Instruction_Zoa_SetXSpeedTableIndexToC</c> at $A3:B43F.</summary>
    SetXSpeedTableIndexToC = 0xb43f,
}

/// <summary>Skree's private bank-$A3 animation instruction.</summary>
internal enum SkreeInstruction : ushort
{
    /// <summary><c>Instruction_Skree_SetAttackReadyFlag</c> at $A3:C6A4.</summary>
    SetAttackReadyFlag = 0xc6a4,
}

/// <summary>Yard's private bank-$A3 animation instructions.</summary>
internal enum YardInstruction : ushort
{
    /// <summary><c>Instruction_Yard_MovementFunctionInY</c> at $A3:CC36.</summary>
    MovementFunctionInY = 0xcc36,

    /// <summary><c>Instruction_Yard_HidingInstListInY</c> at $A3:CC3F.</summary>
    HidingInstListInY = 0xcc3f,

    /// <summary><c>Instruction_Yard_DirectionInY</c> at $A3:CC48.</summary>
    DirectionInY = 0xcc48,

    /// <summary><c>Instruction_Yard_MoveByPixelsInY</c> at $A3:CC5F.</summary>
    MoveByPixelsInY = 0xcc5f,

    /// <summary><c>Instruction_Yard_GoBack4BytesIfHidingOr50PercentChance</c> at $A3:CC78.</summary>
    GoBack4BytesIfHidingOr50PercentChance = 0xcc78,
}

/// <summary>The orange Zoomer's private bank-$A3 animation instruction.</summary>
internal enum HZoomerInstruction : ushort
{
    /// <summary><c>Instruction_HZoomer_FunctionInY</c> at $A3:DFC2.</summary>
    FunctionInY = 0xdfc2,
}

/// <summary>The shared crawler family's private bank-$A3 animation instruction.</summary>
internal enum CrawlerInstruction : ushort
{
    /// <summary><c>Instruction_Crawlers_FunctionInY</c> at $A3:E660.</summary>
    FunctionInY = 0xe660,
}

/// <summary>Metroid's private bank-$A3 animation instructions.</summary>
internal enum MetroidInstruction : ushort
{
    /// <summary><c>Instruction_Metroid_PlayDrainingSamusSFX</c> at $A3:EAA5.</summary>
    PlayDrainingSamusSFX = 0xeaa5,

    /// <summary><c>Instruction_Metroid_PlayRandomMetroidSFX</c> at $A3:EAB1.</summary>
    PlayRandomMetroidSFX = 0xeab1,
}

/// <summary>Hibashi's private bank-$A6 animation instructions.</summary>
internal enum HibashiInstruction : ushort
{
    /// <summary><c>Instruction_Hibashi_PlaySFX</c> at $A6:8DAF.</summary>
    PlaySFX = 0x8daf,

    /// <summary><c>Instruction_Hibashi_ActivityFrame0</c> at $A6:8E13.</summary>
    ActivityFrame0 = 0x8e13,

    /// <summary><c>Instruction_Hibashi_ActivityFrame1</c> at $A6:8E2D.</summary>
    ActivityFrame1 = 0x8e2d,

    /// <summary><c>Instruction_Hibashi_ActivityFrame2</c> at $A6:8E41.</summary>
    ActivityFrame2 = 0x8e41,

    /// <summary><c>Instruction_Hibashi_ActivityFrame3</c> at $A6:8E55.</summary>
    ActivityFrame3 = 0x8e55,

    /// <summary><c>Instruction_Hibashi_ActivityFrame4</c> at $A6:8E69.</summary>
    ActivityFrame4 = 0x8e69,

    /// <summary><c>Instruction_Hibashi_ActivityFrame5</c> at $A6:8E7D.</summary>
    ActivityFrame5 = 0x8e7d,

    /// <summary><c>Instruction_Hibashi_ActivityFrame6</c> at $A6:8E91.</summary>
    ActivityFrame6 = 0x8e91,

    /// <summary><c>Instruction_Hibashi_ActivityFrame7</c> at $A6:8EA5.</summary>
    ActivityFrame7 = 0x8ea5,

    /// <summary><c>Instruction_Hibashi_ActivityFrame8</c> at $A6:8EB9.</summary>
    ActivityFrame8 = 0x8eb9,

    /// <summary><c>Instruction_Hibashi_ActivityFrame9</c> at $A6:8ECD.</summary>
    ActivityFrame9 = 0x8ecd,

    /// <summary><c>Instruction_Hibashi_ActivityFrameA</c> at $A6:8EE1.</summary>
    ActivityFrameA = 0x8ee1,

    /// <summary><c>Instruction_Hibashi_ActivityFrameB</c> at $A6:8EF5.</summary>
    ActivityFrameB = 0x8ef5,

    /// <summary><c>Instruction_Hibashi_ActivityFrameC</c> at $A6:8F09.</summary>
    ActivityFrameC = 0x8f09,

    /// <summary><c>Instruction_Hibashi_ActivityFrameD</c> at $A6:8F1D.</summary>
    ActivityFrameD = 0x8f1d,

    /// <summary><c>Instruction_Hibashi_ActivityFrameE</c> at $A6:8F31.</summary>
    ActivityFrameE = 0x8f31,

    /// <summary><c>Instruction_Hibashi_ActivityFrameF</c> at $A6:8F45.</summary>
    ActivityFrameF = 0x8f45,

    /// <summary><c>Instruction_Hibashi_ActivityFrame10</c> at $A6:8F59.</summary>
    ActivityFrame10 = 0x8f59,

    /// <summary><c>Instruction_Hibashi_ActivityFrame11</c> at $A6:8F6D.</summary>
    ActivityFrame11 = 0x8f6d,

    /// <summary><c>Instruction_Hibashi_ActivityFrame12</c> at $A6:8F81.</summary>
    ActivityFrame12 = 0x8f81,

    /// <summary><c>Instruction_Hibashi_ActivityFrame13</c> at $A6:8F95.</summary>
    ActivityFrame13 = 0x8f95,

    /// <summary><c>Instruction_Hibashi_ActivityFrame14</c> at $A6:8FA9.</summary>
    ActivityFrame14 = 0x8fa9,

    /// <summary><c>Instruction_Hibashi_ActivityFrame15</c> at $A6:8FBD.</summary>
    ActivityFrame15 = 0x8fbd,

    /// <summary><c>Instruction_Hibashi_FinishActivity</c> at $A6:8FD1.</summary>
    FinishActivity = 0x8fd1,
}

/// <summary>Mini-Kraid's private bank-$A6 animation instructions.</summary>
internal enum FakeKraidInstruction : ushort
{
    /// <summary><c>Instruction_MiniKraid_Move</c> at $A6:9B26.</summary>
    Move = 0x9b26,

    /// <summary><c>Instruction_MiniKraid_ChooseAction</c> at $A6:9B74.</summary>
    ChooseAction = 0x9b74,

    /// <summary><c>Instruction_MiniKraid_PlayCrySFX</c> at $A6:9BB2.</summary>
    PlayCrySFX = 0x9bb2,

    /// <summary><c>Instruction_MiniKraid_FireSpitLeft</c> at $A6:9BC4.</summary>
    FireSpitLeft = 0x9bc4,

    /// <summary><c>Instruction_MiniKraid_FireSpitRight</c> at $A6:9C02.</summary>
    FireSpitRight = 0x9c02,
}

/// <summary>The Evir projectile's private bank-$A8 animation instructions.</summary>
internal enum EvirInstruction : ushort
{
    /// <summary><c>Instruction_Evir_PlaySpitSFX</c> at $A8:878F.</summary>
    PlaySpitSFX = 0x878f,

    /// <summary><c>Instruction_Evir_SetInitialRegenerationXOffset</c> at $A8:879B.</summary>
    SetInitialRegenerationXOffset = 0x879b,

    /// <summary><c>Instruction_Evir_AdvanceRegenerationXOffset</c> at $A8:87B6.</summary>
    AdvanceRegenerationXOffset = 0x87b6,

    /// <summary><c>Instruction_Evir_FinishRegeneration</c> at $A8:87CB.</summary>
    FinishRegeneration = 0x87cb,
}

/// <summary>The Fune/Namihe family's private bank-$A8 animation instructions.</summary>
internal enum FuneNamiheInstruction : ushort
{
    /// <summary><c>Instruction_FuneNamihe_QueueSpitSFX</c> at $A8:9625.</summary>
    QueueSpitSFX = 0x9625,

    /// <summary><c>Instruction_Namihe_SpawnFireball_FacingLeft</c> at $A8:9631.</summary>
    NamiheSpawnFireballFacingLeft = 0x9631,

    /// <summary><c>Instruction_Namihe_SpawnFireball_FacingRight</c> at $A8:964A.</summary>
    NamiheSpawnFireballFacingRight = 0x964a,

    /// <summary><c>Instruction_Fune_SpawnFireball_FacingLeft</c> at $A8:9663.</summary>
    FuneSpawnFireballFacingLeft = 0x9663,

    /// <summary><c>Instruction_Fune_SpawnFireball_FacingRight</c> at $A8:967C.</summary>
    FuneSpawnFireballFacingRight = 0x967c,

    /// <summary><c>Instruction_FuneNamihe_FinishActivity</c> at $A8:9695.</summary>
    FinishActivity = 0x9695,

    /// <summary><c>Instruction_FuneNamihe_FinishActivity_duplicate</c> at $A8:96B4.</summary>
    FinishActivityDuplicate = 0x96b4,
}

/// <summary>Yapping Maw's private bank-$A8 animation instructions.</summary>
internal enum YappingMawInstruction : ushort
{
    /// <summary><c>Instruction_YappingMaw_OffsetSamusUpRight</c> at $A8:A0C7.</summary>
    OffsetSamusUpRight = 0xa0c7,

    /// <summary><c>Instruction_YappingMaw_OffsetSamusUpLeft</c> at $A8:A0D9.</summary>
    OffsetSamusUpLeft = 0xa0d9,

    /// <summary><c>Instruction_YappingMaw_OffsetSamusDownRight</c> at $A8:A0EB.</summary>
    OffsetSamusDownRight = 0xa0eb,

    /// <summary><c>Instruction_YappingMaw_OffsetSamusDownLeft</c> at $A8:A0FD.</summary>
    OffsetSamusDownLeft = 0xa0fd,

    /// <summary><c>Instruction_YappingMaw_OffsetSamusUp</c> at $A8:A10F.</summary>
    OffsetSamusUp = 0xa10f,

    /// <summary><c>Instruction_YappingMaw_OffsetSamusDown</c> at $A8:A121.</summary>
    OffsetSamusDown = 0xa121,

    /// <summary><c>Instruction_YappingMaw_QueueSFXIfOnScreen</c> at $A8:A133.</summary>
    QueueSFXIfOnScreen = 0xa133,
}

/// <summary>Beetom's private bank-$A8 animation instruction.</summary>
internal enum BeetomInstruction : ushort
{
    /// <summary><c>Instruction_Beetom_Nothing</c> at $A8:B75E.</summary>
    Nothing = 0xb75e,
}

/// <summary>Alcoon's private bank-$A8 animation instructions.</summary>
internal enum AlcoonInstruction : ushort
{
    /// <summary><c>Instruction_Alcoon_SpawnAlcoonFireballUpward</c> at $A8:DF1C.</summary>
    SpawnFireballUpward = 0xdf1c,

    /// <summary><c>Instruction_Alcoon_SpawnAlcoonFireballHorizontally</c> at $A8:DF33.</summary>
    SpawnFireballHorizontally = 0xdf33,

    /// <summary><c>Instruction_Alcoon_SpawnAlcoonFireballDownward</c> at $A8:DF39.</summary>
    SpawnFireballDownward = 0xdf39,

    /// <summary><c>Instruction_Alcoon_StartWalking</c> at $A8:DF3F.</summary>
    StartWalking = 0xdf3f,

    /// <summary><c>Instruction_Alcoon_DecrementStepCounter_MoveHorizontally</c> at $A8:DF63.</summary>
    DecrementStepCounterMoveHorizontally = 0xdf63,

    /// <summary><c>Instruction_Alcoon_MoveHorizontally_TurnIfWallCollision</c> at $A8:DF71.</summary>
    MoveHorizontallyTurnIfWallCollision = 0xdf71,
}

/// <summary>Spark's private bank-$A8 animation instructions.</summary>
internal enum SparkInstruction : ushort
{
    /// <summary><c>Instruction_Spark_SetAsIntangible</c> at $A8:E61D.</summary>
    SetAsIntangible = 0xe61d,

    /// <summary><c>Instruction_Spark_SetAsTangible</c> at $A8:E62A.</summary>
    SetAsTangible = 0xe62a,
}

/// <summary>The Ki-Hunter body's private bank-$A8 animation instructions.</summary>
internal enum KiHunterInstruction : ushort
{
    /// <summary><c>Instruction_Kihunter_SetIdlingInstListsFacingForwards</c> at $A8:F526.</summary>
    SetIdlingInstListsFacingForwards = 0xf526,

    /// <summary><c>Instruction_Kihunter_SetFunctionToHop</c> at $A8:F5E4.</summary>
    SetFunctionToHop = 0xf5e4,

    /// <summary><c>Instruction_Kihunter_SetFunctionTo_Wingless_Thinking</c> at $A8:F67F.</summary>
    SetFunctionToWinglessThinking = 0xf67f,

    /// <summary><c>Instruction_Kihunter_FireAcidSpitLeft</c> at $A8:F6D2.</summary>
    FireAcidSpitLeft = 0xf6d2,

    /// <summary><c>Instruction_Kihunter_FireAcidSpitRight</c> at $A8:F6D8.</summary>
    FireAcidSpitRight = 0xf6d8,
}

/// <summary>The dead Sidehopper's private bank-$A9 animation instruction.</summary>
internal enum DeadSidehopperInstruction : ushort
{
    /// <summary><c>Instruction_SidehopperCorpse_EndHop</c> at $A9:ECD0.</summary>
    EndHop = 0xecd0,
}

/// <summary>The Tourian Baby Metroid's private bank-$A9 animation instructions.</summary>
internal enum ShitroidInstruction : ushort
{
    /// <summary><c>Instruction_BabyMetroid_GotoNormal</c> at $A9:F920.</summary>
    GotoNormal = 0xf920,

    /// <summary><c>Instruction_GotoLatchedOn</c> at $A9:F936.</summary>
    GotoLatchedOn = 0xf936,

    /// <summary><c>Instruction_BabyMetroid_GotoRemorse</c> at $A9:F990.</summary>
    GotoRemorse = 0xf990,

    /// <summary><c>Instruction_BabyMetroid_GotoY_OrPlayRemorseSFX</c> at $A9:F994.</summary>
    GotoYOrPlayRemorseSFX = 0xf994,
}

/// <summary>Sound identifiers published by <see cref="ShitroidInstruction"/> routines.</summary>
internal static class ShitroidInstructionSounds
{
    /// <summary>Library-two remorse cry $52 played by <see cref="ShitroidInstruction.GotoYOrPlayRemorseSFX"/>.</summary>
    internal const ushort RemorseCry = 0x0052;
}

/// <summary>Dragon's private bank-$A2 animation instruction.</summary>
internal enum DragonInstruction : ushort
{
    /// <summary><c>$A2:E5FB</c>, marks the current body attack animation complete.</summary>
    AttackFinishedCallback = 0xe5fb,
}

/// <summary>Native layout of Hibashi's activity-frame instruction routines.</summary>
internal static class HibashiInstructionLayout
{
    /// <summary>
    /// Byte stride of the 22 <see cref="HibashiInstruction"/> activity-frame routines at
    /// $A6:8E13-$8FBD. Frame zero is $1A bytes long because it also sets the initial X radius;
    /// every later routine follows at this stride.
    /// </summary>
    private const int ActivityFrameStride = 0x14;

    /// <summary>
    /// Index of the Y-offset/radius pair an activity-frame routine selects. Each routine selects
    /// the pair matching its position, so the index derives from the executed routine address
    /// (frame zero's six extra bytes fall below one stride) rather than another 22-arm mapping.
    /// </summary>
    internal static int ActivityFrameIndex(HibashiInstruction instruction) =>
        ((ushort)instruction - (ushort)HibashiInstruction.ActivityFrame0) / ActivityFrameStride;
}
