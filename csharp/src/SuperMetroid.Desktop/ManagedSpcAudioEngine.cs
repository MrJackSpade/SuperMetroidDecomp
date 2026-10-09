using SuperMetroid.Core.Audio;

namespace SuperMetroid.Desktop;

/// <summary>
/// Desktop adapter for the fully managed Super Metroid SPC driver and S-DSP mixer.
/// The adapter resolves upload commands through extracted assets and owns the reusable host PCM buffer;
/// all sequencer, voice, echo, pitch, and envelope behavior lives in debuggable Core code.
/// </summary>
internal sealed class SpcAudioEngine : IDisposable
{
    /// <summary>Output sampling frequency used by the desktop mixer in samples per second.</summary>
    public const int SampleRate = 48_000;
    /// <summary>Number of stereo sample frames rendered for each 60 Hz video frame.</summary>
    public const int StereoFramesPerVideoFrame = SampleRate / 60;
    /// <summary>Number of interleaved audio channels in each rendered sample frame.</summary>
    public const int ChannelCount = 2;

    /// <summary>Managed renderer that applies cartridge audio commands and produces host PCM samples.</summary>
    private readonly CartridgeAudioRenderer renderer;
    /// <summary>Tracks whether disposal has disabled further frame rendering.</summary>
    private bool disposed;

    /// <summary>Creates the desktop audio adapter around extracted sound assets and the persisted managed APU state.</summary>
    /// <param name="assets">Extracted audio data used to resolve cartridge upload commands.</param>
    /// <param name="player">Managed SPC player state whose voices and DSP state are rendered.</param>
    internal SpcAudioEngine(ExtractedAudioAssetCatalog assets, ManagedSpcPlayer player)
    {
        renderer = new CartridgeAudioRenderer(assets, player ?? throw new ArgumentNullException(nameof(player)));
    }

    /// <summary>The exact managed APU state persisted with debugger save states.</summary>
    internal ManagedSpcPlayer Player => renderer.Player;

    /// <summary>Applies this NMI's APU operations, then renders one complete audio frame.</summary>
    public ReadOnlySpan<short> RenderFrame(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(commands);
        return renderer.RenderFrame(commands);
    }

    /// <summary>Returns acknowledgements for cartridge audio commands applied by the renderer.</summary>
    /// <returns>The renderer's current acknowledgement set.</returns>
    public CartridgeAudioAcknowledgements ReadAcknowledgements() => renderer.ReadAcknowledgements();

    /// <summary>Disables subsequent calls to <see cref="RenderFrame"/>.</summary>
    public void Dispose() => disposed = true;
}
