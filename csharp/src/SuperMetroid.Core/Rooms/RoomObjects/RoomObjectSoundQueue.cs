namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How the room objects queue their sound and music requests, each sound stamped with the power-bomb guard at the moment the room object
/// queued it. A request joins the update's <see cref="GameplayAudioRequests"/> as the room object makes it, after every
/// request made before it by any system, as the room-object routines call the game's sound queue routines directly.
/// </summary>
internal sealed class RoomObjectSoundQueue
{
    /// <summary>
    /// The update's audio requests the room objects' requests join: the runtime's, once it points the room's systems at it;
    /// the room objects' own record before that.
    /// </summary>
    internal GameplayAudioRequests Requests { get; private set; } = new();

    /// <summary>Points the room objects' requests at <paramref name="requests"/>, the update's audio requests of the runtime running them.</summary>
    internal void BindAudioRequests(GameplayAudioRequests requests) => Requests = Ensure.NotNull(requests);

    /// <summary>Queues <paramref name="soundEffect"/> at <paramref name="Priority"/> now.</summary>
    internal void Queue(SoundEffectId soundEffect, SoundRequestPriority Priority) => Requests.QueueSound(CreateSoundRequest(soundEffect, Priority));

    /// <summary>Schedules <paramref name="request"/>'s music now, after the update's earlier music requests.</summary>
    internal void ScheduleMusic(DelayedMusicRequest request) => Requests.ScheduleMusic(request);

    /// <summary>Starts the permanent-item fanfare's music sequence now, after the update's earlier music requests.</summary>
    internal void QueuePermanentItemFanfare() => Requests.QueuePermanentItemFanfare();

    /// <summary>Live explosion state consulted when each request is created, then rebound after restores.</summary>
    private IActivePowerBomb? _audioPowerBomb;

    /// <summary>Reconnects the live sound guard before collision setup or instruction execution.</summary>
    public void BindPowerBombAudio(IActivePowerBomb powerBomb) => _audioPowerBomb = powerBomb;

    /// <summary>
    /// Captures the explosion guard when the room object makes the request, as the game's sound queue does; a later change
    /// to that guard cannot alter a request the room object holds until its sound is due (the save station's).
    /// </summary>
    internal SoundRequest CreateSoundRequest(SoundEffectId soundEffect, SoundRequestPriority Priority) =>
        new(soundEffect, Priority, SoundSuppressed: _audioPowerBomb?.IsActive == true);
}
