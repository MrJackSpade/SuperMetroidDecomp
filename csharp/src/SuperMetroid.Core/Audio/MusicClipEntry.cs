namespace SuperMetroid.Core.Audio;

/// <summary>
/// One song of a clip set, rendered as it plays just after the set is loaded (#1305, #1478). Frame zero is the
/// beat at which the song started. A looping song plays its whole clip and then repeats
/// [<see cref="LoopStart"/>, end); an ending song stops at its last frame, leaving no song playing.
/// </summary>
/// <param name="AudioSet">The music data set whose clip set holds the song.</param>
/// <param name="Track">The song's track number within its data set (<see cref="MusicTrack"/>).</param>
/// <param name="Clip">The rendered clip.</param>
/// <param name="LoopStart">The loop's first frame, or null for a song that ends.</param>
/// <param name="TempoSegments">The song's tempo map: the tempo in force from each frame on, in frame order, starting at frame zero.</param>
public sealed record MusicClipEntry(
    MusicDataSet AudioSet,
    int Track,
    AudioClipFile Clip,
    long? LoopStart,
    IReadOnlyList<SongTempo> TempoSegments);
