namespace SuperMetroid.DesktopVerification.Audio;

/// <summary>PCM measurements for one complete pause found in a recorded player session.</summary>
/// <param name="EntryFrame">Recorded update that entered the pause transition.</param>
/// <param name="ResumeFrame">Recorded update that resumed gameplay.</param>
/// <param name="Room">Room active during the pause.</param>
/// <param name="PauseOwnedFrames">Updates owned by pause frontend states.</param>
/// <param name="MinimumPausedRms">Lowest PCM root-mean-square level while fully paused.</param>
/// <param name="MaximumPausedRms">Highest PCM root-mean-square level while fully paused.</param>
/// <param name="MeanPausedRms">Average per-frame PCM root-mean-square level while fully paused.</param>
/// <param name="AudioEvents">Bank loads, music commands and sound-effect starts observed during pause-owned updates.</param>
/// <param name="CommandTrace">Frame/state-tagged audio commands around the pause interval.</param>
public readonly record struct RecordedPauseAudioResult(
    int EntryFrame,
    int ResumeFrame,
    string Room,
    int PauseOwnedFrames,
    double MinimumPausedRms,
    double MaximumPausedRms,
    double MeanPausedRms,
    int AudioEvents,
    string CommandTrace);
