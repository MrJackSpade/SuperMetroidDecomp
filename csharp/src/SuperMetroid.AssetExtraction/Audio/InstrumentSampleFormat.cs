namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Physical-format limits for the stock and user-replaceable instrument sample assets the import renders from.</summary>
public static class InstrumentSampleFormat
{
    /// <summary>Rate of the extracted stock instrument samples.</summary>
    public const int StockSampleRate = 32_000;

    /// <summary>Lowest replacement rate accepted before pitch normalization becomes impractical.</summary>
    public const int MinimumReplacementSampleRate = 8_000;

    /// <summary>Highest accepted replacement rate, sufficient for contemporary HD sources.</summary>
    public const int MaximumReplacementSampleRate = 384_000;

    /// <summary>Instrument sample assets are mono; a voice pans them as it plays.</summary>
    public const int ChannelCount = 1;

    /// <summary>Signed 16-bit PCM holds every decoded instrument sample value.</summary>
    public const int BitsPerSample = 16;

    /// <summary>
    /// Number of source frames staged behind the four-tap interpolator at once: the streaming
    /// window for instrument and replacement PCM.
    /// </summary>
    public const int StreamingWindowSampleCount = 16;
}
