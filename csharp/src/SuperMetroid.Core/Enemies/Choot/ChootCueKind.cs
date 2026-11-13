namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>What a cue of Choot's animations does: one per kind of step its lists use.</summary>
internal enum ChootCueKind
{
    /// <summary>Keeps processing the enemy while it is off screen.</summary>
    ProcessOffScreen,
    /// <summary>Stops processing the enemy while it is off screen.</summary>
    StopProcessingOffScreen,
}
