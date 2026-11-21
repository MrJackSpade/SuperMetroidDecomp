namespace SuperMetroid.Core.Audio;

/// <summary>
/// The way gameplay requests sounds and music (#1620): the sound requests every system makes (<see cref="GameplaySoundRequests"/>),
/// and the music requests the enemies and the room objects make, each going straight on into the music queue the moment it
/// is made, after the update's earlier music requests. A runtime's requests reach its session's queues; a check's, queues of
/// its own.
/// </summary>
/// <remarks>
/// Evidence for the run order: each of the game's systems calls the game's music queue at the point it runs; none holds a
/// request back for a later system.
/// </remarks>
/// <param name="sounds">The sound queues every sound request goes on into as it is made.</param>
/// <param name="music">The music queue every music request goes on into as it is made.</param>
internal sealed class GameplayAudioRequests(IGameplaySoundQueue sounds, IGameplayMusicQueue music) : GameplaySoundRequests(sounds)
{
    /// <summary>The music queue the requests go on into.</summary>
    internal readonly IGameplayMusicQueue music = Ensure.NotNull(music);

    /// <summary>Schedules <paramref name="request"/> now, after the update's earlier music requests.</summary>
    internal void ScheduleMusic(DelayedMusicRequest request) => QueueMusic(new GameplayMusicRequest.Scheduled(request));

    /// <summary>Starts the permanent-item fanfare's music sequence now, after the update's earlier music requests.</summary>
    internal void QueuePermanentItemFanfare() => QueueMusic(new GameplayMusicRequest.PermanentItemFanfare());

    /// <summary>Puts <paramref name="request"/> on the music queue.</summary>
    private void QueueMusic(GameplayMusicRequest request) => music.QueueMusic(request);
}
