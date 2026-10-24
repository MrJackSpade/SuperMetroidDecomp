namespace SuperMetroid.Core.Audio;

/// <summary>
/// One music track rendered from its data set's state just after loading (#1305). Frame zero is the
/// driver tick that started the track. A looping track plays its whole clip and then repeats
/// [<see cref="LoopStart"/>, end); an ending track stops at its last frame, where the driver
/// returned the music port to zero.
/// </summary>
/// <param name="AudioSet">The music data set that holds the track.</param>
/// <param name="Track">The track number written to the music port.</param>
/// <param name="Clip">The rendered clip.</param>
/// <param name="LoopStart">The loop's first frame, or null for a track that ends.</param>
/// <param name="TempoSegments">The tempo in force from each frame on, in frame order, starting at frame zero.</param>
public sealed record MusicClipEntry(
    MusicDataSet AudioSet,
    int Track,
    AudioClipFile Clip,
    long? LoopStart,
    IReadOnlyList<MusicTempoSegment> TempoSegments);
