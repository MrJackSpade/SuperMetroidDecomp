namespace SuperMetroid.Core.Audio;

/// <summary>One music request a game system publishes for the music queue, with how long it waits there.</summary>
/// <param name="Request">The music request.</param>
/// <param name="Delay">How long the request waits on the queue before it plays.</param>
public readonly record struct DelayedMusicRequest(MusicRequest Request, MusicQueueDelay Delay);
