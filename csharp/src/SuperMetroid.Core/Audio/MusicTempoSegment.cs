namespace SuperMetroid.Core.Audio;

/// <summary>
/// From <paramref name="Frame"/> of a music clip, the driver adds <paramref name="Tempo"/> to its
/// eight-bit tempo accumulator on every driver tick; each carry is a music tick, where the driver
/// consumes a pending music command (#1305).
/// </summary>
/// <param name="Frame">Clip-relative stereo frame at which this tempo takes effect.</param>
/// <param name="Tempo">The tempo's whole byte, added per driver tick.</param>
public sealed record MusicTempoSegment(long Frame, byte Tempo);
