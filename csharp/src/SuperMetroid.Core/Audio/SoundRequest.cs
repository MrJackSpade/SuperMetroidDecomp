namespace SuperMetroid.Core.Audio;

/// <summary>
/// One request for the sound queue, whichever system made it. Whether the game turns it away is the queue's test, made
/// as the request reaches it (<see cref="SoundQueueAdmission"/>), never the requester's.
/// </summary>
/// <param name="SoundEffect">The library-qualified sound identity the request names.</param>
/// <param name="Priority">How full the sound's queue may be for the request to join it, as the requesting system chose.</param>
public readonly partial record struct SoundRequest(SoundEffectId SoundEffect, SoundRequestPriority Priority);
