namespace SuperMetroid.Core.Audio;

/// <summary>
/// Measures temporal PCM steps within each stereo channel, including across submitted
/// buffers. Channel separation is not a time discontinuity: never compare L with R.
/// </summary>
public sealed class StereoPcmContinuityMeter
{
    private bool hasPreviousFrame;
    private short previousLeft;
    private short previousRight;

    public int MaximumWithinBufferDelta { get; private set; }
    public int MaximumBoundaryDelta { get; private set; }

    /// <summary>
    /// Observes interleaved L/R frames without retaining the caller's buffer. Empty
    /// submissions preserve history; malformed half-frames fail before changing state.
    /// </summary>
    public void Observe(ReadOnlySpan<short> samples)
    {
        if ((samples.Length & 1) != 0)
            throw new ArgumentException("Stereo PCM must contain complete left/right pairs.", nameof(samples));
        if (samples.IsEmpty) return;
        if (hasPreviousFrame)
        {
            MaximumBoundaryDelta = Math.Max(MaximumBoundaryDelta,
                Math.Max(Math.Abs((int)samples[0] - previousLeft),
                    Math.Abs((int)samples[1] - previousRight)));
        }
        for (int index = 2; index < samples.Length; index++)
            MaximumWithinBufferDelta = Math.Max(MaximumWithinBufferDelta,
                Math.Abs((int)samples[index] - samples[index - 2]));
        previousLeft = samples[^2];
        previousRight = samples[^1];
        hasPreviousFrame = true;
    }
}
