using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Core.Audio;

/// <summary>
/// The game session's audio path with the emulated driver as its backend, for synthesizer
/// verification (#1305): one update's commands go through <see cref="CartridgeAudioEventSource"/>
/// to a <see cref="ManagedSpcAudioBackend"/>, exactly as a session's do to its bound backend.
/// </summary>
/// <param name="assets">The extracted catalog the synthesizer installs on bank loads.</param>
internal sealed class SynthesizingAudioSession(ExtractedAudioAssetCatalog assets)
{
    /// <summary>The port model producing each update's events and acknowledgements.</summary>
    private readonly CartridgeAudioEventSource events = new();

    /// <summary>The emulated driver rendering the events.</summary>
    private readonly ManagedSpcAudioBackend backend = new(assets);

    /// <summary>The most recent update's interleaved 48-kHz PCM; silence before the first.</summary>
    private readonly short[] lastPcm = new short[CartridgeAudioSession.PcmSamplesPerFrame];

    /// <summary>The acknowledgement bytes the most recent update published.</summary>
    internal SoundAcknowledgements Acknowledgements => events.Acknowledgements;

    /// <summary>The emulated driver the backend plays through.</summary>
    internal ManagedSpcPlayer PlayerForVerification => backend.Player;

    /// <summary>Applies one update's commands, renders its PCM and returns its events.</summary>
    internal IReadOnlyList<AudioEvent> Advance(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        AudioEvent[] frameEvents = events.Advance(commands);
        backend.Render(new AudioFrameEvents(frameEvents), lastPcm);
        return frameEvents;
    }

    /// <summary>Copies the most recent update's PCM.</summary>
    internal void CopyFrame(Span<short> destination) => lastPcm.CopyTo(destination);

    /// <summary>Applies one update's commands and returns a copy of its PCM.</summary>
    internal short[] RenderFrame(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        Advance(commands);
        return (short[])lastPcm.Clone();
    }
}
