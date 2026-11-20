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
/// presentation: a session built to mix the installed clips (<see cref="GameAudioOutput.Mixed"/>) also drives a
/// <see cref="ClipAudioBackend"/> with each update's events, whose PCM the host copies out (#1305); a muted session's
/// PCM is silence. The backend is never read back by gameplay.
/// </remarks>
public sealed class AudioSession
{
    /// <summary>
    /// The PCM output rate, in stereo frames per second: the installed clips' own rate, 32,040, at which the session mixes
    /// them (#2057). The host resamples the PCM to its device's rate.
    /// </summary>
    public const int SampleRate = 32_040;
    /// <summary>Two interleaved PCM channels per stereo frame: left followed by right.</summary>
    public const int ChannelCount = 2;
    /// <summary>534 stereo frames generated for each 60-Hz update, yielding 1,068 signed channel samples from -32768 through 32767.</summary>
    public const int StereoFramesPerUpdate = SampleRate / 60;
    /// <summary>Interleaved left/right samples in one update's PCM frame.</summary>
    public const int PcmSamplesPerFrame = StereoFramesPerUpdate * ChannelCount;

    /// <summary>Creates the session's audio advance on the session's <paramref name="soundClock"/>, producing <paramref name="output"/>.</summary>
    /// <param name="soundClock">
    /// The game session's sound acceptance clock, created at the session's composition root and living as long as the
    /// session: its requests, acknowledgements and request-check phase carry across menus and gameplay runtimes.
    /// </param>
    /// <param name="output">What the session's audio produces for its host: PCM mixed from the installed clips, or silence.</param>
    internal AudioSession(SoundAcceptanceClock<SoundEffectLibrary, SoundEffectId> soundClock, GameAudioOutput output)
    {
        eventSource = new AudioEventSource(soundClock);
        mix = Ensure.NotNull(output) switch
        {
            GameAudioOutput.Mixed mixed => new ClipMix(mixed.Clips),
            GameAudioOutput.Muted => MutedMix.Instance,
            _ => throw new InvalidDataException($"Unknown audio output {output}."),
        };
    }

    /// <summary>Synthesis-free request-to-event conversion that also produces the acknowledgements gameplay reads.</summary>
    private readonly AudioEventSource eventSource;
    /// <summary>How the session turns each update's events into the PCM its host copies.</summary>
    private readonly SessionAudioMix mix;

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
    /// first update, and always from a muted session.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is shorter than one update's samples.</exception>
    public void CopyFrame(Span<short> destination)
    {
        if (destination.Length < PcmSamplesPerFrame)
            throw new ArgumentException($"Audio destination requires {PcmSamplesPerFrame} samples.", nameof(destination));
        mix.CopyFrame(destination[..PcmSamplesPerFrame]);
    }

    /// <summary>How a session turns each update's events into PCM: mixed from the installed clips, or silence.</summary>
    private abstract class SessionAudioMix
    {
        /// <summary>Renders one update's <paramref name="frame"/>.</summary>
        internal abstract void Render(in AudioFrameEvents frame);

        /// <summary>Copies the most recent update's PCM into <paramref name="destination"/>, one update's samples long.</summary>
        internal abstract void CopyFrame(Span<short> destination);
    }

    /// <summary>The PCM of a session that mixes the installed clips (#1305).</summary>
    /// <param name="clips">The installed clips the session mixes.</param>
    private sealed class ClipMix(ClipAudioLibrary clips) : SessionAudioMix
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

        /// <inheritdoc/>
        internal override void Render(in AudioFrameEvents frame)
        {
            backend ??= new ClipAudioBackend(clips, clipState);
            lastPcm ??= new short[PcmSamplesPerFrame];
            backend.Render(frame, lastPcm);
        }

        /// <inheritdoc/>
        internal override void CopyFrame(Span<short> destination)
        {
            if (lastPcm is null)
                destination.Clear();
            else
                lastPcm.AsSpan(0, PcmSamplesPerFrame).CopyTo(destination);
        }
    }

    /// <summary>The PCM of a muted session: silence on every update.</summary>
    private sealed class MutedMix : SessionAudioMix
    {
        /// <summary>The one muted mix, which keeps no state.</summary>
        internal static MutedMix Instance { get; } = new();

        /// <inheritdoc/>
        internal override void Render(in AudioFrameEvents frame)
        {
        }

        /// <inheritdoc/>
        internal override void CopyFrame(Span<short> destination) => destination.Clear();
    }
}
