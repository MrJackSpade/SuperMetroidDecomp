using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Which Speed Booster echo slot the next capture fills, whether the echoes are departing, or what a shinespark crash
/// left there. Ordinary boosting alternates the first and second slots; cancelling the boost sends the echoes departing,
/// which stops capture.
/// </summary>
/// <remarks>
/// The shinespark crash shares this cursor with the echoes: while its echoes orbit it holds the crash's orbit stage and
/// radius. A crash that finishes resets the cursor to the first slot, but one left unfinished leaves its stage and
/// radius there, and the next boost reads any of them but the crash's very first, the expanding stage at radius zero,
/// as the second slot: it captures into the second slot first, then returns to the first. The radius steps four pixels
/// at a time, so it never reads as the first slot partway through a step.
/// </remarks>
public readonly record struct SpeedEchoCursor
{
    /// <summary>The largest crash orbit radius the cursor holds, in pixels.</summary>
    private const int LargestCrashRadius = 255;

    /// <summary>The pixels a crash orbit radius steps by.</summary>
    private const int CrashRadiusStep = 4;

    /// <summary>Holds a checked cursor.</summary>
    private SpeedEchoCursor(SpeedEchoCursorState state, ShinesparkCrashOrbitStage crashStage, int crashRadius)
    {
        State = state;
        CrashStage = crashStage;
        CrashRadius = crashRadius;
    }

    /// <summary>What the cursor holds.</summary>
    public SpeedEchoCursorState State { get; }

    /// <summary>The orbit stage of the crash the cursor holds; the expanding stage unless <see cref="State"/> is the crash orbit.</summary>
    public ShinesparkCrashOrbitStage CrashStage { get; }

    /// <summary>The orbit radius of the crash the cursor holds, in pixels; zero unless <see cref="State"/> is the crash orbit.</summary>
    public int CrashRadius { get; }

    /// <summary>The next capture fills the first slot; the cursor of a Samus who has not boosted.</summary>
    public static SpeedEchoCursor FirstSlot => default;

    /// <summary>The next capture fills the second slot.</summary>
    public static SpeedEchoCursor SecondSlot { get; } = new(SpeedEchoCursorState.SecondSlot, ShinesparkCrashOrbitStage.Expanding, 0);

    /// <summary>The echoes are departing toward Samus; nothing is captured until they have gone.</summary>
    public static SpeedEchoCursor Departing { get; } = new(SpeedEchoCursorState.Departing, ShinesparkCrashOrbitStage.Expanding, 0);

    /// <summary>
    /// The cursor a crash leaves at <paramref name="stage"/> and <paramref name="radius"/>. The crash's first, the
    /// expanding stage at radius zero, is the first slot itself.
    /// </summary>
    /// <param name="stage">The crash's orbit stage.</param>
    /// <param name="radius">The orbit radius, 0 through 255 in steps of four; other values are rejected.</param>
    public static SpeedEchoCursor CrashOrbit(ShinesparkCrashOrbitStage stage, int radius)
    {
        if (!Enum.IsDefined(stage))
            throw new ArgumentOutOfRangeException(nameof(stage), stage, "Undefined shinespark crash orbit stage.");
        if (Ensure.BetweenInclusive(0, LargestCrashRadius, radius) % CrashRadiusStep != 0)
            throw new ArgumentOutOfRangeException(nameof(radius), radius, "A crash orbit radius steps four pixels at a time.");
        return stage == ShinesparkCrashOrbitStage.Expanding && radius == 0
            ? FirstSlot
            : new(SpeedEchoCursorState.CrashOrbit, stage, radius);
    }

    /// <summary>Whether the echoes are departing, which stops capture.</summary>
    public bool IsDeparting => State == SpeedEchoCursorState.Departing;

    /// <summary>Whether the next capture fills the first slot; any other cursor that captures fills the second.</summary>
    public bool CapturesFirstSlot => State == SpeedEchoCursorState.FirstSlot;

    /// <summary>The cursor after a capture: the first slot's capture moves it to the second, and every other to the first.</summary>
    /// <exception cref="InvalidOperationException">The echoes are departing, and nothing is captured.</exception>
    public SpeedEchoCursor AfterCapture() => State switch
    {
        SpeedEchoCursorState.FirstSlot => SecondSlot,
        SpeedEchoCursorState.SecondSlot or SpeedEchoCursorState.CrashOrbit => FirstSlot,
        SpeedEchoCursorState.Departing => throw new InvalidOperationException("Departing echoes are not captured."),
        _ => throw new InvalidOperationException($"Undefined {nameof(SpeedEchoCursorState)} {State}."),
    };
}
