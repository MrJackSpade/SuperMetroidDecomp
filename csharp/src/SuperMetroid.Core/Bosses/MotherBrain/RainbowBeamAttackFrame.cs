namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The frame counters, RNG and escape state one rainbow-beam attack step reads (#633).</summary>
public sealed record RainbowBeamAttackFrame
{
    /// <summary>The enemy frame counter's current count; its place in a short repeating cycle paces the beam's colour cadence.</summary>
    public required int EnemyFrameCounter { get; init; }

    /// <summary>The main enemy pass counter's current count; its place in a short repeating cycle paces ammunition drain and corpse dust sounds.</summary>
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

    /// <summary>Whether the room's earthquake has stopped as this step observes it; false unless supplied.</summary>
    public bool RoomEarthquakeStopped { get; init; } = false;

    /// <summary>The session's record of what the beam's push hides from the camera, which every push writes.</summary>
    public required RainbowBeamCameraCheckpoint CameraCheckpoint { get; init; }
}
