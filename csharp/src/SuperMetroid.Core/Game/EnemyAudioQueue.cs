using SuperMetroid.Core.Audio;
namespace SuperMetroid.Core.Game;

/// <summary>
/// The enemy frame's bank-$80 sound and music queue calls, in publication order, plus the room-load
/// initialization music hand-off. Sounds published while a power bomb explodes carry the native guard.
/// </summary>
internal sealed class EnemyAudioQueue
{
    /// <summary>Append-only per-frame sound calls so multiple enemy actors retain publication order.</summary>
    private readonly List<EnemySoundRequest> _soundRequests = [];
    /// <summary>Append-only per-frame music calls from enemy AI.</summary>
    private readonly List<EnemyMusicRequest> _musicRequests = [];

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
    internal void QueueMusic(EnemyMusicRequest request) => _musicRequests.Add(request);

    /// <summary>
    /// Exact bank-$A0/$A6/$A9 sound-queue calls published by the current enemy frame.
    /// The frontend drains these only after all enemy AI and draw instructions have run.
    /// </summary>
    public IReadOnlyList<EnemySoundRequest> SoundRequests => _soundRequests;

    /// <summary>Exact music-queue calls published by the current enemy frame.</summary>
    public IReadOnlyList<EnemyMusicRequest> MusicRequests => _musicRequests;

    /// <summary>Transient room-load music calls consumed in the same update outside EnemyMain's frame window.</summary>
    /// <remarks>The requests do not survive an update boundary or enter debugger snapshots.</remarks>
    [NonSerialized] private List<MusicCommand>? _initializationMusicDelayed8;

    /// <summary>
    /// Records an enemy initialization's <c>QueueMusicDataOrTrack_8FrameDelay</c> ($80:8FC1)
    /// call, which room loading performs after queuing the room's own music data.
    /// </summary>
    internal void QueueInitializationMusicDelayed8(MusicCommand command) =>
        (_initializationMusicDelayed8 ??= []).Add(command);

    /// <summary>Returns and clears the initialization music calls of the room just loaded.</summary>
    internal IReadOnlyList<MusicCommand> ConsumeInitializationMusicDelayed8()
    {
        if (_initializationMusicDelayed8 is not { Count: > 0 } pending)
            return [];
        MusicCommand[] commands = [.. pending];
        pending.Clear();
        return commands;
    }

    /// <summary>
    /// Records an already-decoded cartridge queue call without assigning host-side meaning
    /// to its numeric sound ID. The bank-$80 queue remains the sole owner of arbitration.
    /// </summary>
    internal void QueueEnemySound(SoundEffectId soundEffect, byte maximumQueued)
    {
        if (maximumQueued == 0)
            throw new ArgumentOutOfRangeException(nameof(maximumQueued), maximumQueued, "Queue capacity must be nonzero.");

        _soundRequests.Add(new EnemySoundRequest(soundEffect, maximumQueued,
            SoundSuppressed: PowerBomb?.IsActive == true));
    }

    /// <summary>Converts an optional legacy sound ID into a typed enemy queue call when one is present.</summary>
    internal void QueueLegacySound(
        ushort? soundId,
        SoundEffectLibrary library,
        byte maximumQueued)
    {
        if (soundId is ushort value)
            QueueEnemySound(SoundEffectId.FromCartridge(library, value), maximumQueued);
    }

    /// <summary>Retains a legacy music request only when both command and native delay are present.</summary>
    internal void QueueLegacyMusic(MusicCommand? command, MusicCommandDelay? delay)
    {
        if (command is MusicCommand value && delay is MusicCommandDelay wait)
            _musicRequests.Add(new EnemyMusicRequest(value, wait));
    }
}
