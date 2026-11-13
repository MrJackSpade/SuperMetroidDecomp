namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>What a cue of Boyon's animations does: keep or stop processing it off screen, play the bounce sound and start the bounce, or nothing.</summary>
internal enum BoyonCueKind
{
    /// <summary>Keeps processing the enemy while it is off screen.</summary>
    ProcessOffScreen,
    /// <summary>Returns at once without effect.</summary>
    Return,
    /// <summary>Plays the bounce sound and lets main AI start the bounce arc.</summary>
    StartBounce,
    /// <summary>Stops processing the enemy while it is off screen.</summary>
    StopProcessingOffScreen,
}
