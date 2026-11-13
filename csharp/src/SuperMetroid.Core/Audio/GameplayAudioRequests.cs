namespace SuperMetroid.Core.Audio;

/// <summary>
/// The sound and music requests gameplay makes in one update, in the order it makes them (#1620). Every gameplay system
/// queues its request here the moment it makes it, and a session's requests go straight on into its sound and music queues,
/// so the queues receive them in the update's run order across every system, as the game's callers each queue theirs
/// directly while they run.
/// </summary>
/// <remarks>
/// The record holds only the current update's requests: <see cref="BeginUpdate"/> clears it when the next update begins,
/// so an update that runs only some of its systems records only theirs and nothing is ever queued twice. A record reaches
/// a session's queues once it is attached to them (<see cref="AttachTo"/>); until then, as for a system built on its own
/// outside a runtime, it reaches no queue. Evidence for the run order: the game's main gameplay update runs the colour
/// effects, Samus's input handling (which fires her weapons), the projectiles' hits on enemies, the enemies, Samus's
/// movement and animation, the enemy projectiles, the room objects, the enemy projectiles' hits, the power bomb's hits,
/// the drawing and the HUD in turn, and each of them calls the game's sound and music queue routines at the point it runs;
/// none holds a request back for a later caller.
/// </remarks>
internal sealed class GameplayAudioRequests
{
    /// <summary>The session's queues the requests go on into, or none before the record is attached to a session's.</summary>
    private IGameplayAudioQueues? queues;

    /// <summary>The update's sound requests, in request order.</summary>
    private readonly List<SoundRequest> soundRequests = [];

    /// <summary>The update's music requests, in request order.</summary>
    private readonly List<GameplayMusicRequest> musicRequests = [];

    /// <summary>
    /// Sends every later request straight on into <paramref name="sessionQueues"/>, the queues of the session running the
    /// record's owner.
    /// </summary>
    /// <exception cref="InvalidOperationException">The record already reaches a session's queues.</exception>
    internal void AttachTo(IGameplayAudioQueues sessionQueues)
    {
        Ensure.NotNull(sessionQueues);
        if (queues is not null)
            throw new InvalidOperationException("The audio requests already reach a session's queues.");
        queues = sessionQueues;
    }

    /// <summary>The update's sound requests, in the order the sound queues receive them.</summary>
    internal IReadOnlyList<SoundRequest> SoundRequests => soundRequests;

    /// <summary>The update's music requests, in the order the music queue receives them.</summary>
    internal IReadOnlyList<GameplayMusicRequest> MusicRequests => musicRequests;

    /// <summary>Begins an update: the previous update's requests, which their queues already hold, leave the record.</summary>
    internal void BeginUpdate()
    {
        soundRequests.Clear();
        musicRequests.Clear();
    }

    /// <summary>
    /// Requests <paramref name="request"/>'s sound now, after the update's earlier sound requests, returning what its queue
    /// did with it, or none when the record reaches no queue.
    /// </summary>
    internal SoundQueueAdmission? QueueSound(SoundRequest request)
    {
        soundRequests.Add(request);
        return queues?.QueueSound(request);
    }

    /// <summary>Requests <paramref name="soundEffect"/> now (see <see cref="QueueSound(SoundRequest)"/>).</summary>
    /// <param name="soundEffect">The sound.</param>
    /// <param name="priority">How full its queue may be for the request to join it.</param>
    /// <param name="soundSuppressed">Whether sounds were suppressed when the request was made.</param>
    internal SoundQueueAdmission? QueueSound(SoundEffectId soundEffect, SoundRequestPriority priority, bool soundSuppressed = false) =>
        QueueSound(new SoundRequest(soundEffect, priority, soundSuppressed));

    /// <summary>Schedules <paramref name="request"/> now, after the update's earlier music requests.</summary>
    internal void ScheduleMusic(DelayedMusicRequest request) => QueueMusic(new GameplayMusicRequest.Scheduled(request));

    /// <summary>Starts the permanent-item fanfare's music sequence now, after the update's earlier music requests.</summary>
    internal void QueuePermanentItemFanfare() => QueueMusic(new GameplayMusicRequest.PermanentItemFanfare());

    /// <summary>Records <paramref name="request"/> and puts it on the session's music queue.</summary>
    private void QueueMusic(GameplayMusicRequest request)
    {
        musicRequests.Add(request);
        queues?.QueueMusic(request);
    }
}
