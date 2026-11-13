namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>What a cue of the Sidehoppers' and Dessgeegas' animations does: queue a sound, keep or stop processing it off screen, or let main AI start the next hop.</summary>
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
