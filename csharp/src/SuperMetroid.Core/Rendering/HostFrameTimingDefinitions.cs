namespace SuperMetroid.Core.Rendering;

/// <summary>Wall-clock host policy; does not change cartridge input or emulated frame progression.</summary>
public static class HostFrameTimingDefinitions
{
    /// <summary>Existing host cadence for one managed video/audio frame.</summary>
    public const double FramesPerSecond = 60;
    /// <summary>Two frames of scheduler tolerance, below the audio queue's burst-absorption capacity.</summary>
    public const int MaximumCatchUpFrames = 2;
}
