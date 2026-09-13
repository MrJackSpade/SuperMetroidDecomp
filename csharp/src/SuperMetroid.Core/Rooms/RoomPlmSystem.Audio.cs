using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    [NonSerialized]
    private SamusPowerBombExplosionState? _audioPowerBomb;

    /// <summary>Reconnects the live sound guard before collision setup or instruction execution.</summary>
    public void BindPowerBombAudio(SamusPowerBombExplosionState powerBomb) => _audioPowerBomb = powerBomb;

    // Native QueueSfx checks the explosion flag at the call, not when the host
    // publishes the batch. In particular, pending gate sounds must retain this value.
    private PlmSoundRequest CreateSoundRequest(SoundEffectId soundEffect, byte MaximumQueued) =>
        new(soundEffect, MaximumQueued, SoundSuppressed: _audioPowerBomb?.IsActive == true);
}
