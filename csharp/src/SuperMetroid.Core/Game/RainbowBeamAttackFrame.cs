using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The frame counters, RNG and escape state one rainbow-beam attack step reads (#633).</summary>
public sealed record RainbowBeamAttackFrame
{
    /// <summary>Enemy frame counter.</summary>
    public required ushort EnemyFrameCounter { get; init; }

    /// <summary>Main enemy execution counter.</summary>
    public required ushort MainEnemyExecutionCounter { get; init; }

    /// <summary>Power bomb active.</summary>
    public bool PowerBombActive { get; init; } = false;

    /// <summary>Random number seed.</summary>
    public ushort RandomNumberSeed { get; init; } = 0;

    /// <summary>Next random number.</summary>
    public Func<ushort>? NextRandomNumber { get; init; } = null;

    /// <summary>Alternate escape text.</summary>
    public bool AlternateEscapeText { get; init; } = false;

    /// <summary>Typewriter finished.</summary>
    public bool TypewriterFinished { get; init; } = false;

    /// <summary>Global earthquake timer.</summary>
    public ushort? GlobalEarthquakeTimer { get; init; } = null;
}
