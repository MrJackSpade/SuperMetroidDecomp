namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Presents each sound-effect port's latest CPU write as the SPC's acknowledgement, ignoring the
/// session's port model. Echoing every write on the next update isolates the 65816 handshake
/// from the SPC poll phase.
/// </summary>
/// <remarks>
/// The dispatcher's current request byte for a library is exactly the value it last wrote to
/// that library's port: every send, rewrite, clear and downtime write updates both together.
/// </remarks>
/// <param name="audio">The dispatcher whose port writes are echoed.</param>
internal sealed class PortWriteEchoAcknowledgements(CartridgeAudioState audio) : ISoundAcknowledgementSource
{
    /// <summary>Returns the dispatcher's latest port writes, ignoring the modeled acknowledgements.</summary>
    public CartridgeAudioAcknowledgements ForNextFrame(CartridgeAudioAcknowledgements modeled) =>
        new(0, audio.CurrentSoundForVerification(0), audio.CurrentSoundForVerification(1),
            audio.CurrentSoundForVerification(2));
}
