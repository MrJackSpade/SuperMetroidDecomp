namespace SuperMetroid.Desktop;

/// <summary>Result of the non-GUI cartridge audio diagnostic.</summary>
public readonly record struct CartridgeAudioSmokeTestResult(
    int FramesGenerated,
    int NonZeroSamples,
    int PeakAmplitude,
    int PowerBeamNonZeroSamples);
