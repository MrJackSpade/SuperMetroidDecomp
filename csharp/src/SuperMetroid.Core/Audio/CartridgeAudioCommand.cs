namespace SuperMetroid.Core.Audio;

/// <summary>
/// One debugger-visible operation produced by the translated bank-$80 audio dispatcher.
/// </summary>
/// <remarks>
/// Upload commands retain the 24-bit cartridge source address instead of copying opaque
/// blobs into every frontend frame. Port zero selects music; ports one through three are
/// the retail sound-effect libraries. Unused fields are zero for a given command kind.
/// </remarks>
public readonly record struct CartridgeAudioCommand(
    CartridgeAudioCommandKind Kind,
    int UploadAddress,
    [property: SpcMemoryAccess] byte Port,
    byte Value)
{
    /// <summary>Creates an upload command retaining the 24-bit cartridge stream identity.</summary>
    public static CartridgeAudioCommand Upload(int cartridgeAddress) =>
        new(CartridgeAudioCommandKind.Upload, cartridgeAddress, 0, 0);

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
