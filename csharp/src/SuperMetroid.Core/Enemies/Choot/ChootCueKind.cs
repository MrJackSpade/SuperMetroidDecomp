namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>What a cue of Choot's animations does: keep or stop processing it off screen.</summary>
internal enum ChootCueKind
{
    /// <summary>Keeps processing the enemy while it is off screen.</summary>
    ProcessOffScreen,
    /// <summary>Stops processing the enemy while it is off screen.</summary>
    StopProcessingOffScreen,
}
