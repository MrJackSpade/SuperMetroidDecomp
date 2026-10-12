using SuperMetroid.Core.Audio;
namespace SuperMetroid.Core.Game;

/// <summary>
/// The enemy frame's bank-$80 sound and music queue calls, in publication order, plus the room-load
/// initialization music hand-off. Sounds published while a power bomb explodes carry the native guard.
/// </summary>
internal sealed class EnemyAudioQueue
{
    // Enemy AI can execute several independent actors in one frame. A single nullable
    // "last sound" loses calls when two actors publish during that pass, so new translations
    // use the same append-only-per-frame shape already proven by Samus and PLM audio.
    private readonly List<EnemySoundRequest> _soundRequests = [];
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

    // Enemy initialization runs while a room loads, outside EnemyMain's publication window,
    // and its frontend consumes the requests within that same update. Nothing here
    // survives an update boundary, so debugger snapshots never contain it.
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

    internal void QueueLegacySound(
        ushort? soundId,
        SoundEffectLibrary library,
        byte maximumQueued)
    {
        if (soundId is ushort value)
            QueueEnemySound(SoundEffectId.FromCartridge(library, value), maximumQueued);
    }

    internal void QueueLegacyMusic(MusicCommand? command, MusicCommandDelay? delay)
    {
        if (command is MusicCommand value && delay is MusicCommandDelay wait)
            _musicRequests.Add(new EnemyMusicRequest(value, wait));
    }
}
