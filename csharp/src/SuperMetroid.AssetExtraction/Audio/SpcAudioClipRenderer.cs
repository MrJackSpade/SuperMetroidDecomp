using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Offline renderer for pre-mixed audio clips (#1305). It drives the same
/// <see cref="CartridgeAudioEventSource"/> and <see cref="ManagedSpcAudioBackend"/> the game
/// session uses, so a clip is by construction what runtime synthesis produced, and it returns the
/// S-DSP's native 32.04-kHz output before the host resample. Commands queued before a frame apply
/// at that frame's start, exactly as one game update's port writes and uploads do.
/// </summary>
internal sealed class SpcAudioClipRenderer
{
    /// <summary>Interleaved left/right samples in one native 60-Hz frame (534 stereo frames).</summary>
    internal const int NativeSamplesPerFrame = ManagedSnesDsp.NativeStereoFramesPerVideoFrame * 2;

    /// <summary>Native output rate in stereo frames per second: 534 per 60-Hz frame.</summary>
    internal const int NativeSampleRate = ManagedSnesDsp.NativeStereoFramesPerVideoFrame * 60;

    /// <summary>Port model that turns the queued commands into each frame's events.</summary>
    private readonly CartridgeAudioEventSource eventSource = new();
    /// <summary>The emulated driver and DSP the events drive.</summary>
    private readonly ManagedSpcAudioBackend backend;
    /// <summary>Uploads and port writes applied at the next frame's start, in order.</summary>
    private readonly List<CartridgeAudioCommand> pending = [];

    /// <summary>Creates a renderer over <paramref name="catalog"/> from the driver's power-on state.</summary>
    /// <param name="catalog">Extracted uploads, definitions and samples installed by bank loads.</param>
    /// <param name="observer">Receives the driver's tick, phrase and key events, or null.</param>
    internal SpcAudioClipRenderer(ExtractedAudioAssetCatalog catalog, ISpcDriverObserver? observer = null)
    {
        backend = new ManagedSpcAudioBackend(Ensure.NotNull(catalog));
        backend.Player.observer = observer;
    }

    /// <summary>Frames advanced so far; the next frame's first native sample is <c>Frame * 534</c>.</summary>
    internal long Frame { get; private set; }

    /// <summary>The emulated driver, read by the analysis for its state.</summary>
    internal ManagedSpcPlayer Player => backend.Player;

    /// <summary>Queues an upload of <paramref name="bank"/> for the next frame.</summary>
    internal void LoadBank(AudioBank bank) => pending.Add(CartridgeAudioCommand.Upload(bank));

    /// <summary>Queues a music-port write (a track number, or zero to stop) for the next frame.</summary>
    internal void WriteMusicPort(byte value) => pending.Add(CartridgeAudioCommand.WritePort(ApuPort.Music, value));

    /// <summary>Queues a sound-effect request on <paramref name="library"/>'s port for the next frame; the port keeps the value until rewritten.</summary>
    internal void WriteSoundEffectPort(SoundEffectLibrary library, int command) =>
        pending.Add(CartridgeAudioCommand.WritePort(ApuPorts.ForLibrary(library), command));

    /// <summary>Advances one frame and writes its native interleaved stereo PCM to <paramref name="native"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="native"/> is shorter than <see cref="NativeSamplesPerFrame"/>.</exception>
    internal void RenderFrame(Span<short> native) => backend.RenderNative(NextFrame(), native);

    /// <summary>Advances one frame's driver state without synthesizing PCM; the state matches <see cref="RenderFrame"/> exactly.</summary>
    internal void AdvanceSequencerFrame() => backend.AdvanceSequencer(NextFrame());

    /// <summary>Converts the queued commands and the frame's driver ticks into its events.</summary>
    private AudioFrameEvents NextFrame()
    {
        AudioEvent[] events = eventSource.Advance(pending);
        pending.Clear();
        Frame++;
        return new AudioFrameEvents(events);
    }
}
