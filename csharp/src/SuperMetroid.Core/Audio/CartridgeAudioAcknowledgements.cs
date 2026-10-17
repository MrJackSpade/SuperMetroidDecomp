namespace SuperMetroid.Core.Audio;

/// <summary>
/// What the sound driver last echoed back on each of the four APU ports: the request each port's
/// library acted on, which the sound dispatcher compares with the request it is waiting on.
/// </summary>
/// <param name="Port0">The music port's echo, 0 through 255; gameplay never reads it.</param>
/// <param name="Port1">Library one's echoed request, 0 through 255.</param>
/// <param name="Port2">Library two's echoed request, 0 through 255.</param>
/// <param name="Port3">Library three's echoed request, 0 through 255.</param>
public readonly record struct CartridgeAudioAcknowledgements(
    int Port0,
    int Port1,
    int Port2,
    int Port3)
{
    /// <summary>The music port's echo, validated to the range a port carries.</summary>
    public int Port0 { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, Port0);

    /// <summary>Library one's echoed request, validated to the range a port carries.</summary>
    public int Port1 { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, Port1);

    /// <summary>Library two's echoed request, validated to the range a port carries.</summary>
    public int Port2 { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, Port2);

    /// <summary>Library three's echoed request, validated to the range a port carries.</summary>
    public int Port3 { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, Port3);

    /// <summary>Gets the echo on APU output port zero through three.</summary>
    public int this[ApuPort port] => port switch
    {
        ApuPort.Music => Port0,
        ApuPort.SoundLibrary1 => Port1,
        ApuPort.SoundLibrary2 => Port2,
        ApuPort.SoundLibrary3 => Port3,
        _ => throw new InvalidOperationException($"Undefined {nameof(ApuPort)} {(int)port}."),
    };
}
