namespace SuperMetroid.DesktopVerification.Audio;

/// <summary>Aggregate corpus counts and verified PCM/acknowledgement digests for the installed managed-audio regression scenarios.</summary>
/// <param name="Scenarios">Completed music, sound-effect, and lifecycle scenarios.</param>
/// <param name="Frames">Total audio updates rendered across all scenarios.</param>
/// <param name="PcmSamples">Number of signed 16-bit interleaved PCM values hashed, counting both stereo channels separately.</param>
/// <param name="CanonicalSamples">Distinct canonical waveforms in the installed catalog.</param>
/// <param name="SourceAliases">Installed source-number mappings across uploads.</param>
/// <param name="PcmSha256">Uppercase SHA-256 of scenario names and the rendered PCM bytes in corpus order.</param>
/// <param name="AcknowledgementSha256">Uppercase SHA-256 of scenario names and four-port acknowledgement bytes in corpus order.</param>
public readonly record struct ManagedAudioRegressionSmokeTestResult(
    int Scenarios,
    int Frames,
    int PcmSamples,
    int CanonicalSamples,
    int SourceAliases,
    string PcmSha256,
    string AcknowledgementSha256);
