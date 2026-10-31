namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The low-energy check and its persistent critical-energy latch.</summary>
public sealed class SamusHealthWarningState
{
    /// <summary>Energy at or above this is healthy; below it the critical-energy warning sounds.</summary>
    private const int HealthyThreshold = 31;

    /// <summary>The request priority of the warning's start and stop sounds.</summary>
    private static readonly SoundRequestPriority WarningSoundPriority = SoundRequestPriority.Normal;

    /// <summary>The library-three sound that starts the critical-energy warning.</summary>
    private static readonly SoundEffectId Start = Library3Sound.LowHealthBeep;

    /// <summary>The library-three sound that stops the critical-energy warning.</summary>
    private static readonly SoundEffectId Stop = Library3Sound.Silence;

    /// <summary>The latch, not confirmation that a sound voice actually started.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Executes one admitted check. The caller owns handler admission and ordering;
    /// frozen gameplay alone is not a reason to omit the automatic-reserve external call.
    /// </summary>
    public void Update(int health, AudioQueues audio, bool soundSuppressed = false)
    {
        Ensure.NotNull(audio);
        bool critical = health < HealthyThreshold;
        if (critical == IsActive) return;

        // The latch changes regardless of queue admission. In particular a
        // Power Bomb can suppress the sound while retaining this transition, and a
        // later call at unchanged health must not invent a retry of the lost request.
        if (!critical) IsActive = false;
        audio.QueueSound(critical ? Start : Stop, WarningSoundPriority, soundSuppressed);
        if (critical) IsActive = true;
    }
}
