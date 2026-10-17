namespace SuperMetroid.Core.Audio;

/// <summary>
/// One operation produced by the translated bank-$80 audio dispatcher: a CPU-to-APU port
/// write or a bank upload. These stay inside Core; <see cref="CartridgeAudioSession"/> applies
/// them to its <see cref="SpcCommandPorts"/> and publishes <see cref="AudioEvent"/>s instead (#1304).
/// </summary>
/// <remarks>
/// Port zero selects music; ports one through three are the retail sound-effect libraries.
/// Fields that do not belong to a command's kind hold their defaults.
/// </remarks>
/// <param name="Kind">Whether the command uploads a bank or writes a port.</param>
/// <param name="Bank">The bank an upload installs.</param>
/// <param name="InputPort">The CPU-to-APU input port a port write targets.</param>
/// <param name="Value">
/// What a port write carries: a track for the music port, a library-relative request for a
/// sound-effect port, zero to clear either.
/// </param>
internal readonly record struct CartridgeAudioCommand(
    CartridgeAudioCommandKind Kind,
    AudioBank Bank,
    ApuPort InputPort,
    byte Value)
{
    /// <summary>Creates an upload command installing one audio bank.</summary>
    public static CartridgeAudioCommand Upload(AudioBank bank) =>
        new(CartridgeAudioCommandKind.Upload, bank, default, 0);

    /// <summary>Creates a CPU-to-APU write for one of ports zero through three.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside 0 through 255.</exception>
    public static CartridgeAudioCommand WritePort(ApuPort port, int value) =>
        new(CartridgeAudioCommandKind.WritePort, default, port, (byte)Ensure.BetweenInclusive(0, byte.MaxValue, value));
}
