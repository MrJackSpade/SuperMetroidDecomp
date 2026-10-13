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
    /// <summary>PCM frames rendered per second by the desktop audio adapter.</summary>
    public const int SampleRate = 48_000;
    /// <summary>Samples per channel generated for one sixty-hertz gameplay update.</summary>
    public const int StereoFramesPerVideoFrame = SampleRate / 60;
    /// <summary>Left and right channels in the interleaved PCM output.</summary>
    public const int ChannelCount = 2;

    /// <summary>Resolves SPC uploads from installed audio assets and renders managed APU output.</summary>
    private readonly CartridgeAudioRenderer renderer;
    /// <summary>Prevents rendering after the desktop owner releases this adapter.</summary>
    private bool disposed;

    /// <summary>Binds extracted audio data and the APU state that can be saved and restored.</summary>
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

    /// <summary>Returns APU acknowledgements produced while rendering the last audio frame.</summary>
    public CartridgeAudioAcknowledgements ReadAcknowledgements() => renderer.ReadAcknowledgements();

    /// <summary>Marks the adapter unavailable to subsequent frame rendering.</summary>
    public void Dispose() => disposed = true;
}
