namespace SuperMetroid.DesktopVerification.Audio;

/// <summary>Result of a host-device queue/backpressure diagnostic.</summary>
public readonly record struct WaveOutAudioSmokeTestResult(
    int BuffersSubmitted,
    TimeSpan SubmissionTime);
