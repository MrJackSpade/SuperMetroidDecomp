namespace SuperMetroid.Core.Audio;

/// <summary>
/// What the sound driver last echoed back on each of the four APU ports: the request each port's
/// library acted on, which the sound dispatcher compares with the request it is waiting on.
/// </summary>
public readonly record struct CartridgeAudioAcknowledgements
{
    /// <summary>Each port's echo, 0 through 255, indexed in register order.</summary>
    private readonly ApuPortEchoes echoes;

    /// <summary>Records each port's echo, validated to the range a port carries.</summary>
    /// <param name="music">The music port's echo; gameplay never reads it.</param>
    /// <param name="library1">Library one's echoed request.</param>
    /// <param name="library2">Library two's echoed request.</param>
    /// <param name="library3">Library three's echoed request.</param>
    public CartridgeAudioAcknowledgements(int music, int library1, int library2, int library3)
    {
        int[] values = [music, library1, library2, library3];
        for (int port = 0; port < values.Length; port++)
            echoes[port] = Ensure.BetweenInclusive(0, byte.MaxValue, values[port]);
    }

    /// <summary>Gets the echo on APU output port <paramref name="port"/>.</summary>
    public int this[ApuPort port] => echoes[ApuPorts.RegisterIndex(port)];
}
