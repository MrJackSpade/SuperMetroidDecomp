namespace SuperMetroid.Core.Rooms;

/// <summary>Translated bank-$84 PLM instruction routines.</summary>
/// <remarks>
/// Each member's value is the native subroutine address stored in instruction lists, so
/// streams keep their lossless words while dispatch decodes them into this closed set.
/// </remarks>
public enum RoomPlmInstruction : ushort
{
    /// <summary><c>$84:BE3F Instruction_PLM_SetGreyDoorPreInstruction</c>: typed resident-door owner installs its room-argument condition callback; no operands.</summary>
    SetGreyDoorPreInstruction = 0xbe3f,
    /// <summary><c>$84:86B4 Instruction_PLM_Sleep</c>: leave the list on this word.</summary>
    Sleep = 0x86b4,

    /// <summary><c>$84:86BC Instruction_PLM_Delete</c>: release the current PLM slot.</summary>
    Delete = 0x86bc,

    /// <summary><c>$84:86C1 Instruction_PLM_PreInstruction</c>: install a bank-$84 callback.</summary>
    InstallPreInstruction = 0x86c1,

    /// <summary><c>$84:86CA Instruction_PLM_ClearPreInstruction</c>.</summary>
    ClearPreInstruction = 0x86ca,

    /// <summary><c>$84:8A24 Instruction_PLM_LinkInstruction_Y</c>.</summary>
    LinkInstruction = 0x8a24,

    /// <summary><c>$84:8A72</c>: branch when the slot's persistent door bit is set.</summary>
    GotoIfDoorBitSet = 0x8a72,

    /// <summary><c>$84:8A91</c>: increment the door-hit byte and branch at its threshold.</summary>
    IncrementDoorHitCounterAndGoto = 0x8a91,

    /// <summary><c>$84:8ACD</c>: increment the room-argument byte and branch at a threshold.</summary>
    IncrementArgumentAndGotoIfGreaterOrEqual = 0x8acd,

    /// <summary><c>$84:8D41</c>: branch when Samus is within the operand block rectangle.</summary>
    GotoIfSamusNear = 0x8d41,

    /// <summary><c>$84:8724 Instruction_PLM_GotoY</c>: replace the list cursor.</summary>
    Goto = 0x8724,

    /// <summary><c>$84:873F Instruction_PLM_DecTimer_GotoYIfNonZero</c>.</summary>
    DecrementTimerAndGoto = 0x873f,

    /// <summary><c>$84:874E Instruction_PLM_SetTimer8Bit</c>.</summary>
    SetEightBitTimer = 0x874e,

    /// <summary><c>$84:ABD6 Instruction_PLM_MovePLMRight1Block</c>: increment the native block byte index twice.</summary>
    MoveRightOneBlock = 0xabd6,

    /// <summary><c>$84:87E5 Instruction_PLM_CopyFromRamToVram</c>.</summary>
    CopyFromRamToVram = 0x87e5,

    /// <summary><c>$84:880E Instruction_PLM_GotoYIfBossBitSet</c>.</summary>
    GotoIfAreaBossBitSet = 0x880e,

    /// <summary><c>$84:882D Instruction_PLM_GotoYIfEventSet</c>.</summary>
    GotoIfEventSet = 0x882d,

    /// <summary><c>$84:883E Instruction_PLM_SetEvent</c>.</summary>
    SetEvent = 0x883e,

    /// <summary><c>$84:8AF1 Instruction_PLM_PLMBTS_Y</c>: copy one byte into the origin BTS.</summary>
    SetPlmBtsFromByte = 0x8af1,

    /// <summary><c>$84:8B17 Instruction_PLM_DrawPLMBlock</c>.</summary>
    DrawPlmBlock = 0x8b17,

    /// <summary><c>$84:8B05 Instruction_PLM_DrawPLMBlock_Clone</c>: alternate entry that joins the same one-block draw and one-frame yield tail as $8B17.</summary>
    DrawPlmBlockClone = 0x8b05,

    /// <summary><c>$84:8C10 Instruction_PLM_QueueSound_Y_Lib2_Max6</c>.</summary>
    QueueSoundLibrary2Maximum6 = 0x8c10,

    /// <summary><c>$84:8C19 Instruction_PLM_QueueSound_Y_Lib3_Max6</c>.</summary>
    QueueSoundLibrary3Maximum6 = 0x8c19,

    /// <summary><c>$84:BBDD</c>: clear the downward gate's trigger word.</summary>
    ClearDownwardGateTrigger = 0xbbdd,

    /// <summary><c>$84:BBE1</c>: spawn the following gate projectile definition.</summary>
    SpawnDownwardGateProjectile = 0xbbe1,

    /// <summary><c>$84:BBF0</c>: wake the gate projectile associated with this PLM.</summary>
    WakeDownwardGateProjectile = 0xbbf0,

    /// <summary><c>$84:D77A</c>: spawn an eye-door attack with the following parameter.</summary>
    ShootEyeDoorProjectile = 0xd77a,

    /// <summary><c>$84:D790</c>: spawn an eye-door sweat drop with the following parameter.</summary>
    SpawnEyeDoorSweat = 0xd790,

    /// <summary><c>$84:D79F</c>: spawn two randomized eye-door smoke actors.</summary>
    SpawnTwoEyeDoorSmoke = 0xd79f,

    /// <summary><c>$84:D7B6</c>: spawn one centered eye-door smoke actor.</summary>
    SpawnEyeDoorSmoke = 0xd7b6,

    /// <summary><c>$84:D7C3</c>: move up and construct a right-facing blue-door cap.</summary>
    MoveUpAndMakeBlueDoorFacingRight = 0xd7c3,

    /// <summary><c>$84:D7DA</c>: move up and construct a left-facing blue-door cap.</summary>
    MoveUpAndMakeBlueDoorFacingLeft = 0xd7da,

    /// <summary><c>$84:DB8E</c>: disable and replace a right-facing Draygon cannon.</summary>
    DamageDraygonCannonFacingRight = 0xdb8e,

    /// <summary><c>$84:DC36</c>: disable and replace a left-facing Draygon cannon.</summary>
    DamageDraygonCannonFacingLeft = 0xdc36,

    /// <summary><c>$84:8C46 Instruction_PLM_QueueSound_Y_Lib2_Max3</c>.</summary>
    QueueSoundLibrary2Maximum3 = 0x8c46,

    /// <summary><c>$84:8C79 Instruction_PLM_QueueSound_Y_Lib2_Max1</c>.</summary>
    QueueSoundLibrary2Maximum1 = 0x8c79,

    /// <summary>Direct-entry form of the max-one library-two queue routine at <c>$84:8C7C</c>.</summary>
    QueueSoundLibrary2Maximum1Direct = 0x8c7c,

    /// <summary><c>$84:AB51</c>: set Botwoon's first two scroll cells blue.</summary>
    SetBotwoonScrollsBlue = 0xab51,

    /// <summary><c>$84:AB59</c>: move Botwoon's PLM origin down one room-block row.</summary>
    MoveBotwoonPlmDownOneBlock = 0xab59,

    /// <summary>
    /// <c>$84:BA6F PlmInstr_JumpIfSamusHasNoBombs</c>: branch through the following
    /// instruction-list pointer when the Bombs item has not been collected.
    /// </summary>
    GotoIfSamusHasNoBombs = 0xba6f,

    /// <summary><c>$84:CD93</c>: replace the current PLM block's BTS byte with one.</summary>
    SetPlmBtsToOne = 0xcd93,

    /// <summary><c>$84:D2F9</c>: branch while the PLM room argument is below an operand.</summary>
    GotoIfRoomArgumentLess = 0xd2f9,

    /// <summary><c>$84:D30B</c>: spawn Mother Brain's four glass-shard projectiles.</summary>
    SpawnFourMotherBrainGlassShards = 0xd30b,

    /// <summary><c>$84:D357</c>: spawn one Bomb Torizo statue-breaking projectile.</summary>
    SpawnTorizoStatueBreaking = 0xd357,

    /// <summary><c>$84:D3C7</c>: queue Bomb Torizo's track-one music command.</summary>
    QueueSongOneMusicTrack = 0xd3c7,

    /// <summary><c>$84:D525 Instruction_PLM_EnableWaterPhysics</c>.</summary>
    EnableNoobTubeWaterPhysics = 0xd525,

    /// <summary><c>$84:D52C Instruction_PLM_SpawnNoobTubeCrackEnemyProjectile</c>.</summary>
    SpawnNoobTubeCrack = 0xd52c,

    /// <summary><c>$84:D536 Instruction_PLM_TriggerNoobTubeEarthquake</c>.</summary>
    TriggerNoobTubeEarthquake = 0xd536,

    /// <summary><c>$84:D543</c>: spawn ten tube shards and six released-air bubbles.</summary>
    SpawnNoobTubeShardsAndBubbles = 0xd543,

    /// <summary><c>$84:D5E6 Instruction_PLM_LockSamus</c>.</summary>
    LockSamus = 0xd5e6,

    /// <summary><c>$84:D5EE Instruction_PLM_UnlockSamus</c>.</summary>
    UnlockSamus = 0xd5ee,

    /// <summary>
    /// $84:BB25, <c>Instruction_PLM_MovePLMRight4Blocks</c>: adds eight to the PLM's byte
    /// block index, four level words.
    /// </summary>
    MoveRightFourBlocks = 0xbb25,

    /// <summary>$84:B9B9 marks event 0F, allowing the animals to escape.</summary>
    SetAnimalsEscapedEvent = 0xb9b9,

    /// <summary>$84:AC9D, accumulate two whole points of periodic damage.</summary>
    SamusEaterDamage = 0xac9d,

    /// <summary>$84:ACB1, publish $30 immunity before releasing the position owner.</summary>
    SamusEaterReleaseImmunity = 0xacb1,

    /// <summary>$84:AB00 advances the six-row crumble PLM one row downward.</summary>
    MoveTourianAccessDown = 0xab00,

    /// <summary>$84:D3D7, replace the two Wrecked Ship spike slopes with ordinary slopes.</summary>
    TransformSpikesToSlopes = 0xd3d7,

    /// <summary>$84:D3F4, restore the same two blocks to spike collision.</summary>
    RevertSlopesToSpikes = 0xd3f4,

    /// <summary>$84:D155, restore the lowered acid's base height on room re-entry.</summary>
    SetLoweredAcidHeight = 0xd155,
}
