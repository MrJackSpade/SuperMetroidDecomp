using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy frame's bank-$80 sound and music queue calls, in publication order, plus the room-load
/// initialization music hand-off. Sounds published while a power bomb explodes carry the native guard.
/// </summary>
internal sealed class EnemyAudioQueue
{
    /// <summary>Append-only per-frame sound calls so multiple enemy actors retain publication order.</summary>
    private readonly List<SoundRequest> _soundRequests = [];
    /// <summary>Append-only per-frame music calls from enemy AI.</summary>
    private readonly List<DelayedMusicRequest> _musicRequests = [];

    /// <summary>
    /// The power-bomb explosion bound for the current enemy frame. Runtime frame bindings are not duplicate
    /// serialized cartridge owners: EnemyMain refreshes this before AI; later draw and room-main requests
    /// sample the same owner.
    /// </summary>
    [field: NonSerialized]
    internal SamusPowerBombExplosionState? PowerBomb { get; set; }

    /// <summary>Begins the native EnemyMain publication window: the previous frame's calls are discarded.</summary>
    internal void BeginFrame()
    {
        _soundRequests.Clear();
        _musicRequests.Clear();
    }

    /// <summary>Records one enemy-owned <c>QueueMusic_Delayed*</c> call.</summary>
    internal void QueueMusic(DelayedMusicRequest request) => _musicRequests.Add(request);

    /// <summary>
    /// Exact bank-$A0/$A6/$A9 sound-queue calls published by the current enemy frame.
    /// The frontend drains these only after all enemy AI and draw instructions have run.
    /// </summary>
    public IReadOnlyList<SoundRequest> SoundRequests => _soundRequests;

    /// <summary>Exact music-queue calls published by the current enemy frame.</summary>
    public IReadOnlyList<DelayedMusicRequest> MusicRequests => _musicRequests;

    /// <summary>Transient room-load music calls consumed in the same update outside EnemyMain's frame window.</summary>
    /// <remarks>The requests do not survive an update boundary or enter debugger snapshots.</remarks>
    [NonSerialized] private List<MusicRequest>? _initializationMusicDelayed8;

    /// <summary>
    /// Records an enemy initialization's <c>QueueMusicDataOrTrack_8FrameDelay</c> ($80:8FC1)
    /// call, which room loading performs after queuing the room's own music data.
    /// </summary>
    internal void QueueInitializationMusicDelayed8(MusicRequest request) =>
        (_initializationMusicDelayed8 ??= []).Add(request);

    /// <summary>Returns and clears the initialization music calls of the room just loaded.</summary>
    internal IReadOnlyList<MusicRequest> ConsumeInitializationMusicDelayed8()
    {
        if (_initializationMusicDelayed8 is not { Count: > 0 } pending)
            return [];
        MusicRequest[] requests = [.. pending];
        pending.Clear();
        return requests;
    }

    /// <summary>
    /// Records an already-decoded cartridge queue call without assigning host-side meaning
    /// to its numeric sound ID. The bank-$80 queue remains the sole owner of arbitration.
    /// </summary>
    internal void QueueEnemySound(SoundEffectId soundEffect, SoundQueueLimit maximumQueued) =>
        _soundRequests.Add(new SoundRequest(soundEffect, maximumQueued,
            SoundSuppressed: PowerBomb?.IsActive == true));

    /// <summary>Queues <paramref name="soundEffect"/> when an actor produced one this update.</summary>
    internal void QueueEnemySoundIfAny(SoundEffectId? soundEffect, SoundQueueLimit maximumQueued)
    {
        if (soundEffect is SoundEffectId value)
            QueueEnemySound(value, maximumQueued);
    }

    /// <summary>Retains a music request only when both the request and its delay are present.</summary>
    internal void QueueLegacyMusic(MusicRequest? request, MusicQueueDelay? delay)
    {
        if (request is MusicRequest value && delay is MusicQueueDelay wait)
            _musicRequests.Add(new DelayedMusicRequest(value, wait));
    }
}
