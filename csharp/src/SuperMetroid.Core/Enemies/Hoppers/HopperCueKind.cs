namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>What a cue of the Sidehoppers' and Dessgeegas' animations does: one per kind of step its lists use.</summary>
internal enum HopperCueKind
{
    /// <summary>Queues the enemy-library sound in its operand.</summary>
    PlaySound,
    /// <summary>Keeps processing the enemy while it is off screen.</summary>
    ProcessOffScreen,
    /// <summary>Lets main AI start the next hop.</summary>
    ReadyToHop,
    /// <summary>Stops processing the enemy while it is off screen.</summary>
    StopProcessingOffScreen,
}
