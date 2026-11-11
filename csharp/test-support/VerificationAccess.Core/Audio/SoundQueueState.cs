using System.Collections.Immutable;

namespace SuperMetroid.Core.Audio;

/// <summary>A sound library's waiting requests, oldest first, and its handler's handshake phase.</summary>
/// <param name="Waiting">The requests waiting in the library's queue, oldest first.</param>
/// <param name="State">Where the library's handler is in its handshake.</param>
internal readonly record struct SoundQueueState(ImmutableArray<SoundEffectId> Waiting, SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.Phase State)
{
    /// <summary>Whether both hold the same waiting requests in the same order and the same handshake phase.</summary>
    /// <param name="other">The state compared with this one.</param>
    public bool Equals(SoundQueueState other) => State == other.State && Waiting.SequenceEqual(other.Waiting);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(State, Waiting.Length);

    /// <inheritdoc />
    public override string ToString() => $"[{string.Join(",", Waiting)}]/{State}";

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
