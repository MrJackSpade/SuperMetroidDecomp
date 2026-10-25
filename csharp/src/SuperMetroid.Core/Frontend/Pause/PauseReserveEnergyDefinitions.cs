namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>How the equipment screen shows and refills reserve energy: the tank gauge's steps and the manual refill's sound.</summary>
internal static class PauseReserveEnergyDefinitions
{
    /// <summary>The reserve control the cursor starts on and returns to after a manual refill: the AUTO/MANUAL mode switch.</summary>
    internal const int ModeItem = (int)PauseReserveItem.Mode;
    /// <summary>Reserve energy one tank of the gauge holds.</summary>
    internal const int EnergyPerTank = 100;
    /// <summary>Reserve energy each of a partly filled tank's seven fill steps stands for.</summary>
    internal const int EnergyPerFillStep = 14;
    /// <summary>A partly filled tank showing fewer whole steps than this flickers its next step while that step is partly filled.</summary>
    internal const int FlickerFillStepLimit = 4;
    /// <summary>Updates the flickering step stays shown, then stays hidden, in turn.</summary>
    internal const int FlickerPhaseFrames = 4;
    /// <summary>Refill updates between refill sounds; the countdown starts at the reserve energy rounded up to a whole interval.</summary>
    internal const int RefillSoundInterval = 8;
    /// <summary>The sound each refill interval plays.</summary>
    internal static readonly SoundEffectId RefillSound = Library3Sound.IncrementalEnergy;
    /// <summary>The queue limit the refill sound is requested under.</summary>
    internal static readonly SoundQueueLimit MaximumQueuedSounds = SoundQueueLimit.Max6;
}
