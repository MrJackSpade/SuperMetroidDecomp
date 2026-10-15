using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>PLM sound requests for the frame, each stamped with the power-bomb guard at the moment the PLM queued it.</summary>
internal sealed class PlmSoundQueue
{
    /// <summary>Retains PLM sound requests from the current handler pass until frame publication.</summary>
    internal readonly List<PlmSoundRequest> _soundRequests = new();

    /// <summary>Sound commands emitted during the most recent handler pass.</summary>
    public IReadOnlyList<PlmSoundRequest> SoundRequests => _soundRequests;

    /// <summary>Live explosion state consulted when each request is created, then rebound after restores.</summary>
    [NonSerialized]
    private SamusPowerBombExplosionState? _audioPowerBomb;

    /// <summary>Reconnects the live sound guard before collision setup or instruction execution.</summary>
    public void BindPowerBombAudio(SamusPowerBombExplosionState powerBomb) => _audioPowerBomb = powerBomb;

    /// <summary>
    /// Captures the explosion guard when the PLM queues a sound, as native QueueSfx does;
    /// a later change to that guard cannot alter the pending gate sound.
    /// </summary>
    internal PlmSoundRequest CreateSoundRequest(SoundEffectId soundEffect, byte MaximumQueued) =>
        new(soundEffect, MaximumQueued, SoundSuppressed: _audioPowerBomb?.IsActive == true);
}
