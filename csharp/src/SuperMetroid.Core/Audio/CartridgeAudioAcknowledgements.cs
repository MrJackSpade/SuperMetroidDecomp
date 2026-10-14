namespace SuperMetroid.Core.Audio;

/// <summary>Bytes returned by the SPC to the 65816 through APU ports $2140-$2143.</summary>
public readonly record struct CartridgeAudioAcknowledgements(
    byte Port0,
    byte Port1,
    byte Port2,
    byte Port3)
{
    /// <summary>Gets the acknowledgement byte for APU output port zero through three.</summary>
    public byte this[ApuPort port] => port switch
    {
        ApuPort.Music => Port0,
        ApuPort.SoundLibrary1 => Port1,
        ApuPort.SoundLibrary2 => Port2,
        ApuPort.SoundLibrary3 => Port3,
        _ => throw new InvalidOperationException($"Undefined {nameof(ApuPort)} {(int)port}."),
    };
}
