namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>What a cue of the Beetom's animations does: keep or stop processing it off screen.</summary>
internal enum BeetomCueKind
{
    /// <summary>Keeps processing the enemy while it is off screen.</summary>
    ProcessOffScreen,
    /// <summary>Stops processing the enemy while it is off screen.</summary>
    StopProcessingOffScreen,
}
