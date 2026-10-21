namespace SuperMetroid.Core.Audio;

/// <summary>Decoded 16-bit PCM samples and their sample rate.</summary>
/// <param name="SampleRate">The sample rate.</param>
/// <param name="Samples">The samples.</param>
public readonly record struct PcmSamples(int SampleRate, short[] Samples);
