using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;

/// <summary>
/// Presents a fixture-owned byte per APU port as the SPC's acknowledgement, ignoring the
/// session's port model. Fixtures that echo each CPU port write back on the next update
/// isolate the 65816 handshake from the SPC poll phase.
/// </summary>
/// <param name="ports">The four port bytes, updated by the fixture between updates.</param>
internal sealed class PortWriteEchoAcknowledgements(byte[] ports) : ISoundAcknowledgementSource
{
    /// <summary>Returns the fixture-owned port bytes, ignoring the modeled acknowledgements.</summary>
    public CartridgeAudioAcknowledgements ForNextFrame(CartridgeAudioAcknowledgements modeled) =>
        new(ports[0], ports[1], ports[2], ports[3]);
}
