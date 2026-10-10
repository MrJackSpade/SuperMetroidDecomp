using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>Live power-bomb state sampled when room PLM sound requests capture native suppression.</summary>
    [NonSerialized]
    private SamusPowerBombExplosionState? _audioPowerBomb;

    /// <summary>Reconnects the live sound guard before collision setup or instruction execution.</summary>
    public void BindPowerBombAudio(SamusPowerBombExplosionState powerBomb) => _audioPowerBomb = powerBomb;

    // Native QueueSfx checks the explosion flag at the call, not when the host
    // publishes the batch. In particular, pending gate sounds must retain this value.
    /// <summary>Creates a PLM sound request with the power-bomb suppression state captured at request time.</summary>
    /// <param name="soundEffect">Sound effect requested by the room PLM.</param>
    /// <param name="MaximumQueued">Maximum number of instances the native sound queue may hold.</param>
    /// <returns>A request carrying the sound identity, queue limit, and current suppression decision.</returns>
    private PlmSoundRequest CreateSoundRequest(SoundEffectId soundEffect, byte MaximumQueued) =>
        new(soundEffect, MaximumQueued, SoundSuppressed: _audioPowerBomb?.IsActive == true);
}
