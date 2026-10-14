namespace SuperMetroid.Core.Audio;

/// <summary>One validated BRR directory entry and its canonical PCM sample alias.</summary>
public sealed record AudioSampleMetadata(
    byte Source,
    ushort StartAddress,
    ushort LoopAddress,
    int BlockCount,
    string SampleId);
