namespace SuperMetroid.Core.Fx;

/// <summary>How far and how fast a tidal liquid surface rises and falls (#1415).</summary>
public static class LiquidTideDefinitions
{
    /// <summary>
    /// How far a small tide moves the surface at the wave's crest, in pixels, so the surface swings up to this far either
    /// side of its base line.
    /// </summary>
    public const int SmallTidePeakPixels = 8;

    /// <summary>
    /// How far a large tide moves the surface at the wave's crest, in pixels, so the surface swings up to this far either
    /// side of its base line.
    /// </summary>
    public const int LargeTidePeakPixels = 32;

    /// <summary>How far a small tide turns each update while its surface is at or below its base line: a step and an eighth.</summary>
    public static readonly RotationAngle SmallTidePositivePhaseDelta = RotationAngle.FromStepFraction(9, 8);

    /// <summary>How far a small tide turns each update while its surface is above its base line: 3/4 of a step.</summary>
    public static readonly RotationAngle SmallTideNegativePhaseDelta = RotationAngle.FromStepFraction(3, 4);

    /// <summary>How far a large tide turns each update while its surface is at or below its base line: 7/8 of a step.</summary>
    public static readonly RotationAngle LargeTidePositivePhaseDelta = RotationAngle.FromStepFraction(7, 8);

    /// <summary>How far a large tide turns each update while its surface is above its base line: half a step.</summary>
    public static readonly RotationAngle LargeTideNegativePhaseDelta = RotationAngle.FromStepFraction(1, 2);
}
