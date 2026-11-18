namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The red hand beam's segments: its spawning, fading stage, played three times, and its deletion.</summary>
internal enum MotherBrainHandBeamSegment
{
    /// <summary>
    /// One stage of the beam, which starts its animation: the small orb, the spawn of the next beam, and the orb fading over
    /// six frames. The charging beam and every fired beam play three stages.
    /// </summary>
    Stage,
    /// <summary>Deletes the beam once its stages have played.</summary>
    Delete,
}
