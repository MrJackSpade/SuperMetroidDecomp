namespace SuperMetroid.Core.Audio;

/// <summary>
/// One operation produced by the translated bank-$80 audio dispatcher: a CPU-to-APU port
/// write or a bank upload. These stay inside Core; <see cref="CartridgeAudioSession"/> applies
/// them to its <see cref="SoundAcceptanceClock"/> and publishes <see cref="AudioEvent"/>s instead (#1304).
/// </summary>
/// <remarks>Port zero selects music; ports one through three are the retail sound-effect libraries.</remarks>
internal abstract record CartridgeAudioCommand
{
    /// <summary>Closes the hierarchy to the commands declared here.</summary>
    private CartridgeAudioCommand()
    {
    }

    /// <summary>Uploads one cartridge music/SFX data stream into SPC RAM.</summary>
    /// <param name="Bank">The bank the upload installs.</param>
    internal sealed record Upload(AudioBank Bank) : CartridgeAudioCommand;

    /// <summary>Writes one byte to SNES APU input port $2140-$2143.</summary>
    /// <param name="InputPort">The CPU-to-APU input port written.</param>
    /// <param name="Value">
    /// What the write carries: a track for the music port, a library-relative request for a
    /// sound-effect port, zero to clear either; 0 through 255.
    /// </param>
    internal sealed record WritePort(ApuPort InputPort, int Value) : CartridgeAudioCommand
    {
        /// <inheritdoc cref="WritePort(ApuPort, int)" path="/param[@name='Value']"/>
        public int Value { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Value);
    }
}
