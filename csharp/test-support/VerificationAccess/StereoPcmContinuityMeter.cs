namespace SuperMetroid.Core.Audio;

/// <summary>
/// Measures temporal PCM steps within each stereo channel, including across submitted
/// buffers. Channel separation is not a time discontinuity: never compare L with R.
/// </summary>
internal sealed class StereoPcmContinuityMeter
{
    /// <summary>Indicates whether a preceding nonempty observation supplied a frame for boundary comparisons.</summary>
    private bool hasPreviousFrame;

    /// <summary>Left-channel sample from the most recently observed stereo frame.</summary>
    private short previousLeft;

    /// <summary>Right-channel sample from the most recently observed stereo frame.</summary>
    private short previousRight;

    /// <summary>Largest absolute same-channel sample step between adjacent frames in one submitted buffer.</summary>
    public int MaximumWithinBufferDelta { get; private set; }

    /// <summary>Largest absolute same-channel step from the last frame of one buffer to the first of the next.</summary>
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
