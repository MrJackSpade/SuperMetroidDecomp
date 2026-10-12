using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>PLM sound requests for the frame, each stamped with the power-bomb guard at the moment the PLM queued it.</summary>
internal sealed class PlmSoundQueue
{
    internal readonly List<PlmSoundRequest> _soundRequests = new();

    /// <summary>Sound commands emitted during the most recent handler pass.</summary>
    public IReadOnlyList<PlmSoundRequest> SoundRequests => _soundRequests;

    [NonSerialized]
    private SamusPowerBombExplosionState? _audioPowerBomb;

    /// <summary>Reconnects the live sound guard before collision setup or instruction execution.</summary>
    public void BindPowerBombAudio(SamusPowerBombExplosionState powerBomb) => _audioPowerBomb = powerBomb;

    // Native QueueSfx checks the explosion flag at the call, not when the host
    // publishes the batch. In particular, pending gate sounds must retain this value.
    internal PlmSoundRequest CreateSoundRequest(SoundEffectId soundEffect, byte MaximumQueued) =>
        new(soundEffect, MaximumQueued, SoundSuppressed: _audioPowerBomb?.IsActive == true);
}
