namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 enemy-projectile instructions executed by the shared projectile interpreter; Tourian unlock instructions are handled before it.</summary>
internal enum EnemyProjectileInstruction : ushort
{
    /// <summary><c>Instruction_EnemyProjectile_Delete</c> at $86:8154. Delete.</summary>
        Delete = 0x8154,

    /// <summary><c>Instruction_EnemyProjectile_Properties_OrY</c> at $86:8230. OR packed projectile properties with one literal word.</summary>
        Properties_OrY = 0x8230,

    /// <summary><c>Instruction_EnemyProjectile_Properties_AndY</c> at $86:823C. AND packed projectile properties with one literal word.</summary>
        Properties_AndY = 0x823c,

    /// <summary><c>UNUSED_Inst_EnemyProj_EnableCollisionWithSamusProj_868248</c> at $86:8248. Enable collision with Samus projectiles.</summary>
        UNUSED_Inst_EnemyProj_EnableCollisionWithSamusProj_868248 = 0x8248,

    /// <summary><c>Instruction_EnemyProjectile_DisableCollisionWIthSamusProj</c> at $86:8252. Disable collision with Samus projectiles.</summary>
        DisableCollisionWIthSamusProj = 0x8252,

    /// <summary><c>Instruction_EnemyProjectile_DisableCollisionWithSamus</c> at $86:825C. Disable collision with Samus.</summary>
        DisableCollisionWithSamus = 0x825c,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_EnableCollisionWithSamus_868266</c> at $86:8266. Enable collision with Samus.</summary>
        UNUSED_Inst_EnemyProjectile_EnableCollisionWithSamus_868266 = 0x8266,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_SetToNotDieOnContact_868270</c> at $86:8270. Retain actor after Samus contact.</summary>
        UNUSED_Inst_EnemyProjectile_SetToNotDieOnContact_868270 = 0x8270,

    /// <summary><c>UNUSED_Instruction_EnemyProjectile_SetToDieOnContact_86827A</c> at $86:827A. Delete actor after Samus contact.</summary>
        UNUSED_Instruction_EnemyProjectile_SetToDieOnContact_86827A = 0x827a,

    /// <summary><c>Instruction_EnemyProjectile_SetHighPriority</c> at $86:8284. Set property bit $1000 and select the pre-Samus draw pass.</summary>
        SetHighPriority = 0x8284,

    /// <summary><c>UNUSED_Instruction_EnemyProjectile_SetLowPriority_86828E</c> at $86:828E. Clear property bit $1000 and select the post-Samus draw pass.</summary>
        UNUSED_Instruction_EnemyProjectile_SetLowPriority_86828E = 0x828e,

    /// <summary><c>Instruction_EnemyProjectile_XYRadiusInY</c> at $86:8298. Set packed {X,Y} collision radii.</summary>
        XYRadiusInY = 0x8298,

    /// <summary><c>UNUSED_Instruction_EnemyProjectile_XYRadius_0</c> at $86:82A1. Clear both collision radii.</summary>
        UNUSED_Instruction_EnemyProjectile_XYRadius_0 = 0x82a1,

    /// <summary><c>UNUSED_Instruction_EnemyProjectile_QueueMusicTrackInY</c> at $86:82FD. Queue music with eight-frame delay; one-byte operand.</summary>
        UNUSED_Instruction_EnemyProjectile_QueueMusicTrackInY = 0x82fd,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max6_868309</c> at $86:8309. Queue SFX library 1, maximum 6.</summary>
        UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max6_868309 = 0x8309,

    /// <summary><c>Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6</c> at $86:8312. Queue SFX library 2, maximum 6.</summary>
        QueueSoundInY_Lib2_Max6 = 0x8312,

    /// <summary><c>Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max6</c> at $86:831B. Queue SFX library 3, maximum 6.</summary>
        QueueSoundInY_Lib3_Max6 = 0x831b,

    /// <summary><c>Instruction_EnemyProjectile_QueueSoundInY_Lib1_Max15</c> at $86:8324. Queue SFX library 1, maximum 15.</summary>
        QueueSoundInY_Lib1_Max15 = 0x8324,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max15_86832D</c> at $86:832D. Queue SFX library 2, maximum 15.</summary>
        UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max15_86832D = 0x832d,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max15_868336</c> at $86:8336. Queue SFX library 3, maximum 15.</summary>
        UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max15_868336 = 0x8336,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max3_86833F</c> at $86:833F. Queue SFX library 1, maximum 3.</summary>
        UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max3_86833F = 0x833f,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max3_868348</c> at $86:8348. Queue SFX library 2, maximum 3.</summary>
        UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max3_868348 = 0x8348,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max3_868351</c> at $86:8351. Queue SFX library 3, maximum 3.</summary>
        UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max3_868351 = 0x8351,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86835A</c> at $86:835A. Queue SFX library 1, maximum 9.</summary>
        UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86835A = 0x835a,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max9_868363</c> at $86:8363. Queue SFX library 2, maximum 9.</summary>
        UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max9_868363 = 0x8363,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86836C</c> at $86:836C. Queue SFX library 3, maximum 9.</summary>
        UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86836C = 0x836c,

    /// <summary><c>UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max1_868375</c> at $86:8375. Queue SFX library 1, maximum 1.</summary>
        UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max1_868375 = 0x8375,

    /// <summary><c>Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max1</c> at $86:837E. Queue SFX library 2, maximum 1.</summary>
        QueueSoundInY_Lib2_Max1 = 0x837e,

    /// <summary><c>Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max1</c> at $86:8387. Queue SFX library 3, maximum 1.</summary>
        QueueSoundInY_Lib3_Max1 = 0x8387,

    /// <summary><c>Instruction_EnemyProjectile_Sleep</c> at $86:8159. Sleep forever while pre-instruction movement remains active.</summary>
        Sleep = 0x8159,

    /// <summary><c>Instruction_EnemyProjectile_PreInstructionInY</c> at $86:8161. Install the operand as pre-instruction.</summary>
        PreInstructionInY = 0x8161,

    /// <summary><c>Instruction_EnemyProjectile_CalculateDirectionTowardsSamus</c> at $86:82A5.</summary>
        CalculateDirectionTowardsSamus = 0x82a5,

    /// <summary><c>Instruction_EnemyProjectile_ClearPreInstruction</c> at $86:816A. Clear pre-instruction to $8170 RTS.</summary>
        ClearPreInstruction = 0x816a,

    /// <summary>Instruction <c>$86:E533</c>, which consumes one signed Y-velocity word.</summary>
        DownwardGateSetYVelocityInstruction = 0xe533,

    /// <summary><c>Instruction_EnemyProjectile_CallExternalFunctionInY</c> at $86:8171. Call the following 24-bit external function.</summary>
        CallExternalFunctionInY = 0x8171,

    /// <summary><c>Instruction_SetPreInst_DraygonsWallTurretProjectile_Fired</c> at $86:8CF6.</summary>
        SetPreInst_DraygonsWallTurretProjectile_Fired = 0x8cf6,

    /// <summary><c>Instruction_DraygonGoop_SamusCollision</c> at $86:8D99.</summary>
        DraygonGoop_SamusCollision = 0x8d99,

    /// <summary><c>Instruction_EnemyProjectile_GotoY</c> at $86:81AB. Same-bank goto.</summary>
        GotoY = 0x81ab,

    /// <summary><c>Instruction_EnemyProjectile_GotoY_Y</c> at $86:81B0. Signed-byte same-bank relative goto.</summary>
        GotoY_Y = 0x81b0,

    /// <summary><c>Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero</c> at $86:81C6. Decrement general timer and take an absolute branch while nonzero.</summary>
        DecrementTimer_GotoYIfNonZero = 0x81c6,

    /// <summary><c>UNUSED_Inst_EnemyProj_DecrementTimer_GotoY_YIfNonZero_8681CE</c> at $86:81CE. Decrement general timer and take a signed relative branch while nonzero.</summary>
        UNUSED_Inst_EnemyProj_DecrementTimer_GotoY_YIfNonZero_8681CE = 0x81ce,

    /// <summary><c>Instruction_EnemyProjectile_TimerInY</c> at $86:81D5. Initialize the independent general-purpose loop timer.</summary>
        TimerInY = 0x81d5,

    /// <summary><c>$86:D5E1</c>: assign a random falling angle to a n00b-tube shard.</summary>
        NoobTubeShardAssignFallingAngle = 0xd5e1,

    /// <summary><c>$86:D69A</c>: assign a random falling angle to a released-air bubble.</summary>
        NoobTubeBubbleAssignFallingAngle = 0xd69a,

    /// <summary><c>$86:D5F2</c>: mirror/flicker a two-sided n00b-tube shard frame.</summary>
        NoobTubeShardReflectFlicker = 0xd5f2,

    /// <summary><c>$86:D62A</c>: flicker a one-sided n00b-tube shard frame.</summary>
        NoobTubeShardFlicker = 0xd62a,

    /// <summary><c>RTS_8681DE</c> at $86:81DE. Deliberate entry at the RTS immediately before $81DF.</summary>
        RTS_8681DE = 0x81de,

    /// <summary><c>Instruction_MoveRandomlyWithinXRadius_YRadius</c> at $86:81DF. Randomly offset the actor inside authored X/Y radii.</summary>
        MoveRandomlyWithinXRadius_YRadius = 0x81df,

    /// <summary><c>Instruction_PreInstructionInY_ExecuteY</c> at $86:A050. Pirate laser: install operand as pre-instruction and run it.</summary>
        PreInstructionInY_ExecuteY = 0xa050,

    /// <summary><c>Instruction_EnemyProjectile_Torizo_ResetPosition</c> at $86:A3BE. Restore X/Y saved by the sonic-boom collision pre-instruction.</summary>
        Torizo_ResetPosition = 0xa3be,

    /// <summary><c>UNUSED_Instruction_EnemyProj_MoveHorizontally_GotoY_86AD92</c> at $86:AD92. Move X, then choose one of two lists from velocity sign.</summary>
        UNUSED_Instruction_EnemyProj_MoveHorizontally_GotoY_86AD92 = 0xad92,

    /// <summary><c>Instruction_EnemyProjectile_GoldenTorizoEgg_GoToHatched</c> at $86:B13E. Golden Torizo egg: select left/right terminal list.</summary>
        GoldenTorizoEgg_GoToHatched = 0xb13e,

    /// <summary><c>Instruction_AimSuperMissile_Rightwards</c> at $86:B269. Golden Torizo super missile: velocity toward Samus.</summary>
        AimSuperMissile_Rightwards = 0xb269,

    /// <summary><c>Instruction_AimSuperMissile_Leftwards</c> at $86:B272. Golden Torizo super missile: velocity away from Samus.</summary>
        AimSuperMissile_Leftwards = 0xb272,

    /// <summary><c>Instruction_EnemyProjectile_GotoYIfEyeBeamExplosionsDisabled</c> at $86:B3B8. Golden Torizo eye beam: branch while attack flag is clear.</summary>
        GotoYIfEyeBeamExplosionsDisabled = 0xb3b8,

    /// <summary><c>UNUSED_Instruction_ResetPosition_86B436</c> at $86:B436. Restore X/Y saved in generic projectile variables E/F.</summary>
        UNUSED_Instruction_ResetPosition_86B436 = 0xb436,

    /// <summary><c>Instruction_EnemyProjectile_MotherBrainsTurretBullets_GotoY</c> at $86:C173. The bullet initializer stores direction * 2 in variable E. The ROM</summary>
        MotherBrainsTurretBullets_GotoY = 0xc173,

    /// <summary><c>Instruction_EnemyProjectile_GotoY_Probability_1_4</c> at $86:A456. Take the authored absolute branch with 25-percent probability.</summary>
        GotoY_Probability_1_4 = 0xa456,

    /// <summary><c>Instruction_EnemyProjectile_SpawnEnemyDropsWIthYDropChances</c> at $86:AB8A. Shot Torizo orb: choose area-specific header/drop table.</summary>
        SpawnEnemyDropsWIthYDropChances = 0xab8a,

    /// <summary><c>Instruction_Spawn_HorizontalAfterburn_EnemyProjectiles</c> at $86:95BA. Spawn horizontal right/left afterburn pair.</summary>
        Spawn_HorizontalAfterburn_EnemyProjectiles = 0x95ba,

    /// <summary><c>Instruction_Spawn_VerticalAfterburn_EnemyProjectiles</c> at $86:95ED. Spawn vertical up/down afterburn pair.</summary>
        Spawn_VerticalAfterburn_EnemyProjectiles = 0x95ed,

    /// <summary><c>Instruction_SpawnNext_Afterburn_EnemyProjectile</c> at $86:9620. Decrement count and spawn the next actor in this direction.</summary>
        SpawnNext_Afterburn_EnemyProjectile = 0x9620,

    /// <summary><c>Instruction_SpawnPhantoonDrop</c> at $86:980E. Shot Phantoon flame: request a drop from eye header $E4FF.</summary>
        SpawnPhantoonDrop = 0x980e,

    /// <summary>$86:8C68, EprojInstr_SpawnEnemyDropsWithDraygonsEyeDrops: spawn at projectile position using enemy header $A0:DE7F; no operands.</summary>
        SpawnEnemyDropsWithDraygonEyeChances = 0x8c68,

    /// <summary>$86:9270, Instruction_SpawnEnemyDropsWithCrocomiresDropChances: spawn at projectile position using enemy header $DDBF; no operands.</summary>
        SpawnEnemyDropsWithCrocomireChances = 0x9270,

    /// <summary>
    /// <c>Instruction_EnemyProjectile_KagoBug_StartJumping</c> at $86:D15C.
    /// </summary>
        KagoBugStartJumpInstruction = 0xd15c,

    /// <summary>
    /// <c>Instruction_EnemyProjectile_KagoBug_StartIdling</c> at $86:D1B6.
    /// </summary>
        KagoBugStartIdleInstruction = 0xd1b6,

    /// <summary>
    /// <c>Instruction_EnemyProjectile_UsePalette0_duplicate_again</c> at $86:D1C7.
    /// </summary>
        KagoBugUsePaletteZeroInstruction = 0xd1c7,

    /// <summary>
    /// <c>PreInstruction_EnemyProjectile_KagoBug_SpawnDrop</c> used as an instruction
    /// callback at $86:D1CE.
    /// </summary>
        KagoBugSpawnDropInstruction = 0xd1ce,

    /// <summary>
    /// <c>Instruction_EnemyProjectile_MagdolliteFlame_SpawnDrops</c> at $86:DFEA.
    /// Spawns an enemy drop using Magdollite's enemy header.
    /// </summary>
        MagdolliteFlame_SpawnDrops = 0xdfea,

    /// <summary><c>Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_20</c> at $86:ECE3. Random sprite-object position inside a 64x64 square.</summary>
        EnemyDeathExpl_SpawnSpriteObjectInY_20 = 0xece3,

    /// <summary><c>Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_10</c> at $86:ED17. Random sprite-object position inside a 32x32 square.</summary>
        EnemyDeathExpl_SpawnSpriteObjectInY_10 = 0xed17,

    /// <summary><c>Instruction_EnemyProj_EnemyDeathExpl_QueueEnemyKilledSoundFX</c> at $86:EE8B. Queue sound 9 in library two; this opcode has no operand.</summary>
        EnemyDeathExpl_QueueEnemyKilledSoundFX = 0xee8b,

    /// <summary><c>Instruction_EnemyProj_EDeathExplo_QueueSmallExplosionSoundFX</c> at $86:EE97. Queue sound $24 in library two; no operand.</summary>
        EDeathExplo_QueueSmallExplosionSoundFX = 0xee97,

    /// <summary><c>Instruction_EnemyProj_EDeathExplo_QueueContactKilledSoundFX</c> at $86:EEA3. Queue sound $0B in library two; no operand.</summary>
        EDeathExplo_QueueContactKilledSoundFX = 0xeea3,

    /// <summary><c>Instruction_EnemyProjectile_Spores_SetProperties3000</c> at $86:DC5A. Spore impact: replace packed properties with literal $3000.</summary>
        Spores_SetProperties3000 = 0xdc5a,

    /// <summary><c>Instruction_EnemyProjectile_Spores_SpawnEnemyDrops</c> at $86:DC61. Spore impact: request the stalk header $DF7F's drop table.</summary>
        Spores_SpawnEnemyDrops = 0xdc61,

    /// <summary><c>Instruction_EnemyProjectile_SporeSpawner_SpawnSpore</c> at $86:DC77. Ceiling spawner: allocate one room-graphics spore here.</summary>
        SporeSpawner_SpawnSpore = 0xdc77,

    /// <summary><c>Instruction_EnemyProjectile_TorizoLandingDustClouds</c> at $86:AF92. Torizo landing-dust instruction: move actor four pixels up.</summary>
        TorizoLandingDustClouds = 0xaf92,

    /// <summary><c>Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup</c> at $86:EEAF. Random drop selection after an enemy death animation.</summary>
        EnemyDeathExplosion_BecomePickup = 0xeeaf,

    /// <summary><c>Instruction_EnemyProjectile_Pickup_HandleRespawningEnemy</c> at $86:EF10. Respawn the retained physical enemy slot, when bit $8000 is set.</summary>
        Pickup_HandleRespawningEnemy = 0xef10,
    /// <summary>
    /// <c>Instruction_EnemyProjectile_MotherBrainPurpleBreath_Inactive</c> at $86:CAEE: clears
    /// <c>MotherBrainBody.smallPurpleBreathActiveFlag</c> before the small breath deletes itself.
    /// </summary>
    MotherBrainPurpleBreath_Inactive = 0xcaee,
    /// <summary><c>Instruction_EnemyProjectile_UsePalette0</c> at $86:C1B4. First instruction of the shared touch/shot smoke sequence.</summary>
    UsePalette0 = 0xc1b4,
    /// <summary><c>Instruction_EnemyProjectile_UsePalette0_duplicate</c> at $86:C42E. Mother Brain onion-ring impact switches to palette zero.</summary>
    UsePalette0_Duplicate = 0xc42e,
    /// <summary><c>Instruction_EnemyProj_MotherBrainsDrool_MoveDownCPixels</c> at $86:C8D0. After changing to the falling pre-instruction, the list lowers the</summary>
    MotherBrainsDrool_MoveDownCPixels = 0xc8d0,
}
