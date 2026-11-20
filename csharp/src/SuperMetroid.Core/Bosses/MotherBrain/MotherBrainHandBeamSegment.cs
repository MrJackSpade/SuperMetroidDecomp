namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The red hand beam's segments: its spawning, fading stage, played once for each of its stages, and its deletion.</summary>
internal enum MotherBrainHandBeamSegment
{
    /// <summary>
    /// One stage of the beam, which starts its animation: the small orb, the spawn of the next beam, and the orb fading
    /// out. The charging beam and every fired beam play all its stages.
    /// </summary>
    Stage,
    /// <summary>Deletes the beam once its stages have played.</summary>
    Delete,
}
