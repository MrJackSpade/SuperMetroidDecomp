using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One validated BRR directory entry and its canonical PCM sample alias.</summary>
public sealed record AudioSampleMetadata(
    int Source,
    int StartAddress,
    int LoopAddress,
    string SampleId)
{
    /// <summary>The BRR directory source number, 0 through 255.</summary>
    public int Source { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Source);

    /// <summary>The sample's audio-RAM start address, 0 through 65535.</summary>
    public int StartAddress { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, StartAddress);

    /// <summary>The sample's audio-RAM loop address, 0 through 65535.</summary>
    public int LoopAddress { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, LoopAddress);
}
