namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What a cue of Mother Brain's purple breath does (#2554): stop or delete the breath, or mark her small breath ended, which
/// only the breath keeping her encounter runs.
/// </summary>
internal enum MotherBrainBreathCue
{
    /// <summary>Stops the breath's movement.</summary>
    StopMoving,
    /// <summary>Deletes the breath.</summary>
    Delete,
    /// <summary>Marks Mother Brain's small purple breath inactive.</summary>
    EndSmallPurpleBreath,
}
