namespace SuperMetroid.Core.Audio;

/// <summary>
/// One entry of a song's tempo map (#1305, #1478): from <paramref name="Frame"/> of the song's clip, every sound clock
/// step advances the music's beat by <paramref name="Tempo"/> of a beat's <see cref="ClipPlaybackState.BeatParts"/>
/// parts. A song change waits for the beat to complete, so songs change in time with the music that plays.
/// </summary>
/// <param name="Frame">Clip-relative frame at which this tempo takes effect.</param>
/// <param name="Tempo">Parts of a beat each clock step advances, 0 through <see cref="ClipPlaybackState.BeatParts"/> - 1.</param>
public sealed record SongTempo(long Frame, int Tempo)
{
    /// <summary>The tempo, validated to less than one whole beat per clock step.</summary>
    public int Tempo { get; init; } = Ensure.BetweenInclusive(0, ClipPlaybackState.BeatParts - 1, Tempo);
}
