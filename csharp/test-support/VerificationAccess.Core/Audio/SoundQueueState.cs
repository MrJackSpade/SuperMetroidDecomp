namespace SuperMetroid.Core.Audio;

/// <summary>A sound queue's start and next positions and its library handler's handshake phase.</summary>
/// <param name="Start">The start.</param>
/// <param name="Next">The next.</param>
/// <param name="State">Where the library's handler is in its handshake.</param>
internal readonly record struct SoundQueueState(int Start, int Next, SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.Phase State)
{
    /// <summary>
    /// Decodes a recorded native handler state, 0 through 3 in handshake order (ready, awaiting acceptance, holding,
    /// awaiting the withdrawal's acceptance), for comparison with the port's phase.
    /// </summary>
    /// <param name="value">The recorded state.</param>
    /// <exception cref="InvalidDataException"><paramref name="value"/> names no handshake phase.</exception>
    internal static SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.Phase PhaseFromNative(int value) => value switch
    {
        0 => SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.Phase.ReadyToRequest,
        1 => SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.Phase.AwaitingAcceptance,
        2 => SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.Phase.HoldingBeforeWithdrawal,
        3 => SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.Phase.AwaitingWithdrawalAcceptance,
        _ => throw new InvalidDataException($"Recorded sound handler state {value} names no handshake phase."),
    };
}
