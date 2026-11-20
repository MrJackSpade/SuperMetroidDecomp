namespace SuperMetroid.Core.Fx;

/// <summary>How far and how fast a tidal liquid surface rises and falls (#1415), one motion per tide (#2557).</summary>
public static class LiquidTideDefinitions
{
    /// <summary>
    /// A small tide: the surface swings up to 8 pixels either side of its base line, turning a step and an eighth each update
    /// while at or below it and 3/4 of a step while above.
    /// </summary>
    public static LiquidTideMotion Small { get; } = new(
        PeakPixels: 8,
        AtOrBelowPhaseDelta: RotationAngle.FromStepFraction(9, 8),
        AbovePhaseDelta: RotationAngle.FromStepFraction(3, 4));

    /// <summary>
    /// A large tide: the surface swings up to 32 pixels either side of its base line, turning 7/8 of a step each update while
    /// at or below it and half a step while above.
    /// </summary>
    public static LiquidTideMotion Large { get; } = new(
        PeakPixels: 32,
        AtOrBelowPhaseDelta: RotationAngle.FromStepFraction(7, 8),
        AbovePhaseDelta: RotationAngle.FromStepFraction(1, 2));

    /// <summary>Each moving tide's motion; a surface with no tide stays on its base line and has none.</summary>
    public static IReadOnlyDictionary<LiquidTide, LiquidTideMotion> Motions { get; } = new Dictionary<LiquidTide, LiquidTideMotion>
    {
        [LiquidTide.Small] = Small,
        [LiquidTide.Large] = Large,
    };
}
