using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Offline renderer for pre-mixed audio clips (#1305). It drives the same
/// <see cref="AudioEventSource"/> and <see cref="ManagedSpcAudioBackend"/> the game
/// session uses, so a clip is by construction what runtime synthesis produced, and it returns the
/// S-DSP's native 32.04-kHz output before the host resample. Requests queued before a frame apply
/// at that frame's start, exactly as one game update's audio requests do.
/// </summary>
internal sealed class SpcAudioClipRenderer
{
    /// <summary>Interleaved left/right samples in one native 60-Hz frame (534 stereo frames).</summary>
    internal const int NativeSamplesPerFrame = ManagedSnesDsp.NativeStereoFramesPerVideoFrame * 2;

    /// <summary>Native output rate in stereo frames per second: 534 per 60-Hz frame.</summary>
    internal const int NativeSampleRate = ManagedSnesDsp.NativeStereoFramesPerVideoFrame * 60;

    /// <summary>The sound acceptance clock that turns the queued requests into each frame's events.</summary>
    private readonly AudioEventSource eventSource = new(new SoundAcceptanceClock<SoundEffectId>());
    /// <summary>The emulated driver and DSP the events drive.</summary>
    private readonly ManagedSpcAudioBackend backend;
    /// <summary>Requests applied at the next frame's start, in order.</summary>
    private readonly List<AudioRequest> pending = [];

    /// <summary>Creates a renderer over <paramref name="catalog"/> from the driver's power-on state.</summary>
    /// <param name="catalog">Extracted uploads, definitions and samples installed by bank loads.</param>
    /// <param name="observer">Receives the driver's tick, phrase and key events, or null.</param>
    internal SpcAudioClipRenderer(ExtractedAudioAssetCatalog catalog, ISpcDriverObserver? observer = null)
    {
        backend = new ManagedSpcAudioBackend(Ensure.NotNull(catalog));
        backend.Player.Music.observer = observer;
    }

    /// <summary>Frames advanced so far; the next frame's first native sample is <c>Frame * 534</c>.</summary>
    internal long Frame { get; private set; }

    /// <summary>The emulated driver, read by the analysis for its state.</summary>
    internal ManagedSpcPlayer Player => backend.Player;

    /// <summary>
    /// Queues an upload of <paramref name="bank"/> for the next frame: the change to the music data set it holds, or to
    /// only the shared sounds for the resident driver's upload.
    /// </summary>
    internal void LoadBank(AudioBank bank) =>
        pending.Add(new AudioRequest.ChangeMusicDataSet(AudioAssetCatalogDefinitions.Get(bank).DataSet));

    /// <summary>Queues playing track <paramref name="track"/> of the loaded data set for the next frame.</summary>
    internal void PlayTrack(MusicTrack track) => pending.Add(new AudioRequest.PlayMusicTrack(track));

    /// <summary>Queues a request for <paramref name="sound"/> for the next frame; the library keeps it until changed.</summary>
    internal void RequestSound(SoundEffectId sound) => pending.Add(new AudioRequest.RequestSound(sound));

    /// <summary>Advances one frame and writes its native interleaved stereo PCM to <paramref name="native"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="native"/> is shorter than <see cref="NativeSamplesPerFrame"/>.</exception>
    internal void RenderFrame(Span<short> native) => backend.RenderNative(NextFrame(), native);

    /// <summary>Advances one frame's driver state without synthesizing PCM; the state matches <see cref="RenderFrame"/> exactly.</summary>
    internal void AdvanceSequencerFrame() => backend.AdvanceSequencer(NextFrame());

    /// <summary>Converts the queued requests and the frame's clock steps into its events.</summary>
    private AudioFrameEvents NextFrame()
    {
        AudioEvent[] events = eventSource.Advance(pending);
        pending.Clear();
        Frame++;
        return new AudioFrameEvents(events);
    }
}
