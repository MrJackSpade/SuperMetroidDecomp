namespace SuperMetroid.Core.Audio;

/// <summary>
/// One operation produced by the translated bank-$80 audio dispatcher: a CPU-to-APU port
/// write or a bank upload. These stay inside Core; <see cref="CartridgeAudioSession"/> applies
/// them to its <see cref="SpcCommandPorts"/> and publishes <see cref="AudioEvent"/>s instead (#1304).
/// </summary>
/// <remarks>
/// Port zero selects music; ports one through three are the retail sound-effect libraries.
/// Unused fields are zero for a given command kind.
/// </remarks>
internal readonly record struct CartridgeAudioCommand(
    CartridgeAudioCommandKind Kind,
    AudioBank Bank,
    [property: SpcMemoryAccess] byte Port,
    byte Value)
{
    /// <summary>Creates an upload command installing one audio bank.</summary>
    public static CartridgeAudioCommand Upload(AudioBank bank) =>
        new(CartridgeAudioCommandKind.Upload, bank, 0, 0);

    /// <summary>Creates a CPU-to-APU write for one of ports zero through three.</summary>
    [SpcMemoryBoundary("codec: a port command stores its port number as the debugger-visible byte")]
    public static CartridgeAudioCommand WritePort(ApuPort port, byte value) =>
        new(CartridgeAudioCommandKind.WritePort, 0, (byte)port, value);

    /// <summary>The CPU-to-APU input port a <see cref="CartridgeAudioCommandKind.WritePort"/> command writes.</summary>
    [SpcMemoryBoundary("codec: decodes the stored port number")]
    public ApuPort InputPort => Enum.IsDefined((ApuPort)Port)
        ? (ApuPort)Port
        : throw new InvalidDataException($"APU input port {Port} is outside $2140-$2143.");
}
