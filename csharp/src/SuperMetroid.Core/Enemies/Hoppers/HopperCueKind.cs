namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>What a cue of the Sidehoppers' and Dessgeegas' programs does: one per bank-$A3 instruction its lists use.</summary>
internal enum HopperCueKind
{
    /// <summary>Queues the library-two sound in its operand (<c>SidehopperQueueSoundInY</c>, $A3:AA68).</summary>
    PlaySound,
    /// <summary>Keeps processing the enemy while it is off screen (<c>CommonEnemyInstruction.EnableOffScreenProcessing</c>, $A0:8173).</summary>
    ProcessOffScreen,
    /// <summary>Lets main AI start the next hop (<c>ReadyToHop</c>, $A3:AAFE).</summary>
    ReadyToHop,
    /// <summary>Stops processing the enemy while it is off screen (<c>CommonEnemyInstruction.DisableOffScreenProcessing</c>, $A0:817D).</summary>
    StopProcessingOffScreen,
}
