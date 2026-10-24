namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The low-energy check and its persistent critical-energy latch.</summary>
public sealed class SamusHealthWarningState
{
    /// <summary>Energy at or above this is healthy; below it the critical-energy warning sounds.</summary>
    private const int HealthyThreshold = 31;

    /// <summary>Queued sounds at which the warning's start or stop request is refused.</summary>
    private const int MaximumQueuedSounds = 6;

    /// <summary>The library-three sound that starts the critical-energy warning.</summary>
    private static readonly SoundEffectId Start = new(SoundEffectLibrary.Library3, 2);

    /// <summary>The library-three sound that stops the critical-energy warning.</summary>
    private static readonly SoundEffectId Stop = new(SoundEffectLibrary.Library3, 1);

    /// <summary>The latch, not confirmation that a sound voice actually started.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Executes one admitted check. The caller owns handler admission and ordering;
    /// frozen gameplay alone is not a reason to omit the automatic-reserve external call.
    /// </summary>
    public void Update(int health, CartridgeAudioState audio, bool soundSuppressed = false)
    {
        Ensure.NotNull(audio);
        bool critical = health < HealthyThreshold;
        if (critical == IsActive) return;

        // The latch changes regardless of queue admission. In particular a
        // Power Bomb can suppress the sound while retaining this transition, and a
        // later call at unchanged health must not invent a retry of the lost request.
        if (!critical) IsActive = false;
        audio.QueueSoundAndGetAccumulator(critical ? Start : Stop, MaximumQueuedSounds, soundSuppressed);
        if (critical) IsActive = true;
    }
}
