namespace SuperMetroid.Core.Audio;

/// <summary>A music request waiting on the music queue with the delay it waits once the music handler fetches it (#1604).</summary>
/// <param name="Request">What plays, stops or loads when the delay runs out.</param>
/// <param name="Delay">The music-handler calls the request waits after it is fetched.</param>
internal sealed record QueuedMusic(MusicRequest Request, MusicQueueDelay Delay);
