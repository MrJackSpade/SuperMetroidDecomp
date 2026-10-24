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
internal sealed class PortWriteEchoAcknowledgements : ISoundAcknowledgementTiming<SoundEffectId>
{
    /// <summary>The stateless echo policy.</summary>
    internal static PortWriteEchoAcknowledgements Instance { get; } = new();

    /// <summary>Prevents instances other than <see cref="Instance"/>.</summary>
    private PortWriteEchoAcknowledgements() { }

    /// <summary>Returns the dispatcher's latest port writes, ignoring the modeled acknowledgements.</summary>
    public SoundAcknowledgements<SoundEffectId> ForNextUpdate(SoundAcknowledgements<SoundEffectId> modeled, SoundAcknowledgements<SoundEffectId> portWrites) =>
        portWrites;
}
