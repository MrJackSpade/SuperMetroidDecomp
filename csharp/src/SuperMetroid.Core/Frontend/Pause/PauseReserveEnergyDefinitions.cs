namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>How the equipment screen shows and refills reserve energy: the tank gauge's steps and the manual refill's sound.</summary>
internal static partial class PauseReserveEnergyDefinitions
{
    /// <summary>The supply digits' number base: each digit place counts ten of the place after it.</summary>
    internal const int DecimalBase = 10;
    /// <summary>The place value of the supply's leading digit.</summary>
    internal const int Hundred = DecimalBase * DecimalBase;
    /// <summary>Reserve energy each of a partly filled tank's seven fill steps stands for.</summary>
    internal const int EnergyPerFillStep = 14;
    /// <summary>A partly filled tank showing fewer whole steps than this flickers its next step while that step is partly filled.</summary>
    internal const int FlickerFillStepLimit = 4;
    /// <summary>Refill updates between refill sounds; the countdown starts at the reserve energy rounded up to a whole interval.</summary>
    internal const int RefillSoundInterval = 8;
    /// <summary>
    /// The updates left of a transfer's sound cadence on which the refill sound plays: each that starts one of its
    /// <see cref="RefillSoundInterval"/>-update intervals, and a cadence of none on its only update.
    /// </summary>
    internal static readonly UpdateCadence RefillSoundStartsInterval = new(RefillSoundInterval, 0);
    /// <summary>The sound each refill interval plays.</summary>
    internal static readonly SoundEffectId RefillSound = RoomSound.IncrementalEnergy;
    /// <summary>How crowded the sound queue may be for a refill sound to join it: the usual priority.</summary>
    internal static readonly SoundRequestPriority RefillSoundPriority = SoundRequestPriority.Normal;
}
