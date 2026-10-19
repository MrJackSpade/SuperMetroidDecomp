namespace SuperMetroid.Core.Audio;

/// <summary>
/// From <paramref name="Frame"/> of a music clip, the driver adds <paramref name="Tempo"/> to its
/// eight-bit tempo accumulator on every driver tick; each carry is a music tick, where the driver
/// consumes a pending music command (#1305).
/// </summary>
/// <param name="Frame">Clip-relative stereo frame at which this tempo takes effect.</param>
/// <param name="Tempo">Counts added to the tempo accumulator per driver tick, 0 through 255.</param>
public sealed record MusicTempoSegment(long Frame, int Tempo)
{
    /// <summary>The tempo, validated to the counts one driver tick can add.</summary>
    public int Tempo { get; init; } = Ensure.BetweenInclusive(0, TickAccumulator.Period - 1, Tempo);
}
