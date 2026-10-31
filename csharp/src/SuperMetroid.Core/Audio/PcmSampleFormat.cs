namespace SuperMetroid.Core.Audio;

/// <summary>Shared physical-format limits for stock and user-replaceable sample assets.</summary>
public static class PcmSampleFormat
{
    /// <summary>Rate of the extracted stock instrument samples.</summary>
    public const int StockSampleRate = 32_000;

    /// <summary>Lowest replacement rate accepted before pitch normalization becomes impractical.</summary>
    public const int MinimumReplacementSampleRate = 8_000;

    /// <summary>Highest accepted replacement rate, sufficient for contemporary HD sources.</summary>
    public const int MaximumReplacementSampleRate = 384_000;

    /// <summary>Instrument sample assets are mono; a voice pans them as it plays.</summary>
    public const ushort ChannelCount = 1;

    /// <summary>Interleaved left/right channels of a rendered stereo clip (#1305).</summary>
    public const ushort StereoChannelCount = 2;

    /// <summary>Signed 16-bit PCM holds every decoded instrument sample value.</summary>
    public const ushort BitsPerSample = 16;

    /// <summary>
    /// Number of source frames staged behind the four-tap interpolator at once: the streaming
    /// window for instrument and replacement PCM.
    /// </summary>
    public const int StreamingWindowSampleCount = 16;
}
