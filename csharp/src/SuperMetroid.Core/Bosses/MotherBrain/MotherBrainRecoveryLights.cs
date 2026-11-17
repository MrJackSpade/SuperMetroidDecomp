namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One of the seven images of Mother Brain's room lights recovering after the Baby's death (#2259).</summary>
public readonly record struct MotherBrainRecoveryLights
{
    /// <summary>The image at <paramref name="step"/> of the recovery.</summary>
    private MotherBrainRecoveryLights(int step) => Step = step;

    /// <summary>The image's place in the recovery, from zero: the step of its installed shade.</summary>
    public int Step { get; }

    /// <summary>Every image of the recovery, in order.</summary>
    public static IReadOnlyList<MotherBrainRecoveryLights> All { get; } =
        [.. Enumerable.Range(0, MotherBrainRoomColorDefinitions.RecoveryLightsFrames).Select(step => new MotherBrainRecoveryLights(step))];

    /// <summary>The image at <paramref name="step"/> of the recovery, which its cutscene's step names.</summary>
    /// <exception cref="InvalidDataException">The step is not one of the seven authored images.</exception>
    public static MotherBrainRecoveryLights At(int step) => step is >= 0 and < MotherBrainRoomColorDefinitions.RecoveryLightsFrames
        ? All[step]
        : throw new InvalidDataException($"Mother Brain room-light recovery frame {step} is not authored.");
}
