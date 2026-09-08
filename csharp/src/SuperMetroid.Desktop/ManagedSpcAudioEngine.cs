using SuperMetroid.Core.Audio;

namespace SuperMetroid.Desktop;

/// <summary>
/// Desktop adapter for the fully managed Super Metroid SPC driver and S-DSP mixer.
/// The adapter resolves upload commands through extracted assets and owns the reusable host PCM buffer;
/// all sequencer, voice, echo, pitch, and envelope behavior lives in debuggable Core code.
/// </summary>
internal sealed class SpcAudioEngine : IDisposable
{
    public const int SampleRate = 48_000;
    public const int StereoFramesPerVideoFrame = SampleRate / 60;
    public const int ChannelCount = 2;

    private readonly CartridgeAudioRenderer renderer;
    private bool disposed;

    public SpcAudioEngine() : this(
        ExtractedAudioAssetCatalog.Load(ExtractedAudioAssetLocator.FindAudioDirectory()),
        new ManagedSpcPlayer())
    {
    }

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

    public CartridgeAudioAcknowledgements ReadAcknowledgements() => renderer.ReadAcknowledgements();

    public void Dispose() => disposed = true;
}
