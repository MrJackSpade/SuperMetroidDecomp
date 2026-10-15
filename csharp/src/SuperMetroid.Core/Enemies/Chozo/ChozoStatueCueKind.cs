namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>What a cue of the Chozo statue's programs does: one per bank-$AA instruction its lists use.</summary>
internal enum ChozoStatueCueKind
{
    /// <summary>Clears the pre-instruction (<c>Instruction_CommonAA3_SetEnemy0FB2ToRTS</c>, $AA:8074).</summary>
    ClearPreInstruction,
    /// <summary>Clears spikes under the operand's footstep offset (<c>Instruction_Chozo_SpawnChozoSpikeClearingFootstepProjectile</c>, $AA:E58F).</summary>
    Footstep,
    /// <summary>Moves the statue by the operand's carry-motion entry and carries Samus (<c>Instruction_Chozo_Movement_IndexInY</c>, $AA:E5D8).</summary>
    Move,
    /// <summary>Plays the footstep sound (<c>Instruction_Chozo_PlayChozoFootstepsSFX</c>, $AA:E587).</summary>
    PlayFootstepSound,
    /// <summary>Plays the grab sound (<c>Instruction_Chozo_PlayChozoGrabsSamusSFX</c>, $AA:E57F).</summary>
    PlayGrabSound,
    /// <summary>Releases Samus and blocks the slope (<c>Instruction_Chozo_ReleaseSamus_BlockSlopeAccess</c>, $AA:E6F0).</summary>
    ReleaseSamus,
    /// <summary>Sets the acid's lowered height (<c>Instruction_Chozo_SetLoweredAcidPosition</c>, $AA:E436).</summary>
    SetLoweredAcidPosition,
    /// <summary>Sets the pre-instruction main AI runs (<c>Instruction_CommonAA_Enemy0FB2_InY</c>, $AA:806B).</summary>
    SetPreInstruction,
    /// <summary>Starts lowering the acid (<c>Instruction_Chozo_StartLoweringAcid</c>, $AA:E429).</summary>
    StartLoweringAcid,
    /// <summary>Gives Samus her controls back (<c>Instruction_Chozo_UnlockSamus</c>, $AA:E43D).</summary>
    UnlockSamus,
}
