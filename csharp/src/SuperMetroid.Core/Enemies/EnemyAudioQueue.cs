namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How the enemies queue their sound and music requests, plus the music the room's enemies start when the room loads.
/// </summary>
/// <remarks>
/// Every enemy, boss, enemy projectile and drop queues its sound or music the moment it makes the request, as the game
/// does: the request joins the update's <see cref="GameplayAudioRequests"/> after every request made before it, by an
/// enemy or by any other system, so the enemies' requests arrive in the order the enemies run, and two requests of one
/// enemy or species in the same update are both queued. Nothing is held for the end of the enemy pass or merged with
/// another request. The sound queues admit requests by how full they already are, so this order decides which requests a
/// full queue drops.
/// </remarks>
internal sealed partial class EnemyAudioQueue
{
    /// <summary>Creates the room's enemy audio, whose requests join <paramref name="requests"/>, the runtime's update's (#2057).</summary>
    internal EnemyAudioQueue(GameplayAudioRequests requests) => Requests = Ensure.NotNull(requests);

    /// <summary>The update's audio requests the enemies' requests join: the runtime's.</summary>
    internal GameplayAudioRequests Requests { get; }

    /// <summary>The music the room's enemies start as it loads, in request order (#2562).</summary>
    private readonly List<MusicRequest> _roomStartMusic = [];

    /// <summary>
    /// The music the room's enemies start as it loads, in request order: a fact of the loaded room, which the frontend
    /// queues once, after the room's own music, with the shortest music delay, dropping what a full music queue refuses
    /// (#2562).
    /// </summary>
    internal IReadOnlyList<MusicRequest> RoomStartMusic => _roomStartMusic;

    /// <summary>Starts <paramref name="request"/> as the room loads, as an enemy's initialization does (<see cref="RoomStartMusic"/>).</summary>
    internal void RequestRoomEnemyStartMusic(MusicRequest request) => _roomStartMusic.Add(request);

    /// <summary>Queues <paramref name="soundEffect"/> now, after the update's earlier sound requests.</summary>
    internal void QueueEnemySound(SoundEffectId soundEffect, SoundRequestPriority priority) =>
        Requests.QueueSound(soundEffect, priority);

    /// <summary>Queues <paramref name="request"/> now, after the update's earlier music requests.</summary>
    internal void QueueEnemyMusic(DelayedMusicRequest request) => Requests.ScheduleMusic(request);
}
