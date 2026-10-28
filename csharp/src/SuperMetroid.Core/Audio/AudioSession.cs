namespace SuperMetroid.Core.Audio;

/// <summary>
/// The game session's per-update audio advance (#1303, #1304, #1410): it applies the update's
/// <see cref="AudioRequest"/>s to the <see cref="SoundAcceptanceClock{TSound}"/>, runs the clock for one
/// update, publishes the acknowledgements the next update's sound handlers read, and turns the
/// update into semantic <see cref="AudioEvent"/>s for an <see cref="ClipAudioBackend"/>.
/// </summary>
/// <remarks>
/// Acknowledgements come only from the synthesis-free <see cref="SoundAcceptanceClock{TSound}"/>, so they are
/// identical whether a host plays audio, has audio disabled, or has no device. Audio output is
/// presentation: when a host binds the installed clips, the update's events also drive a
/// <see cref="ClipAudioBackend"/> whose PCM the host copies out (#1305). The backend is never
/// read back by gameplay.
/// </remarks>
public sealed class AudioSession
{
    /// <summary>Host PCM output rate of 48,000 stereo frames per second, after resampling the native 32-kHz DSP output.</summary>
    public const int SampleRate = 48_000;
    /// <summary>Two interleaved PCM channels per stereo frame: left followed by right.</summary>
    public const int ChannelCount = 2;
    /// <summary>Eight hundred stereo frames generated for each 60-Hz emulation update, yielding 1,600 signed 16-bit channel samples.</summary>
    public const int StereoFramesPerVideoFrame = SampleRate / 60;
    /// <summary>Interleaved left/right samples in one update's PCM frame.</summary>
    public const int PcmSamplesPerFrame = StereoFramesPerVideoFrame * ChannelCount;

    /// <summary>Synthesis-free command-to-event conversion that also produces the acknowledgements gameplay reads.</summary>
    private readonly AudioEventSource eventSource = new();
    /// <summary>Whether any update has advanced the session, so PCM exists to copy.</summary>
    private bool hasAdvanced;
    /// <summary>The clip backend's playback state, kept with the session so a restored session continues its PCM.</summary>
    private ClipPlaybackState? clipState;
    /// <summary>Host-bound backend that renders each update's events.</summary>
    [NonSerialized]
    private ClipAudioBackend? backend;
    /// <summary>The most recent update's interleaved 48-kHz PCM, copied out by hosts.</summary>
    [NonSerialized]
    private short[]? lastPcm;

    /// <summary>The acknowledgements published by the most recent update, for the next one.</summary>
    public SoundAcknowledgements<SoundEffectId> Acknowledgements => eventSource.Acknowledgements;

    /// <summary>
    /// Binds the installed clips (#1305) so each following update also renders host PCM from them.
    /// A restored session keeps its saved playback state; otherwise the binding must precede the
    /// first update, because the backend has to see the power-on data set.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="clips"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The session has already advanced without clips.</exception>
    public void BindClips(ClipAudioLibrary clips)
    {
        Ensure.NotNull(clips);
        if (clipState is null)
        {
            if (hasAdvanced)
                throw new InvalidOperationException(
                    "Audio clips must be bound before the session's first update; this session advanced without them.");
            clipState = new ClipPlaybackState();
        }
        backend = new ClipAudioBackend(clips, clipState);
        lastPcm = null;
    }

    /// <summary>
    /// Advances audio by one update: applies <paramref name="requests"/> in order, runs the
    /// sound acceptance clock, publishes the next update's acknowledgements and, when bound, renders PCM.
    /// </summary>
    /// <returns>The update's ordered semantic events (see <see cref="AudioFrameEvents"/>).</returns>
    /// <exception cref="InvalidDataException">A request kind is unknown, or the backend rejects its input.</exception>
    internal IReadOnlyList<AudioEvent> Advance(IReadOnlyList<AudioRequest> requests)
    {
        AudioEvent[] frameEvents = eventSource.Advance(requests);
        hasAdvanced = true;
        if (backend is not null)
        {
            lastPcm ??= new short[PcmSamplesPerFrame];
            backend.Render(new AudioFrameEvents(frameEvents), lastPcm);
        }
        return frameEvents;
    }

    /// <summary>
    /// Copies the most recent update's 1,600 interleaved 48-kHz stereo samples; silence before
    /// the first synthesized update.
    /// </summary>
    /// <exception cref="InvalidOperationException">No audio catalog is bound.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is shorter than one update's samples.</exception>
    public void CopyFrame(Span<short> destination)
    {
        if (backend is null)
            throw new InvalidOperationException("Audio PCM requires a bound audio catalog.");
        if (destination.Length < PcmSamplesPerFrame)
            throw new ArgumentException($"Audio destination requires {PcmSamplesPerFrame} samples.", nameof(destination));
        if (lastPcm is null)
            destination[..PcmSamplesPerFrame].Clear();
        else
            lastPcm.AsSpan(0, PcmSamplesPerFrame).CopyTo(destination);
    }
}
