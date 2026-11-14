namespace SuperMetroid.Core.Audio;

/// <summary>The music request the music queue fetched and is waiting out the delay of (#1604).</summary>
/// <param name="Request">The request dispatched when the wait ends.</param>
/// <param name="CallsUntilSent">Music-queue passes left; the pass that brings it to zero sends the request.</param>
internal sealed record PendingMusicDispatch(MusicRequest Request, int CallsUntilSent);
