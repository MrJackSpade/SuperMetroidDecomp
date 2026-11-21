namespace SuperMetroid.Core.Fx;

/// <summary>How far and how fast a tidal liquid surface rises and falls (#1415), one motion per tide (#2557).</summary>
public static class LiquidTideDefinitions
{
    /// <summary>
    /// Each moving tide's motion; a surface with no tide stays on its base line and has none. A small tide swings the surface
    /// up to 8 pixels either side of its base line, turning a step and an eighth each update while at or below it and 3/4 of
    /// a step while above; a large tide up to 32 pixels, turning 7/8 of a step while at or below it and half a step above.
    /// </summary>
    public static IReadOnlyDictionary<LiquidTide, LiquidTideMotion> Motions { get; } = new Dictionary<LiquidTide, LiquidTideMotion>
    {
        [LiquidTide.Small] = new(PeakPixels: 8, AtOrBelowPhaseDelta: RotationAngle.FromStepFraction(9, 8), AbovePhaseDelta: RotationAngle.FromStepFraction(3, 4)),
        [LiquidTide.Large] = new(PeakPixels: 32, AtOrBelowPhaseDelta: RotationAngle.FromStepFraction(7, 8), AbovePhaseDelta: RotationAngle.FromStepFraction(1, 2)),
    };
}
