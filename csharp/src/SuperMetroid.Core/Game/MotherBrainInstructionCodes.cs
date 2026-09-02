namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A9 instruction entry points used by Mother Brain bytecode.</summary>
internal static class MotherBrainInstructionCodes
{
    /// <summary><c>Instruction_CommonA9_Sleep</c> at $A9:812F.</summary>
    public const ushort Instruction_CommonA9_Sleep = 0x812f;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyUpBy2_ScrollRightBy1</c> at $A9:95FC.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyUpBy2_ScrollRightBy1 = 0x95fc;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyUpBy10_ScrollLeftBy4</c> at $A9:95B6.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyUpBy10_ScrollLeftBy4 = 0x95b6;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyUpBy16_ScrollLeftBy4</c> at $A9:95C0.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyUpBy16_ScrollLeftBy4 = 0x95c0;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyUpBy12_ScrollRightBy2</c> at $A9:95CA.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyUpBy12_ScrollRightBy2 = 0x95ca;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyDownBy12_ScrollLeftBy4</c> at $A9:95DE.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyDownBy12_ScrollLeftBy4 = 0x95de;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyDownBy16_ScrollRightBy2</c> at $A9:95E8.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyDownBy16_ScrollRightBy2 = 0x95e8;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyDownBy10_ScrollRightBy2</c> at $A9:95F2.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyDownBy10_ScrollRightBy2 = 0x95f2;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyRightBy2</c> at $A9:960C.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyRightBy2 = 0x960c;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyUpBy1</c> at $A9:961C.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyUpBy1 = 0x961c;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyUpBy1_RightBy3_Footstep</c> at $A9:9622.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyUpBy1_RightBy3_Footstep = 0x9622;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy15</c> at $A9:9638.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy15 = 0x9638;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy6</c> at $A9:9648.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy6 = 0x9648;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy2</c> at $A9:9658.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy2 = 0x9658;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep</c> at $A9:9668.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep = 0x9668;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep_d</c> at $A9:967E.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep_d = 0x967e;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyLeftBy2</c> at $A9:9694.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyLeftBy2 = 0x9694;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyDownBy1</c> at $A9:96A4.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyDownBy1 = 0x96a4;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyDownBy1_LeftBy3</c> at $A9:96AA.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyDownBy1_LeftBy3 = 0x96aa;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy15_Footstep</c> at $A9:96BA.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy15_Footstep = 0x96ba;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy6</c> at $A9:96D0.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy6 = 0x96d0;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy2</c> at $A9:96E0.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy2 = 0x96e0;

    /// <summary><c>Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy1</c> at $A9:96F0.</summary>
    public const ushort Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy1 = 0x96f0;

    /// <summary><c>Instruction_MotherBrainBody_SetPoseToStanding</c> at $A9:9700.</summary>
    public const ushort Instruction_MotherBrainBody_SetPoseToStanding = 0x9700;

    /// <summary><c>Instruction_MotherBrainBody_SetPoseToWalking</c> at $A9:9708.</summary>
    public const ushort Instruction_MotherBrainBody_SetPoseToWalking = 0x9708;

    /// <summary><c>Instruction_MotherBrainBody_SetPoseToCrouching</c> at $A9:9710.</summary>
    public const ushort Instruction_MotherBrainBody_SetPoseToCrouching = 0x9710;

    /// <summary><c>Instruction_MotherBrainBody_SetPoseToCrouchingTransition</c> at $A9:9718.</summary>
    public const ushort Instruction_MotherBrainBody_SetPoseToCrouchingTransition = 0x9718;

    /// <summary><c>Instruction_MotherBrainBody_SetPoseToLeaningDown</c> at $A9:9728.</summary>
    public const ushort Instruction_MotherBrainBody_SetPoseToLeaningDown = 0x9728;

    /// <summary><c>Instruction_MotherBrainHead_IncBabyMetroidAttackCounter</c> at $A9:9EA3.</summary>
    public const ushort Instruction_MotherBrainHead_IncBabyMetroidAttackCounter = 0x9ea3;

    /// <summary><c>Instruction_MotherBrainHead_ResetBabyMetroidAttackCounter</c> at $A9:9EB5.</summary>
    public const ushort Instruction_MotherBrainHead_ResetBabyMetroidAttackCounter = 0x9eb5;

    /// <summary><c>Instruction_MotherBrainHead_DisableNeckMovement</c> at $A9:9B20.</summary>
    public const ushort Instruction_MotherBrainHead_DisableNeckMovement = 0x9b20;

    /// <summary><c>Instruction_MotherBrainHead_AimOnionRingsAtBabyMetroid</c> at $A9:9E37.</summary>
    public const ushort Instruction_MotherBrainHead_AimOnionRingsAtBabyMetroid = 0x9e37;

    /// <summary><c>Instruction_MotherBrainHead_AimOnionRingsAtSamus</c> at $A9:9E5B.</summary>
    public const ushort Instruction_MotherBrainHead_AimOnionRingsAtSamus = 0x9e5b;

    /// <summary><c>Instruction_MotherBrain_GotoX</c> at $A9:9B0F.</summary>
    public const ushort Instruction_MotherBrain_GotoX = 0x9b0f;

    /// <summary><c>Instruction_MotherBrainHead_EnableNeckMovement_GotoX</c> at $A9:9B14.</summary>
    public const ushort Instruction_MotherBrainHead_EnableNeckMovement_GotoX = 0x9b14;

    /// <summary><c>Instruction_MotherBrainHead_QueueBabyMetroidAttackSFX</c> at $A9:9DF7.</summary>
    public const ushort Instruction_MotherBrainHead_QueueBabyMetroidAttackSFX = 0x9df7;

    /// <summary><c>Instruction_MotherBrainHead_SpawnOnionRingsProjectile</c> at $A9:9E29.</summary>
    public const ushort Instruction_MotherBrainHead_SpawnOnionRingsProjectile = 0x9e29;

    /// <summary><c>Instruction_MotherBrainHead_QueueSoundX_Lib3_Max6</c> at $A9:9B32.</summary>
    public const ushort Instruction_MotherBrainHead_QueueSoundX_Lib3_Max6 = 0x9b32;

    /// <summary><c>Instruction_MotherBrainHead_QueueSoundX_Lib2_Max6</c> at $A9:9B28.</summary>
    public const ushort Instruction_MotherBrainHead_QueueSoundX_Lib2_Max6 = 0x9b28;

    /// <summary><c>Instruction_MotherBrainHead_SpawnBombProjectileWithParamX</c> at $A9:9EBD.</summary>
    public const ushort Instruction_MotherBrainHead_SpawnBombProjectileWithParamX = 0x9ebd;

    /// <summary><c>Instruction_MotherBrainHead_SpawnPurpleBreathBigProjectile</c> at $A9:9B6D.</summary>
    public const ushort Instruction_MotherBrainHead_SpawnPurpleBreathBigProjectile = 0x9b6d;

    /// <summary><c>Instruction_MotherBrainHead_MaybeGotoNeutralPhase3</c> at $A9:9D0D.</summary>
    public const ushort Instruction_MotherBrainHead_MaybeGotoNeutralPhase3 = 0x9d0d;

    /// <summary><c>Instruction_MotherBrainBody_SetPoseToDeathBeamMode</c> at $A9:9720.</summary>
    public const ushort Instruction_MotherBrainBody_SetPoseToDeathBeamMode = 0x9720;

    /// <summary><c>Instruction_MotherBrainHead_SpawnDroolProjectile</c> at $A9:9B3C.</summary>
    public const ushort Instruction_MotherBrainHead_SpawnDroolProjectile = 0x9b3c;

    /// <summary><c>Instruction_MotherBrainHead_SetMainShakeTimerTo50</c> at $A9:9B77.</summary>
    public const ushort Instruction_MotherBrainHead_SetMainShakeTimerTo50 = 0x9b77;

    /// <summary><c>Instruction_MotherBrainBody_SpawnDustCloudExplosionProj</c> at $A9:9AC8.</summary>
    public const ushort Instruction_MotherBrainBody_SpawnDustCloudExplosionProj = 0x9ac8;

    /// <summary><c>Instruction_MotherBrainBody_SpawnDeathBeamProjectile</c> at $A9:9AEF.</summary>
    public const ushort Instruction_MotherBrainBody_SpawnDeathBeamProjectile = 0x9aef;

    /// <summary><c>Instruction_MotherBrainBody_IncrementDeathBeamAttackPhase</c> at $A9:9B05.</summary>
    public const ushort Instruction_MotherBrainBody_IncrementDeathBeamAttackPhase = 0x9b05;

    /// <summary><c>Instruction_MotherBrainHead_MaybeGotoNeutralPhase2</c> at $A9:9CAD.</summary>
    public const ushort Instruction_MotherBrainHead_MaybeGotoNeutralPhase2 = 0x9cad;

    /// <summary><c>Instruction_MotherBrainHead_GotoDyingDroolInstList</c> at $A9:9C65.</summary>
    public const ushort Instruction_MotherBrainHead_GotoDyingDroolInstList = 0x9c65;

    /// <summary><c>InstList_MotherBrainHead_SpawnLaserProjectile</c> at $A9:9F46.</summary>
    public const ushort InstList_MotherBrainHead_SpawnLaserProjectile = 0x9f46;

    /// <summary><c>Instruction_MotherBrainHead_SpawnRainbowBeamChargingProj</c> at $A9:9F84.</summary>
    public const ushort Instruction_MotherBrainHead_SpawnRainbowBeamChargingProj = 0x9f84;

    /// <summary><c>Instruction_MotherBrainHead_SetupEffectsForRainbowBeamCharge</c> at $A9:9F8E.</summary>
    public const ushort Instruction_MotherBrainHead_SetupEffectsForRainbowBeamCharge = 0x9f8e;

    /// <summary><c>Instruction_BabyMetroid_GotoInitial</c> at $A9:CFB4.</summary>
    public const ushort Instruction_BabyMetroid_GotoInitial = 0xcfb4;

    /// <summary><c>Instruction_BabyMetroid_GotoDrainingMotherBrain</c> at $A9:CFCA.</summary>
    public const ushort Instruction_BabyMetroid_GotoDrainingMotherBrain = 0xcfca;

    /// <summary><c>Function_MotherBrainTubes_NonMainTube</c> at $A9:8B88.</summary>
    public const ushort Function_MotherBrainTubes_NonMainTube = 0x8b88;

    /// <summary><c>Function_MotherBrainTubes_MainTube_WaitingToFall</c> at $A9:8BCB.</summary>
    public const ushort Function_MotherBrainTubes_MainTube_WaitingToFall = 0x8bcb;

    /// <summary><c>Function_MotherBrainTubes_MainTube_Falling</c> at $A9:8BD6.</summary>
    public const ushort Function_MotherBrainTubes_MainTube_Falling = 0x8bd6;

}
