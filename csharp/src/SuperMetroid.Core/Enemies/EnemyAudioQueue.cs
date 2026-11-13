using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The sound and music requests the enemies make in one update, in the order the audio queues receive them, plus the
/// music the room's enemies start when the room loads. Sounds requested while a power bomb explodes are marked suppressed.
/// </summary>
/// <remarks>
/// Every enemy, boss, enemy projectile and drop queues its sound or music the moment it makes the request, as the game
/// does: each request follows the ones made before it, so the enemies' requests arrive in the order the enemies run, and
/// two requests of one enemy or species in the same update are both queued. Nothing is held for the end of the enemy pass
/// or merged with another request. The sound queues admit requests by how full they already are, so this order decides
/// which requests a full queue drops.
/// </remarks>
internal sealed class EnemyAudioQueue
{
    /// <summary>The update's sound requests, in request order.</summary>
    private readonly List<SoundRequest> _soundRequests = [];
    /// <summary>The update's music requests, in request order.</summary>
    private readonly List<DelayedMusicRequest> _musicRequests = [];

    /// <summary>
    /// The power-bomb explosion bound for the current enemy frame: the enemy pass refreshes this before AI; later draw and
    /// room-main requests sample the same owner.
    /// </summary>
    internal SamusPowerBombExplosionState? PowerBomb { get; set; }

    /// <summary>
    /// Begins an update's enemy audio: the previous update's requests, which the audio queues have already received, are
    /// dropped.
    /// </summary>
    internal void BeginFrame()
    {
        _soundRequests.Clear();
        _musicRequests.Clear();
    }

    /// <summary>
    /// The update's sound requests, in the order the sound queues receive them: the enemy pass's in the order the enemies
    /// ran, then those the enemy projectiles, their collisions and the power bomb's hits make after it.
    /// </summary>
    public IReadOnlyList<SoundRequest> SoundRequests => _soundRequests;

    /// <summary>The update's music requests, in the order the music queue receives them.</summary>
    public IReadOnlyList<DelayedMusicRequest> MusicRequests => _musicRequests;

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
        _soundRequests.Add(new SoundRequest(soundEffect, priority,
            SoundSuppressed: PowerBomb?.IsActive == true));

    /// <summary>Queues <paramref name="request"/> now, after the update's earlier music requests.</summary>
    internal void QueueEnemyMusic(DelayedMusicRequest request) => _musicRequests.Add(request);
}
