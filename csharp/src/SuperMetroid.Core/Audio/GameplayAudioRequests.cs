namespace SuperMetroid.Core.Audio;

/// <summary>
/// The way gameplay requests sounds and music (#1620): every gameplay system makes its request here the moment it makes
/// it, and the request goes straight on into the queues these requests reach, so the queues receive an update's requests
/// in its run order across every system, as each of the game's systems queues its own directly while they run.
/// </summary>
/// <remarks>
/// The requests always reach queues (#2057): a runtime's reach its session's, a cinematic's the frontend's, a check's queues
/// of its own; every system that makes sounds takes the requests it joins when it is built, so no request is ever made
/// that reaches no queue. Evidence for the run order: the game's main gameplay update runs the colour effects, Samus's
/// input handling (which fires her weapons), the projectiles' hits on enemies, the enemies, Samus's movement and
/// animation, the enemy projectiles, the room objects, the enemy projectiles' hits, the power bomb's hits, the drawing and
/// the HUD in turn, and each of them calls the game's sound and music queues at the point it runs; none holds a request
/// back for a later system.
/// </remarks>
/// <param name="queues">The queues every request goes on into as it is made.</param>
internal sealed class GameplayAudioRequests(IGameplayAudioQueues queues)
{
    /// <summary>The queues the requests go on into.</summary>
    private readonly IGameplayAudioQueues queues = Ensure.NotNull(queues);

    /// <summary>
    /// Requests <paramref name="request"/>'s sound now, after the update's earlier sound requests, returning what its queue
    /// did with it.
    /// </summary>
    internal SoundQueueAdmission QueueSound(SoundRequest request) => queues.QueueSound(request);

    /// <summary>Requests <paramref name="soundEffect"/> now (see <see cref="QueueSound(SoundRequest)"/>).</summary>
    /// <param name="soundEffect">The sound.</param>
    /// <param name="priority">How full its queue may be for the request to join it.</param>
    internal SoundQueueAdmission QueueSound(SoundEffectId soundEffect, SoundRequestPriority priority) =>
        QueueSound(new SoundRequest(soundEffect, priority));

    /// <summary>Schedules <paramref name="request"/> now, after the update's earlier music requests.</summary>
    internal void ScheduleMusic(DelayedMusicRequest request) => QueueMusic(new GameplayMusicRequest.Scheduled(request));

    /// <summary>Starts the permanent-item fanfare's music sequence now, after the update's earlier music requests.</summary>
    internal void QueuePermanentItemFanfare() => QueueMusic(new GameplayMusicRequest.PermanentItemFanfare());

    /// <summary>Puts <paramref name="request"/> on the music queue.</summary>
    private void QueueMusic(GameplayMusicRequest request) => queues.QueueMusic(request);
}
