namespace SuperMetroid.Core.Audio;

/// <summary>
/// The game session's per-update audio advance: it applies the update's cartridge audio
/// commands to the SPC port model, runs the driver timer for one update, and publishes the
/// acknowledgements the next update's sound handlers read (#1303).
/// </summary>
/// <remarks>
/// Acknowledgements come only from the synthesis-free <see cref="SpcCommandPorts"/>, so they are
/// identical whether a host plays audio, has audio disabled, or has no device. Synthesis is
/// presentation: when a host binds the extracted audio catalog, the same commands also drive a
/// <see cref="ManagedSpcPlayer"/> whose PCM the host copies out. The player is never read back
/// by gameplay.
/// </remarks>
public sealed class CartridgeAudioSession
{
    /// <summary>Synthesis-free SPC port model that produces the acknowledgements gameplay reads.</summary>
    private readonly SpcCommandPorts ports = new();
    /// <summary>The synthesizer's APU state, kept with the session so a restored session continues its PCM.</summary>
    private ManagedSpcPlayer? player;
    /// <summary>Whether any update has advanced the session, so PCM exists to copy.</summary>
    private bool hasAdvanced;
    /// <summary>Host-bound renderer that drives <see cref="player"/> from the bound audio catalog.</summary>
    [NonSerialized]
    private CartridgeAudioRenderer? synthesizer;
    /// <summary>The most recent update's interleaved 48-kHz PCM, copied out by hosts.</summary>
    [NonSerialized]
    private short[]? lastPcm;

    /// <summary>The acknowledgement bytes published by the most recent update, for the next one.</summary>
    /// <remarks>The music echo on port zero belongs to the sequencer and is never read by gameplay; it is published as zero.</remarks>
    public CartridgeAudioAcknowledgements Acknowledgements { get; private set; }

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
        synthesizer = new CartridgeAudioRenderer(assets, player);
        lastPcm = null;
    }

    /// <summary>
    /// Advances audio by one update: applies <paramref name="commands"/> in order, walks the
    /// driver timer, publishes the next update's acknowledgements and, when bound, synthesizes PCM.
    /// </summary>
    /// <exception cref="InvalidDataException">A command kind is unknown, or the synthesizer rejects its input.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A port write selects a port outside 0..3.</exception>
    public void Advance(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        Ensure.NotNull(commands);
        foreach (CartridgeAudioCommand command in commands)
        {
            switch (command.Kind)
            {
                case CartridgeAudioCommandKind.Upload:
                    ports.ResetForUpload();
                    break;
                case CartridgeAudioCommandKind.WritePort:
                    ports.WritePort(command.InputPort, command.Value);
                    break;
                default:
                    throw new InvalidDataException($"Unknown cartridge audio command {command.Kind}.");
            }
        }
        Span<SpcDriverTick> schedule = stackalloc SpcDriverTick[SpcCommandPorts.MaximumTicksPerFrame];
        ports.AdvanceFrame(schedule);
        hasAdvanced = true;
        Acknowledgements = new CartridgeAudioAcknowledgements(
            0,
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary1),
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary2),
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary3));
        lastPcm = synthesizer?.RenderFrame(commands);
    }

    /// <summary>
    /// Copies the most recent update's 1,600 interleaved 48-kHz stereo samples; silence before
    /// the first synthesized update.
    /// </summary>
    /// <exception cref="InvalidOperationException">No audio catalog is bound.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is shorter than one update's samples.</exception>
    public void CopyFrame(Span<short> destination)
    {
        if (synthesizer is null)
            throw new InvalidOperationException("Audio PCM requires a bound audio catalog.");
        if (destination.Length < PcmSamplesPerFrame)
            throw new ArgumentException($"Audio destination requires {PcmSamplesPerFrame} samples.", nameof(destination));
        if (lastPcm is null)
            destination[..PcmSamplesPerFrame].Clear();
        else
            lastPcm.AsSpan(0, PcmSamplesPerFrame).CopyTo(destination);
    }

    /// <summary>Interleaved left/right samples in one update's PCM frame.</summary>
    public const int PcmSamplesPerFrame =
        CartridgeAudioRenderer.StereoFramesPerVideoFrame * CartridgeAudioRenderer.ChannelCount;
}
