namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>What a cue of the Skree's and Metaree's programs does: one per bank-$A3 instruction its lists use.</summary>
internal enum SkreeMetareeCueKind
{
    /// <summary>Keeps processing the enemy while it is off screen (<c>CommonEnemyInstruction.EnableOffScreenProcessing</c>, $A0:8173).</summary>
    ProcessOffScreen,
    /// <summary>Tells main AI the wind-up is done and the dive may start (<c>Instruction_Metaree_SetAttackReadyFlag</c>, $A3:8956, and <c>Instruction_Skree_SetAttackReadyFlag</c>, $A3:C6A4).</summary>
    SetAttackReady,
    /// <summary>Stops processing the enemy while it is off screen (<c>CommonEnemyInstruction.DisableOffScreenProcessing</c>, $A0:817D).</summary>
    StopProcessingOffScreen,
}
