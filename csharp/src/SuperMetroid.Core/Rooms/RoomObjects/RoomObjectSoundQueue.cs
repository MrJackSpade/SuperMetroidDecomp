namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How the room objects queue their sound and music requests. A request joins the update's <see cref="GameplayAudioRequests"/> as the room object makes it, after every
/// request made before it by any system, as the room objects call the game's sound queue directly.
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
    internal void Queue(SoundEffectId soundEffect, SoundRequestPriority Priority) => Requests.QueueSound(soundEffect, Priority);

    /// <summary>Schedules <paramref name="request"/>'s music now, after the update's earlier music requests.</summary>
    internal void ScheduleMusic(DelayedMusicRequest request) => Requests.ScheduleMusic(request);

    /// <summary>Starts the permanent-item fanfare's music sequence now, after the update's earlier music requests.</summary>
    internal void QueuePermanentItemFanfare() => Requests.QueuePermanentItemFanfare();
}
