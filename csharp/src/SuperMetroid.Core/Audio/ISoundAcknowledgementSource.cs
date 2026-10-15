namespace SuperMetroid.Core.Audio;

/// <summary>
/// Supplies the SPC acknowledgement bytes that bank $82's sound handlers read during an update.
/// </summary>
/// <remarks>
/// <c>HandleSounds</c> ($82:89EF) waits for the SPC to echo each sent request and its following
/// zero. The session's synthesis-free <see cref="SpcCommandPorts"/> model produces those echoes
/// every update, whether or not a host renders audio. Verification replays whose native capture
/// shows a different echo timing (the cartridge's CPU load moves its port writes against the SPC
/// poll; the port removes that load) substitute the captured outcome here.
/// </remarks>
public interface ISoundAcknowledgementSource
{
    /// <summary>The acknowledgements the next update's sound handlers read.</summary>
    /// <param name="modeled">The bytes the session's SPC port model published after the previous update.</param>
    /// <returns>The bytes to present to the update; only the three sound-effect ports are read.</returns>
    CartridgeAudioAcknowledgements ForNextFrame(CartridgeAudioAcknowledgements modeled);
}
