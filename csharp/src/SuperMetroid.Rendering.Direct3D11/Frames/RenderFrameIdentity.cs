namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The host's identity for a frame it hands to presentation, independent of the game's frame number, which wraps and
/// rewinds when the host restores a game. Sequence increases for every frame the host publishes; generation increases
/// when the host loads or resets a game.
/// </summary>
public readonly record struct RenderFrameIdentity
{
    /// <summary>Creates an identity from a positive sequence and generation.</summary>
    /// <param name="sequence">Positive publication sequence, kept increasing across load/reset generations.</param>
    /// <param name="generation">Positive load/reset generation used to reject stale queued frames.</param>
    /// <exception cref="ArgumentOutOfRangeException">The sequence or generation is zero or negative.</exception>
    public RenderFrameIdentity(long sequence, long generation)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(generation);
        Sequence = sequence;
        Generation = generation;
    }

    /// <summary>Positive publication sequence, kept increasing across load/reset generations.</summary>
    public long Sequence { get; }

    /// <summary>Positive load/reset generation used to reject stale queued frames.</summary>
    public long Generation { get; }
}
