namespace SuperMetroid.Core.Audio;

/// <summary>Shared physical-format limits for stock and user-replaceable sample assets.</summary>
public static class PcmSampleFormat
{
    /// <summary>Rate assigned to one decoded S-DSP sample cycle in the extracted stock WAVs.</summary>
    public const int StockSampleRate = 32_000;

    /// <summary>Lowest replacement rate accepted before pitch normalization becomes impractical.</summary>
    public const int MinimumReplacementSampleRate = 8_000;

    /// <summary>Highest accepted replacement rate, sufficient for contemporary HD sources.</summary>
    public const int MaximumReplacementSampleRate = 384_000;

    /// <summary>Runtime sample assets are mono because voice panning remains a live DSP property.</summary>
    public const ushort ChannelCount = 1;

    /// <summary>Signed PCM16 preserves every value produced by the SNES BRR decoder.</summary>
    public const ushort BitsPerSample = 16;

    /// <summary>
    /// Number of source frames staged behind the four-tap interpolator at once; this matches
    /// one decoded BRR block and remains the managed streaming window for replacement PCM.
    /// </summary>
    public const int StreamingWindowSampleCount = 16;
}
