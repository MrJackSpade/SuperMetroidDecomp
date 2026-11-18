namespace SuperMetroid.Core.Audio;

/// <summary>
/// The sound queues a session's gameplay sound requests go straight into as they are made (#2499), behind the game state's
/// own test of whether gameplay may queue sounds at all.
/// </summary>
internal interface IGameplaySoundQueue
{
    /// <summary>Requests <paramref name="request"/>'s sound from its library's queue, returning what the queue did with it.</summary>
    SoundQueueAdmission QueueSound(SoundRequest request);
}
