using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

/// <summary>LowEnergyCheck ($90:EA7F) and its persistent $0A6A critical-energy latch.</summary>
public sealed class SamusHealthWarningState
{
    /// <summary>The native latch, not confirmation that an SPC voice actually started.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Executes one admitted native check. The caller owns handler admission and ordering;
    /// frozen gameplay alone is not a reason to omit the automatic-reserve external call.
    /// </summary>
    public void Update(ushort health, CartridgeAudioState audio, bool soundSuppressed = false)
    {
        ArgumentNullException.ThrowIfNull(audio);
        bool critical = unchecked((short)(health - SamusHealthWarningRomData.HealthyThreshold)) < 0;
        if (critical == IsActive) return;

        // Native mutates the latch regardless of queue admission. In particular a
        // Power Bomb can suppress the sound while retaining this transition, and a
        // later call at unchanged health must not invent a retry of the lost request.
        if (!critical) IsActive = false;
        audio.QueueSoundAndGetAccumulator(critical ? SamusHealthWarningRomData.Start : SamusHealthWarningRomData.Stop,
            SamusHealthWarningRomData.MaximumQueuedSounds, soundSuppressed);
        if (critical) IsActive = true;
    }
}
