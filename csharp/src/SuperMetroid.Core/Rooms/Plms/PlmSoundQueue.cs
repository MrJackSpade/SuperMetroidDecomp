namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>PLM sound requests for the frame, each stamped with the power-bomb guard at the moment the PLM queued it.</summary>
internal sealed class PlmSoundQueue
{
    /// <summary>Retains PLM sound requests from the current handler pass until frame publication.</summary>
    private readonly List<SoundRequest> _soundRequests = new();

    /// <summary>Queues <paramref name="soundEffect"/> at <paramref name="Priority"/>.</summary>
    internal void Queue(SoundEffectId soundEffect, SoundRequestPriority Priority) => _soundRequests.Add(CreateSoundRequest(soundEffect, Priority));

    /// <summary>Queues requests built earlier, in order.</summary>
    internal void QueueAll(IEnumerable<SoundRequest> requests) => _soundRequests.AddRange(requests);

    /// <summary>Frame start and room reset: drops the previous pass's requests.</summary>
    internal void Clear() => _soundRequests.Clear();

    /// <summary>Sound commands emitted during the most recent handler pass.</summary>
    public IReadOnlyList<SoundRequest> SoundRequests => _soundRequests;

    /// <summary>Live explosion state consulted when each request is created, then rebound after restores.</summary>
    [NonSerialized]
    private IActivePowerBomb? _audioPowerBomb;

    /// <summary>Reconnects the live sound guard before collision setup or instruction execution.</summary>
    public void BindPowerBombAudio(IActivePowerBomb powerBomb) => _audioPowerBomb = powerBomb;

    /// <summary>
    /// Captures the explosion guard when the PLM queues a sound, as native QueueSfx does;
    /// a later change to that guard cannot alter the pending gate sound.
    /// </summary>
    internal SoundRequest CreateSoundRequest(SoundEffectId soundEffect, SoundRequestPriority Priority) =>
        new(soundEffect, Priority, SoundSuppressed: _audioPowerBomb?.IsActive == true);
}
