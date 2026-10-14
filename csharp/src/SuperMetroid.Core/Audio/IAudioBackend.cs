namespace SuperMetroid.Core.Audio;

/// <summary>
/// Produces host PCM from one update's semantic audio events (#1304). Backends are
/// presentation only: gameplay reads its sound-effect acknowledgements from the session's
/// <see cref="SpcCommandPorts"/>, never from a backend, so a backend has no acknowledgement
/// output and does not own the driver timer.
/// </summary>
public interface IAudioBackend
{
    /// <summary>
    /// Applies one update's events in order and renders that update's
    /// <see cref="CartridgeAudioSession.PcmSamplesPerFrame"/> interleaved 48-kHz stereo samples.
    /// </summary>
    /// <param name="frame">The update's ordered events.</param>
    /// <param name="stereo48k">Receives the update's interleaved left/right samples.</param>
    /// <exception cref="ArgumentException"><paramref name="stereo48k"/> is shorter than one update's samples.</exception>
    /// <exception cref="InvalidDataException">The events are out of order or name content the backend cannot play.</exception>
    void Render(in AudioFrameEvents frame, Span<short> stereo48k);
}
