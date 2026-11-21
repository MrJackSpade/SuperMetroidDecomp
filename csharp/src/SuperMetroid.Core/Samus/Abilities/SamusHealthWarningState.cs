using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The low-energy check and its persistent critical-energy latch.</summary>
public sealed class SamusHealthWarningState
{
    /// <summary>The request priority of the warning's start and stop sounds.</summary>
    private static readonly SoundRequestPriority WarningSoundPriority = SoundRequestPriority.Normal;

    /// <summary>The room-library sound that starts the critical-energy warning.</summary>
    private static readonly SoundEffectId Start = RoomSound.LowHealthBeep;

    /// <summary>The room-library sound that stops the critical-energy warning.</summary>
    private static readonly SoundEffectId Stop = RoomSoundStop.Any;

    /// <summary>The latch, not confirmation that the warning sound actually started.</summary>
    internal bool IsActive { get; set; }

    /// <summary>
    /// Executes one admitted check: starts or stops the warning sound when Samus's energy crosses the healthy threshold.
    /// Samus's update decides when it runs and in what order.
    /// </summary>
    internal void Update(int health, GameplaySoundRequests audio)
    {
        Ensure.NotNull(audio);
        bool critical = health < SamusEnergyDefinitions.HealthyEnergy;
        if (critical == IsActive) return;

        // The latch changes regardless of queue admission. In particular an exploding
        // Power Bomb turns the sound away in the queue while this transition stays, and a
        // later update at unchanged health must not invent a retry of the lost request.
        if (!critical) IsActive = false;
        audio.QueueSound(critical ? Start : Stop, WarningSoundPriority);
        if (critical) IsActive = true;
    }
}
