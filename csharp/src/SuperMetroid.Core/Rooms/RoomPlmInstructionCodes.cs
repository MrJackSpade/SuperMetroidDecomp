namespace SuperMetroid.Core.Rooms;

/// <summary>Translated bank-$84 PLM instruction routine offsets.</summary>
/// <remarks>
/// A PLM instruction word is a native subroutine address, not an enum ordinal. Keeping the
/// catalog as named constants preserves lossless pointer comparisons while separating ROM
/// identity from the interpreter that executes it.
/// </remarks>
public static class RoomPlmInstructionCodes
{
    /// <summary><c>$84:86B4 Instruction_PLM_Sleep</c>: leave the list on this word.</summary>
    public const ushort Sleep = 0x86b4;

    /// <summary><c>$84:86BC Instruction_PLM_Delete</c>: release the current PLM slot.</summary>
    public const ushort Delete = 0x86bc;

    /// <summary><c>$84:86C1 Instruction_PLM_PreInstruction</c>: install a bank-$84 callback.</summary>
    public const ushort InstallPreInstruction = 0x86c1;

    /// <summary><c>$84:86CA Instruction_PLM_ClearPreInstruction</c>.</summary>
    public const ushort ClearPreInstruction = 0x86ca;

    /// <summary><c>$84:8A24 Instruction_PLM_LinkInstruction_Y</c>.</summary>
    public const ushort LinkInstruction = 0x8a24;

    /// <summary><c>$84:8724 Instruction_PLM_GotoY</c>: replace the list cursor.</summary>
    public const ushort Goto = 0x8724;

    /// <summary><c>$84:873F Instruction_PLM_DecTimer_GotoYIfNonZero</c>.</summary>
    public const ushort DecrementTimerAndGoto = 0x873f;

    /// <summary><c>$84:874E Instruction_PLM_SetTimer8Bit</c>.</summary>
    public const ushort SetEightBitTimer = 0x874e;

    /// <summary><c>$84:8764 Instruction_PLM_LoadItemPLMGfx</c>: load an item's graphics set.</summary>
    public const ushort LoadItemGraphics = 0x8764;

    /// <summary><c>$84:87E5 Instruction_PLM_CopyFromRamToVram</c>.</summary>
    public const ushort CopyFromRamToVram = 0x87e5;

    /// <summary><c>$84:880E Instruction_PLM_GotoYIfBossBitSet</c>.</summary>
    public const ushort GotoIfAreaBossBitSet = 0x880e;

    /// <summary><c>$84:882D Instruction_PLM_GotoYIfEventSet</c>.</summary>
    public const ushort GotoIfEventSet = 0x882d;

    /// <summary><c>$84:883E Instruction_PLM_SetEvent</c>.</summary>
    public const ushort SetEvent = 0x883e;

    /// <summary><c>$84:8AF1 Instruction_PLM_PLMBTS_Y</c>: copy one byte into the origin BTS.</summary>
    public const ushort SetPlmBtsFromByte = 0x8af1;

    /// <summary><c>$84:8B17 Instruction_PLM_DrawPLMBlock</c>.</summary>
    public const ushort DrawPlmBlock = 0x8b17;

    /// <summary><c>$84:8C10 Instruction_PLM_QueueSound_Y_Lib2_Max6</c>.</summary>
    public const ushort QueueSoundLibrary2Maximum6 = 0x8c10;

    /// <summary><c>$84:8C19 Instruction_PLM_QueueSound_Y_Lib3_Max6</c>.</summary>
    public const ushort QueueSoundLibrary3Maximum6 = 0x8c19;

    /// <summary><c>$84:8C46 Instruction_PLM_QueueSound_Y_Lib2_Max3</c>.</summary>
    public const ushort QueueSoundLibrary2Maximum3 = 0x8c46;

    /// <summary><c>$84:8C79 Instruction_PLM_QueueSound_Y_Lib2_Max1</c>.</summary>
    public const ushort QueueSoundLibrary2Maximum1 = 0x8c79;

    /// <summary>Direct-entry form of the max-one library-two queue routine at <c>$84:8C7C</c>.</summary>
    public const ushort QueueSoundLibrary2Maximum1Direct = 0x8c7c;

    /// <summary><c>$84:AB51</c>: set Botwoon's first two scroll cells blue.</summary>
    public const ushort SetBotwoonScrollsBlue = 0xab51;

    /// <summary><c>$84:AB59</c>: move Botwoon's PLM origin down one room-block row.</summary>
    public const ushort MoveBotwoonPlmDownOneBlock = 0xab59;

    /// <summary><c>$84:CD93</c>: replace the current PLM block's BTS byte with one.</summary>
    public const ushort SetPlmBtsToOne = 0xcd93;

    /// <summary><c>$84:D2F9</c>: branch while the PLM room argument is below an operand.</summary>
    public const ushort GotoIfRoomArgumentLess = 0xd2f9;

    /// <summary><c>$84:D30B</c>: spawn Mother Brain's four glass-shard projectiles.</summary>
    public const ushort SpawnFourMotherBrainGlassShards = 0xd30b;

    /// <summary><c>$84:D357</c>: spawn one Bomb Torizo statue-breaking projectile.</summary>
    public const ushort SpawnTorizoStatueBreaking = 0xd357;

    /// <summary><c>$84:D3C7</c>: queue Bomb Torizo's track-one music command.</summary>
    public const ushort QueueSongOneMusicTrack = 0xd3c7;

    /// <summary><c>$84:D525 Instruction_PLM_EnableWaterPhysics</c>.</summary>
    public const ushort EnableNoobTubeWaterPhysics = 0xd525;

    /// <summary><c>$84:D52C Instruction_PLM_SpawnNoobTubeCrackEnemyProjectile</c>.</summary>
    public const ushort SpawnNoobTubeCrack = 0xd52c;

    /// <summary><c>$84:D536 Instruction_PLM_TriggerNoobTubeEarthquake</c>.</summary>
    public const ushort TriggerNoobTubeEarthquake = 0xd536;

    /// <summary><c>$84:D543</c>: spawn ten tube shards and six released-air bubbles.</summary>
    public const ushort SpawnNoobTubeShardsAndBubbles = 0xd543;

    /// <summary><c>$84:D5E6 Instruction_PLM_LockSamus</c>.</summary>
    public const ushort LockSamus = 0xd5e6;

    /// <summary><c>$84:D5EE Instruction_PLM_UnlockSamus</c>.</summary>
    public const ushort UnlockSamus = 0xd5ee;
}
