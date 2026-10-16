namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One validated BRR directory entry and its canonical PCM sample alias.</summary>
public sealed record AudioSampleMetadata(
    byte Source,
    ushort StartAddress,
    ushort LoopAddress,
    string SampleId);
