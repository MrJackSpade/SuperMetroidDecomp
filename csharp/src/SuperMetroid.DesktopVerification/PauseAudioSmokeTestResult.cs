namespace SuperMetroid.Desktop;

/// <summary>PCM-level regression for gameplay-to-pause-to-gameplay audio continuity.</summary>
public readonly record struct PauseAudioSmokeTestResult(
    int FramesGenerated,
    int PauseFrames,
    int AudioCommandCount,
    int MaximumAdjacentSampleDelta,
    int MaximumFrameBoundaryDelta);
