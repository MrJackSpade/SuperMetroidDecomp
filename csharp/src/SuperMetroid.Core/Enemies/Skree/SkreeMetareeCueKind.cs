namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>What a cue of the Skree's and Metaree's animations does: one per kind of step its lists use.</summary>
internal enum SkreeMetareeCueKind
{
    /// <summary>Keeps processing the enemy while it is off screen.</summary>
    ProcessOffScreen,
    /// <summary>Tells main AI the wind-up is done and the dive may start.</summary>
    SetAttackReady,
    /// <summary>Stops processing the enemy while it is off screen.</summary>
    StopProcessingOffScreen,
}
