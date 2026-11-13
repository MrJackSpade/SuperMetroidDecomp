namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>What a cue of the Beetom's programs does: one per kind of step its lists use.</summary>
internal enum BeetomCueKind
{
    /// <summary>Does nothing: the empty step the drain animation calls before its loop.</summary>
    Nothing,
    /// <summary>Keeps processing the enemy while it is off screen.</summary>
    ProcessOffScreen,
    /// <summary>Stops processing the enemy while it is off screen.</summary>
    StopProcessingOffScreen,
}
