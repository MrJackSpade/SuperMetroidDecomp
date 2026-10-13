namespace SuperMetroid.Core.Audio;

/// <summary>
/// Development-tool reads of a synthesizer's own SPC output ports; never linked by player hosts.
/// Gameplay reads acknowledgements only from the session's <see cref="SpcCommandPorts"/> model.
/// </summary>
internal static class CartridgeAudioRendererTooling
{
    /// <summary>Snapshots the renderer's SPC-to-CPU bytes for APU ports $2140-$2143 without advancing synthesis.</summary>
    internal static CartridgeAudioAcknowledgements ReadAcknowledgements(this CartridgeAudioRenderer renderer) => new(
        renderer.player.ReadPort(ApuPort.Music), renderer.player.ReadPort(ApuPort.SoundLibrary1),
        renderer.player.ReadPort(ApuPort.SoundLibrary2), renderer.player.ReadPort(ApuPort.SoundLibrary3));

    /// <summary>Reads one of the player's SPC-to-CPU output ports: the sequencer's music echo or a sound-effect acknowledgement.</summary>
    internal static byte ReadPort(this ManagedSpcPlayer player, ApuPort port) => port == ApuPort.Music
        ? player.musicPortToSnes
        : player.commandPorts.ReadSoundEffectPort(port);
}
