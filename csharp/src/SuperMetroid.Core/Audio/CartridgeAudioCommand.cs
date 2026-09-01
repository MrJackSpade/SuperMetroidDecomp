namespace SuperMetroid.Core.Audio;

/// <summary>The two operations the 65816 can publish to Super Metroid's audio CPU.</summary>
public enum CartridgeAudioCommandKind : byte
{
    /// <summary>Upload one cartridge music/SFX data stream into SPC RAM.</summary>
    Upload,

    /// <summary>Write one byte to SNES APU input port $2140-$2143.</summary>
    WritePort,
}

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
    byte Port,
    byte Value)
{
    public static CartridgeAudioCommand Upload(int cartridgeAddress) =>
        new(CartridgeAudioCommandKind.Upload, cartridgeAddress, 0, 0);

    public static CartridgeAudioCommand WritePort(byte port, byte value) =>
        new(CartridgeAudioCommandKind.WritePort, 0, port, value);
}

/// <summary>Bytes returned by the SPC to the 65816 through APU ports $2140-$2143.</summary>
public readonly record struct CartridgeAudioAcknowledgements(
    byte Port0,
    byte Port1,
    byte Port2,
    byte Port3)
{
    public byte this[int port] => port switch
    {
        0 => Port0,
        1 => Port1,
        2 => Port2,
        3 => Port3,
        _ => throw new ArgumentOutOfRangeException(nameof(port), port, "APU port must be 0..3."),
    };
}
