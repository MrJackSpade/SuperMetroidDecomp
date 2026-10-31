using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The sound and music requests an enemy pass makes, in the order the audio queues receive them, plus the music the
/// room's enemies start when the room loads. Sounds requested while a power bomb explodes are marked suppressed.
/// </summary>
/// <remarks>
/// <para>
/// Most producers request immediately (<see cref="QueueEnemySound"/>), in the order they run. The sources named by
/// <see cref="EnemySoundSource"/> and <see cref="EnemyMusicSource"/> instead hold their request until the enemy pass
/// ends, and the room enemy system then publishes them in one fixed source order, after every immediate request of the
/// pass. This end-of-update rule is gameplay-observable: the sound queues admit requests by how full they already are,
/// so which requests come first decides which are dropped, and the Speed Booster's stage selection and the sound
/// acceptance clock read the outcome.
/// </para>
/// <para>
/// A source holds one sound per update: its latest request replaces an earlier one, so two requests of one species in
/// the same pass produce one sound, the later. The rule and the fixed order reproduce the request sequence the port
/// produced before the sources were named (#1478), which the recorded-movie replays (Ridley, 13% and 100%) exercise;
/// they are kept as they are, not re-derived per species.
/// </para>
/// </remarks>
internal sealed class EnemyAudioQueue
{
    /// <summary>The pass's published sound requests, in request order.</summary>
    private readonly List<SoundRequest> _soundRequests = [];
    /// <summary>The pass's published music requests, in request order.</summary>
    private readonly List<DelayedMusicRequest> _musicRequests = [];
    /// <summary>Each end-of-update source's pending sound, its latest request.</summary>
    private readonly Dictionary<EnemySoundSource, SoundRequest> _pendingSounds = [];
    /// <summary>Each end-of-update source's pending music requests, in request order.</summary>
    private readonly Dictionary<EnemyMusicSource, List<DelayedMusicRequest>> _pendingMusic = [];

    /// <summary>
    /// The power-bomb explosion bound for the current enemy frame. Runtime frame bindings are not duplicate
    /// serialized owners: EnemyMain refreshes this before AI; later draw and room-main requests
    /// sample the same owner.
    /// </summary>
    [field: NonSerialized]
    internal SamusPowerBombExplosionState? PowerBomb { get; set; }

    /// <summary>
    /// Begins an enemy pass: the previous pass's published requests and every pending end-of-update request are dropped,
    /// except the drop and death sounds (<see cref="EnemySoundSource.EnemyPickup"/>,
    /// <see cref="EnemySoundSource.EnemyDeath"/>), which the enemy-projectile pass cancels instead
    /// (<see cref="CancelSoundAtUpdateEnd"/>).
    /// </summary>
    internal void BeginFrame()
    {
        _soundRequests.Clear();
        _musicRequests.Clear();
        // The drop and death sounds come from the enemy-projectile pass, which runs after the enemy pass, so they are
        // published at the end of the following enemy pass and stay pending until the next projectile pass begins.
        bool hasPickup = _pendingSounds.TryGetValue(EnemySoundSource.EnemyPickup, out SoundRequest pickup);
        bool hasDeath = _pendingSounds.TryGetValue(EnemySoundSource.EnemyDeath, out SoundRequest death);
        _pendingSounds.Clear();
        if (hasPickup)
            _pendingSounds[EnemySoundSource.EnemyPickup] = pickup;
        if (hasDeath)
            _pendingSounds[EnemySoundSource.EnemyDeath] = death;
        _pendingMusic.Clear();
    }

    /// <summary>
    /// The sound requests of the current enemy pass, in the order the sound queues receive them.
    /// The frontend drains these only after all enemy AI and draw instructions have run.
    /// </summary>
    public IReadOnlyList<SoundRequest> SoundRequests => _soundRequests;

    /// <summary>The music requests of the current enemy pass, in the order the music queue receives them.</summary>
    public IReadOnlyList<DelayedMusicRequest> MusicRequests => _musicRequests;

    /// <summary>The music the loading room's enemies start, consumed in the same update outside the enemy pass.</summary>
    /// <remarks>The requests do not survive an update boundary or enter debugger snapshots.</remarks>
    [NonSerialized] private List<MusicRequest>? _roomEnemyStartMusic;

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

    /// <summary>Requests <paramref name="soundEffect"/> immediately, after the pass's earlier sound requests.</summary>
    internal void QueueEnemySound(SoundEffectId soundEffect, SoundRequestPriority priority) =>
        _soundRequests.Add(new SoundRequest(soundEffect, priority,
            SoundSuppressed: PowerBomb?.IsActive == true));

    /// <summary>
    /// Requests <paramref name="soundEffect"/> from <paramref name="source"/> at the end of the enemy pass. The source's
    /// latest request this update replaces its earlier one.
    /// </summary>
    internal void RequestSoundAtUpdateEnd(EnemySoundSource source, SoundEffectId soundEffect, SoundRequestPriority priority) =>
        _pendingSounds[source] = new SoundRequest(soundEffect, priority);

    /// <summary>Drops <paramref name="source"/>'s pending sound for this update; nothing happens when none is pending.</summary>
    internal void CancelSoundAtUpdateEnd(EnemySoundSource source) => _pendingSounds.Remove(source);

    /// <summary>
    /// Requests <paramref name="request"/> from <paramref name="source"/> at the end of the enemy pass. The source's
    /// latest request this update replaces its earlier ones.
    /// </summary>
    internal void RequestMusicAtUpdateEnd(EnemyMusicSource source, DelayedMusicRequest request) =>
        _pendingMusic[source] = [request];

    /// <summary>
    /// Adds <paramref name="request"/> to <paramref name="source"/>'s music at the end of the enemy pass, after its
    /// earlier requests this update: a source that plays a sequence keeps every request.
    /// </summary>
    internal void AppendMusicAtUpdateEnd(EnemyMusicSource source, DelayedMusicRequest request)
    {
        if (!_pendingMusic.TryGetValue(source, out List<DelayedMusicRequest>? requests))
            _pendingMusic[source] = requests = [];
        requests.Add(request);
    }

    /// <summary>
    /// Publishes <paramref name="source"/>'s pending sound after the pass's earlier requests, marked suppressed when a
    /// power bomb explodes now. The request stays pending until its source's lifetime ends.
    /// </summary>
    internal void PublishSoundsFrom(EnemySoundSource source)
    {
        if (_pendingSounds.TryGetValue(source, out SoundRequest request))
            _soundRequests.Add(request with { SoundSuppressed = PowerBomb?.IsActive == true });
    }

    /// <summary>Publishes <paramref name="source"/>'s pending music after the pass's earlier music requests.</summary>
    internal void PublishMusicFrom(EnemyMusicSource source)
    {
        if (_pendingMusic.TryGetValue(source, out List<DelayedMusicRequest>? requests))
            _musicRequests.AddRange(requests);
    }
}
