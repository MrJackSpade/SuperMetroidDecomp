namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One canonical, replaceable PCM asset shared by every equivalent bank source.</summary>
public sealed partial record AudioCanonicalSampleMetadata(
    string Id,
    string WavFile,
    int SampleRate,
    int SampleCount,
    int? LoopSampleIndex);
