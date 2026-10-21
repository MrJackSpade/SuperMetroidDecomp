namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The frame counters, RNG and escape state one rainbow-beam attack step reads (#633).</summary>
public sealed record RainbowBeamAttackFrame
{
    /// <summary>The enemy frame counter's current count; its low bits pace the beam's palette cadence.</summary>
    public required int EnemyFrameCounter { get; init; }

    /// <summary>The main enemy pass counter's current count; its low bits pace ammunition drain and corpse dust sounds.</summary>
    public required int MainEnemyExecutionCounter { get; init; }

    /// <summary>Power bomb active.</summary>
    public bool PowerBombActive { get; init; } = false;

    /// <summary>The room's random number as this step reads it without advancing the generator; zero unless supplied.</summary>
    public MotherBrainRandomSample RandomNumberSeed { get; init; } = MotherBrainRandomSample.FromGenerator(0);

    /// <summary>Next random number.</summary>
    public Func<int>? NextRandomNumber { get; init; } = null;

    /// <summary>Alternate escape text.</summary>
    public bool AlternateEscapeText { get; init; } = false;

    /// <summary>Typewriter finished.</summary>
    public bool TypewriterFinished { get; init; } = false;

    /// <summary>The global earthquake timer as this frame observes it, or null when the caller supplies none.</summary>
    public WrappingCountdown? GlobalEarthquakeTimer { get; init; } = null;
}
