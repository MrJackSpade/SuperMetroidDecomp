namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Whether the rainbow beam is still widening as it aims.</summary>
internal enum MotherBrainRainbowBeamGrowth
{
    /// <summary>The beam widens, then aims.</summary>
    Widening,

    /// <summary>The beam only aims.</summary>
    FullWidth,
}
