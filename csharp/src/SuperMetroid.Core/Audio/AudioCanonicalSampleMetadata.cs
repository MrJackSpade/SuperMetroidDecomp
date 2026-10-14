namespace SuperMetroid.Core.Audio;

/// <summary>One canonical, replaceable PCM asset shared by every equivalent bank source.</summary>
public sealed record AudioCanonicalSampleMetadata(
    string Id,
    string WavFile,
    int SampleRate,
    int SampleCount,
    int? LoopSampleIndex,
    string Sha256);
