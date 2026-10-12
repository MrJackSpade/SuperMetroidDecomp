using SuperMetroid.Core;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Desktop;

/// <summary>
/// Desktop adapter for the fully managed Super Metroid SPC driver and S-DSP mixer.
/// The adapter resolves upload commands through extracted assets and owns the reusable host PCM buffer;
/// all sequencer, voice, echo, pitch, and envelope behavior lives in debuggable Core code.
/// </summary>
internal sealed class SpcAudioEngine : IDisposable
{
    /// <summary>Stores sample rate.</summary>
    public const int SampleRate = 48_000;
    /// <summary>Stores stereo frames per video frame.</summary>
    public const int StereoFramesPerVideoFrame = SampleRate / 60;
    /// <summary>Stores channel count.</summary>
    public const int ChannelCount = 2;

    /// <summary>Stores renderer.</summary>
    private readonly CartridgeAudioRenderer renderer;
    /// <summary>Stores disposed.</summary>
    private bool disposed;

    /// <summary>Initializes a new spc audio engine instance.</summary>
    internal SpcAudioEngine(ExtractedAudioAssetCatalog assets, ManagedSpcPlayer player)
    {
        renderer = new CartridgeAudioRenderer(assets, Ensure.NotNull(player));
    }

    /// <summary>The exact managed APU state persisted with debugger save states.</summary>
    internal ManagedSpcPlayer Player => renderer.Player;

    /// <summary>Applies this NMI's APU operations, then renders one complete audio frame.</summary>
    public ReadOnlySpan<short> RenderFrame(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        Ensure.NotNull(commands);
        return renderer.RenderFrame(commands);
    }

    /// <summary>Reads acknowledgements.</summary>
    public CartridgeAudioAcknowledgements ReadAcknowledgements() => renderer.ReadAcknowledgements();

    /// <summary>Disposes.</summary>
    public void Dispose() => disposed = true;
}
