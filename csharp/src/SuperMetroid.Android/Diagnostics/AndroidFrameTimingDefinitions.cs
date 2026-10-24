namespace SuperMetroid.Android.Diagnostics;

/// <summary>The Android host's wall-clock pacing policy; it never changes input consumption or the simulation's step order.</summary>
internal static class AndroidFrameTimingDefinitions
{
    /// <summary>Host cadence for one managed video/audio frame.</summary>
    public const double FramesPerSecond = 60;
    /// <summary>Two frames of scheduler tolerance, below the audio queue's burst-absorption capacity.</summary>
    public const int MaximumCatchUpFrames = 2;
}
