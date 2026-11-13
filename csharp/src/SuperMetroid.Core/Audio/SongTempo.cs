namespace SuperMetroid.Core.Audio;

/// <summary>
/// One entry of a song's tempo map (#1305, #1478, #1725): from <paramref name="Frame"/> of the song's clip, the music's
/// beat lasts <paramref name="Beat"/>. A song change waits for the beat to complete, so songs change in time with the
/// music that plays.
/// </summary>
/// <param name="Frame">Clip-relative frame at which this beat length takes effect.</param>
/// <param name="Beat">How long the beat lasts from that frame on.</param>
public sealed record SongTempo(long Frame, BeatLength Beat)
{
    /// <summary>The beat's length, required.</summary>
    public BeatLength Beat { get; init; } = Ensure.NotNull(Beat);
}
