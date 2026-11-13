namespace SuperMetroid.Core.Audio;

/// <summary>
/// The sound and music queues a session's gameplay requests go straight into as they are made, behind the game state's own
/// test of whether gameplay may queue audio at all.
/// </summary>
internal interface IGameplayAudioQueues
{
    /// <summary>Requests <paramref name="request"/>'s sound from its library's queue, returning what the queue did with it.</summary>
    SoundQueueAdmission QueueSound(SoundRequest request);

    /// <summary>Puts <paramref name="request"/> on the music queue.</summary>
    void QueueMusic(GameplayMusicRequest request);
}
