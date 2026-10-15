namespace SuperMetroid.Core.Audio;

/// <summary>
/// The game session's per-update audio advance (#1303, #1304): it applies the update's
/// cartridge port writes and uploads to the SPC port model, runs the driver timer for one
/// update, publishes the acknowledgements the next update's sound handlers read, and turns the
/// update into semantic <see cref="AudioEvent"/>s for an <see cref="IAudioBackend"/>.
/// </summary>
/// <remarks>
/// Acknowledgements come only from the synthesis-free <see cref="SpcCommandPorts"/>, so they are
/// identical whether a host plays audio, has audio disabled, or has no device. Synthesis is
/// presentation: when a host binds the extracted audio catalog, the update's events also drive
/// a <see cref="ManagedSpcAudioBackend"/> whose PCM the host copies out. The backend is never
/// read back by gameplay.
/// </remarks>
public sealed class CartridgeAudioSession
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
    private readonly CartridgeAudioEventSource eventSource = new();
    /// <summary>The emulated driver's state, kept with the session so a restored session continues its PCM.</summary>
    internal ManagedSpcPlayer? player;
    /// <summary>Whether any update has advanced the session, so PCM exists to copy.</summary>
    private bool hasAdvanced;
    /// <summary>Host-bound backend that renders each update's events; the emulated driver is the one <see cref="IAudioBackend"/> today.</summary>
    [NonSerialized]
    private ManagedSpcAudioBackend? backend;
    /// <summary>The most recent update's interleaved 48-kHz PCM, copied out by hosts.</summary>
    [NonSerialized]
    private short[]? lastPcm;

    /// <summary>The acknowledgement bytes published by the most recent update, for the next one.</summary>
    /// <remarks>The music echo on port zero belongs to the sequencer and is never read by gameplay; it is published as zero.</remarks>
    public CartridgeAudioAcknowledgements Acknowledgements => eventSource.Acknowledgements;

    /// <summary>
    /// Binds the extracted audio catalog so each following update also synthesizes host PCM. A
    /// restored session keeps its saved player; otherwise the binding must precede the first update,
    /// because the player has to see the reset upload.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="assets"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The session has already advanced without a synthesizer.</exception>
    public void BindSynthesis(ExtractedAudioAssetCatalog assets)
    {
        Ensure.NotNull(assets);
        if (player is null)
        {
            if (hasAdvanced)
                throw new InvalidOperationException(
                    "Audio synthesis must be bound before the session's first update; this session advanced without it.");
            player = new ManagedSpcPlayer();
        }
        backend = new ManagedSpcAudioBackend(assets, player);
        lastPcm = null;
    }

    /// <summary>
    /// Advances audio by one update: applies <paramref name="commands"/> in order, walks the
    /// driver timer, publishes the next update's acknowledgements and, when bound, renders PCM.
    /// </summary>
    /// <returns>The update's ordered semantic events (see <see cref="AudioFrameEvents"/>).</returns>
    /// <exception cref="InvalidDataException">A command kind is unknown, or the backend rejects its input.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A port write selects a port outside 0..3.</exception>
    internal IReadOnlyList<AudioEvent> Advance(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        AudioEvent[] frameEvents = eventSource.Advance(commands);
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
