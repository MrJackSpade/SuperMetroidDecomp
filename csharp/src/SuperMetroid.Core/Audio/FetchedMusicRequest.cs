namespace SuperMetroid.Core.Audio;

/// <summary>The music request the music queue fetched and is waiting out the delay of (#1604).</summary>
/// <param name="Request">The request sent when the wait ends.</param>
/// <param name="Wait">The music-queue passes the request waits, counted once a pass; the pass that ends it sends the request.</param>
internal sealed record FetchedMusicRequest(MusicRequest Request, UpdateCountdown Wait);
