namespace SuperMetroid.Core.Audio;

/// <summary>
/// One music track rendered from a bank's canonical post-upload state (#1305). Frame zero is the
/// driver tick that started the track. A looping track plays its whole clip and then repeats
/// [<see cref="LoopStart"/>, end); an ending track stops at its last frame, where the driver
/// returned the music port to zero.
/// </summary>
/// <param name="AudioSet">The music bank whose upload holds the track.</param>
/// <param name="Track">The track number written to the music port.</param>
/// <param name="Clip">The rendered clip.</param>
/// <param name="LoopStart">The loop's first frame, or null for a track that ends.</param>
/// <param name="TempoSegments">The tempo in force from each frame on, in frame order, starting at frame zero.</param>
public sealed record MusicClipEntry(
    AudioBank AudioSet,
    int Track,
    AudioClipFile Clip,
    long? LoopStart,
    IReadOnlyList<MusicTempoSegment> TempoSegments);
