namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>What a cue of the Beetom's animations does: keep or stop processing it off screen, or nothing before the drain loop.</summary>
internal enum BeetomCueKind
{
    /// <summary>Does nothing: the empty step the drain animation calls before its loop.</summary>
    Nothing,
    /// <summary>Keeps processing the enemy while it is off screen.</summary>
    ProcessOffScreen,
    /// <summary>Stops processing the enemy while it is off screen.</summary>
    StopProcessingOffScreen,
}
