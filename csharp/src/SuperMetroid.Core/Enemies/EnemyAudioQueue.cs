using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How the enemies queue their sound and music requests, plus the music the room's enemies start when the room loads.
/// Sounds requested while a power bomb explodes are marked suppressed.
/// </summary>
/// <remarks>
/// Every enemy, boss, enemy projectile and drop queues its sound or music the moment it makes the request, as the game
/// does: the request joins the update's <see cref="GameplayAudioRequests"/> after every request made before it, by an
/// enemy or by any other system, so the enemies' requests arrive in the order the enemies run, and two requests of one
/// enemy or species in the same update are both queued. Nothing is held for the end of the enemy pass or merged with
/// another request. The sound queues admit requests by how full they already are, so this order decides which requests a
/// full queue drops.
/// </remarks>
internal sealed class EnemyAudioQueue
{
    /// <summary>
    /// The update's audio requests the enemies' requests join: the runtime's, once it points the room's systems at it; the
    /// enemies' own record before that.
    /// </summary>
    internal GameplayAudioRequests Requests { get; private set; } = new();

    /// <summary>
    /// The power-bomb explosion bound for the current enemy frame: the enemy pass refreshes this before AI; later draw and
    /// room-main requests sample the same owner.
    /// </summary>
    internal SamusPowerBombExplosionState? PowerBomb { get; set; }

    /// <summary>Points the enemies' requests at <paramref name="requests"/>, the update's audio requests of the runtime running them.</summary>
    internal void BindAudioRequests(GameplayAudioRequests requests) => Requests = Ensure.NotNull(requests);

    /// <summary>The music the loading room's enemies start, consumed in the same update outside the enemy pass.</summary>
    /// <remarks>The requests do not outlive the update that makes them, so a saved game state never holds any.</remarks>
    private List<MusicRequest>? _roomEnemyStartMusic;

    /// <summary>
    /// Requests music an enemy's initialization starts as its room loads. Room loading plays it after the room's own
    /// music, with the shortest music delay, and drops it when the music queue is full.
    /// </summary>
    internal void RequestRoomEnemyStartMusic(MusicRequest request) =>
        (_roomEnemyStartMusic ??= []).Add(request);

    /// <summary>Returns and clears the music the enemies of the room just loaded start, in request order.</summary>
    internal IReadOnlyList<MusicRequest> ConsumeRoomEnemyStartMusic()
    {
        if (_roomEnemyStartMusic is not { Count: > 0 } pending)
            return [];
        MusicRequest[] requests = [.. pending];
        pending.Clear();
        return requests;
    }

    /// <summary>Queues <paramref name="soundEffect"/> now, after the update's earlier sound requests.</summary>
    internal void QueueEnemySound(SoundEffectId soundEffect, SoundRequestPriority priority) =>
        Requests.QueueSound(new SoundRequest(soundEffect, priority,
            SoundSuppressed: PowerBomb?.IsActive == true));

    /// <summary>Queues <paramref name="request"/> now, after the update's earlier music requests.</summary>
    internal void QueueEnemyMusic(DelayedMusicRequest request) => Requests.ScheduleMusic(request);
}
