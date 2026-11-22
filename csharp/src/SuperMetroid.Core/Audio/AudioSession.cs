namespace SuperMetroid.Core.Audio;

/// <summary>
/// The game session's per-update audio advance (#1303, #1304, #1410): it applies the update's
/// <see cref="AudioRequest"/>s to the <see cref="SoundAcceptanceClock{TLibrary, TSound}"/>, runs the clock for one
/// update, publishes the acknowledgements the next update's sound queues read, and turns the
/// update into semantic <see cref="AudioEvent"/>s for an <see cref="ClipAudioBackend"/>.
/// </summary>
/// <remarks>
/// Acknowledgements come only from the synthesis-free <see cref="SoundAcceptanceClock{TLibrary, TSound}"/>, so they are
/// identical whether a host plays audio, has audio disabled, or has no device. Audio output is
/// presentation: the session mixes the installed clips, driving a <see cref="ClipAudioBackend"/> with each update's
/// events, whose PCM a host that plays audio copies out (#1305). The backend is never read back by gameplay.
/// </remarks>
public sealed partial class AudioSession
{
    /// <summary>
    /// The PCM output rate, in stereo frames per second: the installed clips' own rate, 32,040, at which the session mixes
    /// them (#2057). The host resamples the PCM to its device's rate.
    /// </summary>
    public const int SampleRate = 32_040;
    /// <summary>Two interleaved PCM channels per stereo frame: left followed by right.</summary>
    public const int ChannelCount = 2;
    /// <summary>The stereo frames generated for each update, yielding <see cref="PcmSamplesPerFrame"/> signed channel samples.</summary>
    public const int StereoFramesPerUpdate = SampleRate / 60;
    /// <summary>Interleaved left/right samples in one update's PCM frame.</summary>
    public const int PcmSamplesPerFrame = StereoFramesPerUpdate * ChannelCount;

    /// <summary>Creates the session's audio advance on the session's <paramref name="soundClock"/>, mixing <paramref name="clips"/>.</summary>
    /// <param name="soundClock">
    /// The game session's sound acceptance clock, created at the session's composition root and living as long as the
    /// session: its requests, acknowledgements and request-check phase carry across menus and gameplay runtimes.
    /// </param>
    /// <param name="clips">The installed clips the session mixes into the PCM its host copies.</param>
    internal AudioSession(SoundAcceptanceClock<SoundEffectLibrary, SoundEffectId> soundClock, ClipAudioLibrary clips)
    {
        eventSource = new AudioEventSource(soundClock);
        mix = new ClipMix(clips);
    }

    /// <summary>Synthesis-free request-to-event conversion that also produces the acknowledgements gameplay reads.</summary>
    private readonly AudioEventSource eventSource;
    /// <summary>The mix that turns each update's events into the PCM its host copies.</summary>
    private readonly ClipMix mix;

    /// <summary>The acknowledgements published by the most recent update, for the next one.</summary>
    public SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> Acknowledgements => eventSource.Acknowledgements;

    /// <summary>
    /// Advances audio by one update: applies <paramref name="requests"/> in order, runs the
    /// sound acceptance clock, publishes the next update's acknowledgements and renders the update's PCM.
    /// </summary>
    /// <returns>The update's ordered semantic events (see <see cref="AudioFrameEvents"/>).</returns>
    /// <exception cref="InvalidDataException">A request kind is unknown, or the backend rejects its input.</exception>
    internal IReadOnlyList<AudioEvent> Advance(IReadOnlyList<AudioRequest> requests)
    {
        AudioEvent[] frameEvents = eventSource.Advance(requests);
        mix.Render(new AudioFrameEvents(frameEvents));
        return frameEvents;
    }

    /// <summary>
    /// Copies the most recent update's interleaved stereo samples (<see cref="PcmSamplesPerFrame"/>): silence before the
    /// first update.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is shorter than one update's samples.</exception>
    public void CopyFrame(Span<short> destination)
    {
        if (destination.Length < PcmSamplesPerFrame)
            throw new ArgumentException($"Audio destination requires {PcmSamplesPerFrame} samples.", nameof(destination));
        mix.CopyFrame(destination[..PcmSamplesPerFrame]);
    }

    /// <summary>The session's PCM, mixed from the installed clips (#1305).</summary>
    /// <param name="clips">The installed clips the session mixes.</param>
    private sealed partial class ClipMix(ClipAudioLibrary clips)
    {
        /// <summary>The installed clips the session mixes.</summary>
        private readonly ClipAudioLibrary clips = Ensure.NotNull(clips);

        /// <summary>The clip player's playback state, which the session's audio continues from, a restored session's too.</summary>
        private readonly ClipPlaybackState clipState = new(clips.InitialBeat);

        /// <summary>
        /// The backend that mixes each update's events over the clips: the clips' decoded samples and a mix buffer, kept
        /// per clip as clips first play. None of it is game state, so a saved session leaves it out and its first update
        /// after a restore builds it again over <see cref="clipState"/>, which is.
        /// </summary>
        [NonSerialized]
        private ClipAudioBackend? backend;

        /// <summary>The most recent update's interleaved PCM, which hosts copy; none before the first update.</summary>
        private short[]? lastPcm;

        /// <summary>Renders one update's <paramref name="frame"/>.</summary>
        internal void Render(in AudioFrameEvents frame)
        {
            backend ??= new ClipAudioBackend(clips, clipState);
            lastPcm ??= new short[PcmSamplesPerFrame];
            backend.Render(frame, lastPcm);
        }

        /// <summary>Copies the most recent update's PCM into <paramref name="destination"/>, one update's samples long.</summary>
        internal void CopyFrame(Span<short> destination)
        {
            if (lastPcm is null)
                destination.Clear();
            else
                lastPcm.AsSpan(0, PcmSamplesPerFrame).CopyTo(destination);
        }
    }
}
